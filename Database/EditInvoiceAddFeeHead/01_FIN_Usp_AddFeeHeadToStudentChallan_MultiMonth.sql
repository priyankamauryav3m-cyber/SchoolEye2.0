/*
    FIN_Usp_AddFeeHeadToStudentChallan  (Edit Invoice -> Add Fee Head)

    Problem : the screen sends all selected months as dbo.TVP_MonthNo, the procedure took one month (@MonthNo int)
              -> "Operand type clash: table type is incompatible with int".
    Change  : @MonthNo is now dbo.TVP_MonthNo READONLY. Every selected month is added in ONE transaction
              (all months or none). Per month: one V3M_FIN_DetInvoice row, StudentLedger + FeeHeadLedger entries
              (and concession entries) — the same rows the single-month version wrote for its one month.
              V3M_FIN_MstInvoice totals are increased by (amount / concession) x number of months.
              Concession, round off, Ch_Status, -11 rule and the exception log are unchanged.
    Transport: the screen sends @HeadId = 0 and @HeadAmount = 0. The procedure then finds the transport fee head
              and the per-month amount (V3M_FIN_MstTransportFeeConfig for the student's RouteDistance).
    @Result : 1 added, -11 concession > fee head amount, -2 no month passed,
              -3 transport head / distance / transport amount not found, 0 error
    Caller  : StudentInvoiceService.AddFeeHeadToStudentChallanData (only caller;
              FIN_Usp_AddFeeHeadToStudentChallanReCal is a separate procedure and is not changed).
    Rollback: 01_FIN_Usp_AddFeeHeadToStudentChallan_Rollback.sql (single-month version as deployed on 27-10-2026)
*/
CREATE OR ALTER PROCEDURE [dbo].[FIN_Usp_AddFeeHeadToStudentChallan]
    @StudentId  BIGINT,
    @InvoiceId  BIGINT,
    @HeadId     INT,
    @MonthNo    dbo.TVP_MonthNo READONLY,
    @HeadAmount DECIMAL(18,2),
    @CreatedBy  VARCHAR(50),
    @Narration1 VARCHAR(200),
    @Result     INT OUTPUT
AS
DECLARE @ErrorCode int=0
DECLARE @ChallanNo varchar(20)=''     -- InvoiceNo is varchar (e.g. 0101IN1481)
DECLARE @GroupCode varchar(50)='',@BranchCode varchar(50)='',@SessionId varchar(50),@Narration varchar(400)
DECLARE @FeeHeadName varchar(50)=''
DECLARE @DisplayOrder int=0
DECLARE @tmpConcessionId int=0
DECLARE @ClassCode varchar(50)=''
DECLARE @MonthCount int=0
DECLARE @Month int

BEGIN
 ------------------------------Affected Tables-------------------------------------
 --------V3M_FIN_MstInvoice
 --------V3M_FIN_DetInvoice
 --------V3M_FIN_StudentLedger
 --------V3M_FIN_FeeHeadLedger
 ---------------------------------------------------------------
 SET NOCOUNT ON;
 SET @Result = NULL

 -- valid, distinct months only
 DECLARE @Months TABLE (MonthNo int PRIMARY KEY)
 INSERT INTO @Months(MonthNo) SELECT DISTINCT MonthNo FROM @MonthNo WHERE MonthNo BETWEEN 1 AND 12
 SELECT @MonthCount = COUNT(1) FROM @Months
 IF(@MonthCount = 0)
 BEGIN
	SET @Result = -2
	RETURN @ErrorCode
 END

 BEGIN TRY
 BEGIN TRANSACTION

	-------------------------------Getting Challan Id----------------------------------
	Select @ChallanNo=InvoiceNo,@GroupCode=GroupCode,@BranchCode=BranchCode,@SessionId=SessionId,@ClassCode=ClassCode from V3M_FIN_MstInvoice where InvoiceId=@InvoiceId

	--================================================ Transport (screen sends @HeadId = 0, @HeadAmount = 0) ================================================
	-- fee head  : transport head already on this invoice -> else the student's fee template -> else the branch transport head
	-- amount    : per month from V3M_FIN_MstTransportFeeConfig for the student's RouteDistance (same as V3M_FIN_UspGetFeeDetailsForInvoiceWithTemplate)
	if(ISNULL(@HeadId,0) = 0)
	begin
		DECLARE @DistanceId int = 0, @FeeTemplateId int = 0

		SELECT TOP 1 @FeeTemplateId = sj.FeeTemplateId, @DistanceId = ISNULL(RouteDistance, 0)
		FROM StudentJourney sj INNER JOIN StudentMaster sm ON sj.StudentID = sm.StudentId AND sj.GroupCode = sm.GroupCode AND sj.BranchCode = sm.BranchCode
		WHERE sj.GroupCode = @GroupCode AND sj.BranchCode = @BranchCode AND sj.SessionId = @SessionId AND sj.StudentId = @StudentId

		SELECT TOP 1 @HeadId = di.FeeHeadId
		FROM V3M_FIN_DetInvoice di
		INNER JOIN V3M_FIN_MstFeeHead fh ON fh.FeeHeadId = di.FeeHeadId AND fh.GroupCode = @GroupCode AND fh.BranchCode = @BranchCode
		WHERE di.InvoiceId = @InvoiceId AND di.IsValid = 1 AND fh.FeeHeadType = 'Transport'

		if(ISNULL(@HeadId,0) = 0)
			SELECT TOP 1 @HeadId = fht.FeeHeadId
			FROM V3M_FIN_DetFeeHeadMappedWithTemplate fht
			INNER JOIN V3M_FIN_MstFeeHead fh ON fh.FeeHeadId = fht.FeeHeadId AND fh.FeeHeadType = 'Transport'
			WHERE fht.FeeTemplateId = @FeeTemplateId AND fht.SessionId = @SessionId AND fht.IsValid = 1

		if(ISNULL(@HeadId,0) = 0)
			SELECT TOP 1 @HeadId = FeeHeadId FROM V3M_FIN_MstFeeHead
			WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND FeeHeadType = 'Transport'
			ORDER BY FeeHeadId

		SELECT @HeadAmount = ISNULL(SUM(Amount), 0) FROM V3M_FIN_MstTransportFeeConfig
		WHERE IsValid = 1 AND DistanceId = @DistanceId AND SessionId = @SessionId

		-- no transport head / no distance / no amount for the distance: nothing is added
		if(ISNULL(@HeadId,0) = 0 OR ISNULL(@HeadAmount,0) <= 0)
		begin
			ROLLBACK TRANSACTION
			SET @Result = -3
			RETURN @ErrorCode
		end
	end
	--=======================================================================================================================================================

	---------------------------------Getting Fee Head Name-------------------------------
	Select @FeeHeadName=FeeHeadName,@DisplayOrder=DisplayOrder from V3M_FIN_MstFeeHead where FeeHeadId=@HeadId and GroupCode=@GroupCode and BranchCode=@BranchCode

	--================================================================Checking Concession===============================================================

	Select @tmpConcessionId=ConcessionId from [dbo].[V3M_FIN_DetConcessionMapToStudent] where Status=1 and GroupCode=@GroupCode and BranchCode=@BranchCode and SessionId=@SessionId and StudentId=@StudentId

    DECLARE @ConcessionName varchar(50)='',@ConcessionType varchar(50)='',@ConcessionAmount decimal(18,2)=0
    SELECT @ConcessionName = Concession,@ConcessionType=Type  FROM V3M_FIN_MstConcession WHERE ConcessionId = @tmpConcessionId
	--Change against Ticket id 54770
	if(@ConcessionName = 'Other' or @ConcessionType ='Student')
	begin
		select @ConcessionAmount = case ConcessionType when 0 then (@HeadAmount * ConcessionValue)/100 else ConcessionValue end
		from V3M_FIN_DetMapStudentFeeHeadConcession
		where GroupCode = @GroupCode and BranchCode = @BranchCode and SessionId = @SessionId and StudentId  = @StudentId and
		ClassCode = @ClassCode and IsValid = 1 and FeeHeadId = @HeadId
	end
	else
	begin
		select @ConcessionAmount = case ConcessionType when 0 then (@HeadAmount * ConcessionApplicable)/100 else ConcessionApplicable end
		from V3M_FIN_DetCocessionMapToFeeHead where GroupCode = @GroupCode and BranchCode = @BranchCode and SessionId = @SessionId and
		ConcessionId = @tmpConcessionId and FeeHeadId = @HeadId and IsValid = 1
	end

	-------------------------------------------------------Check for Concession Round Off-----------------------------------------------------
	DECLARE @IsRoundOff int=0
	if exists(select KeyValue from DefaultSettings where GroupCode=@GroupCode and BranchCode=@BranchCode and KeyName='ConcessionRoundOff')
	begin
	select @IsRoundOff=KeyValue from DefaultSettings where GroupCode=@GroupCode and BranchCode=@BranchCode and KeyName='ConcessionRoundOff'
	if(@IsRoundOff=1)
		set @ConcessionAmount=ROUND(@ConcessionAmount,0)
	else if(@IsRoundOff=2)
		set @ConcessionAmount=FLOOR(@ConcessionAmount)
	else if(@IsRoundOff=3)
		set @ConcessionAmount=CEILING(@ConcessionAmount)
	end

	--------------------------------------------------------------------

		if(@ConcessionAmount > @HeadAmount)
		begin
			ROLLBACK TRANSACTION;
			BEGIN TRANSACTION ;

			insert into Det_ExceptionDetails(ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)
			values('Finance','ViewStudentChallan.aspx','Proc FIN_Usp_AddFeeHeadToStudentChallan','In Add Fee Head Concessoin amt can not be > Fee Head Amt. For InvoiceId='+cast(@InvoiceId as Varchar),GETDATE())

			set @Result = -11
		end
		ELSE
		BEGIN

			--------------------------------Updating V3M_FIN_MstInvoice (all months) -----------------------
			Update V3M_FIN_MstInvoice Set TotalAmount=TotalAmount+(@HeadAmount*@MonthCount),TotalConcession=TotalConcession+(@ConcessionAmount*@MonthCount)
			where InvoiceId=@InvoiceId and GroupCode=@GroupCode and BranchCode=@BranchCode

			-----------------------------------------------------updating V3M_FIN_MstInvoice Payment Status--------------------------------
			DECLARE @tmpchBalance decimal(18,2)=0
			select @tmpchBalance=ReceivedAmount from V3M_FIN_MstInvoice where InvoiceId=@InvoiceId and GroupCode=@GroupCode and BranchCode=@BranchCode

			if(@tmpchBalance>0)
			begin
				Update V3M_FIN_MstInvoice set Ch_Status=2 where InvoiceId=@InvoiceId and GroupCode=@GroupCode and BranchCode=@BranchCode
			end
			else
			begin
				Update V3M_FIN_MstInvoice set Ch_Status=1 where InvoiceId=@InvoiceId and GroupCode=@GroupCode and BranchCode=@BranchCode
			end

			--------------------------------One fee head row + ledger entries per selected month -----------------------
			DECLARE @DetInvoiceId int =0
			DECLARE month_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT MonthNo FROM @Months ORDER BY MonthNo
			OPEN month_cursor
			FETCH NEXT FROM month_cursor INTO @Month
			WHILE @@FETCH_STATUS = 0
			BEGIN
				Select @DetInvoiceId=isnull(Max(DetInvoiceId),0)+1 from V3M_FIN_DetInvoice WITH (UPDLOCK, HOLDLOCK)

				insert into V3M_FIN_DetInvoice(DetInvoiceId,InvoiceId,FeeHeadId,DisplayOrder,MonthNo,Amount,Concession,ReceivedAmt,Adjusted,WaiveOff,IsValid,Status)
				values(@DetInvoiceId,@InvoiceId,@HeadId,@DisplayOrder,@Month,@HeadAmount,@ConcessionAmount,0,0,0,1,1)
				------------------------------------------------------------------------------------------------------------------------------
				set @Narration='Added fee head = '+@FeeHeadName+',  Month = '+ Cast(DateName(mm,DATEADD(mm,@Month-1,0)) as varchar)+', session='+@SessionId+' Challan No= '+Cast(@ChallanNo as varchar)+'. '+ISNULL(@Narration1,'')
				--------------------------------- Entry into V3M_FIN_StudentLedger-------------
				insert into V3M_FIN_StudentLedger(GroupCode,BranchCode,SessionId,StudentId,ReferenceNo,ReferenceType,Narration,Amount,TransMode,TransDetail,RunningBalance,TransactionDate,TransactionBy)
				Values(@GroupCode,@BranchCode,@SessionId,@StudentId,@ChallanNo,'CHALLAN',@Narration,@HeadAmount,'','',dbo.Fn_GetStudentRunningBalance(@GroupCode,@BranchCode,@SessionId,@StudentId,@HeadAmount),getdate(),@CreatedBy)

				---------------------------------Entry into V3M_FIN_FeeHeadLedger-------------
				insert into V3M_FIN_FeeHeadLedger(GroupCode,BranchCode,CurrentSession,HeadId,MonthNoFor,MonthNoInDue,SessionFor,Amount,RefType,RefId,TransType,Narration
				,IsValid,InvoiceId,TransDate,TransBy,CrDr,EnteredVia)
				Values(@GroupCode,@BranchCode,@SessionId,@HeadId,@Month,Month(getdate()),@SessionId,@HeadAmount,'STUDENT',@StudentId,'DUE',@Narration
				,1,@InvoiceId,getdate(),@CreatedBy,'DR','Pro_FIN_Usp_AddFeeHeadToStudentChallan')

				if(@ConcessionAmount>0)
				begin
					set @Narration='Concession added fee head = '+@FeeHeadName+',  Month = '+ Cast(DateName(mm,DATEADD(mm,@Month-1,0)) as varchar)+', session='+@SessionId+' Challan No= '+Cast(@ChallanNo as varchar)+'. '+ISNULL(@Narration1,'')

					--------------------------------- Entry into V3M_FIN_StudentLedger-------------
					insert into V3M_FIN_StudentLedger(GroupCode,BranchCode,SessionId,StudentId,ReferenceNo,ReferenceType,Narration,Amount,TransMode,TransDetail,RunningBalance,TransactionDate,TransactionBy)
					Values(@GroupCode,@BranchCode,@SessionId,@StudentId,@ChallanNo,'CHALLAN',@Narration,-@ConcessionAmount,'','',dbo.Fn_GetStudentRunningBalance(@GroupCode,@BranchCode,@SessionId,@StudentId,-@ConcessionAmount),getdate(),@CreatedBy)

					---------------------------------Entry into V3M_FIN_FeeHeadLedger-------------
					insert into V3M_FIN_FeeHeadLedger(GroupCode,BranchCode,CurrentSession,HeadId,MonthNoFor,MonthNoInDue,SessionFor,Amount,RefType,RefId,TransType,Narration
					,IsValid,InvoiceId,TransDate,TransBy,CrDr,EnteredVia)
					Values(@GroupCode,@BranchCode,@SessionId,@HeadId,@Month,Month(getdate()),@SessionId,-@ConcessionAmount,'STUDENT',@StudentId,'CON',@Narration
					,1,@InvoiceId,getdate(),@CreatedBy,'DR','Pro_FIN_Usp_AddFeeHeadToStudentChallan')
				end

				FETCH NEXT FROM month_cursor INTO @Month
			END
			CLOSE month_cursor
			DEALLOCATE month_cursor
		END

 COMMIT TRANSACTION
 -- keep -11 (concession > fee head amount); success only when nothing was rejected
 if(ISNULL(@Result,0) <> -11)
	set @Result=1
 SELECT  @ErrorCode  = 0
 RETURN @ErrorCode                               -- =0 if success,  <>0 if failure

 END TRY
 BEGIN CATCH
  IF @@TRANCOUNT > 0
	ROLLBACK TRANSACTION
  set @Result=0
  insert into Det_ExceptionDetails(ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)
  values('Proc','EditChall_AddFeeHead','FIN_Usp_AddFeeHeadToStudentChallan',ERROR_MESSAGE(), GETDATE())
  RETURN @ErrorCode
 END CATCH
END
GO

/*
DECLARE @Result int, @Months dbo.TVP_MonthNo
INSERT INTO @Months(MonthNo) VALUES (9),(10)
EXEC dbo.FIN_Usp_AddFeeHeadToStudentChallan @StudentId=1, @InvoiceId=1, @HeadId=1, @MonthNo=@Months,
     @HeadAmount=2500, @CreatedBy='test', @Narration1='', @Result=@Result OUTPUT
SELECT @Result
*/
