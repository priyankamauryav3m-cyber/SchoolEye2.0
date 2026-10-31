/*
    FIN_Usp_RemoveFeeHeadFromChallan — PAID fee head only with @AllowPaidHead = 1
    ----------------------------------------------------------------
    A paid fee head can be removed in two ways, never both for the same money:

      a) View Student Dues and Receipt -> Edit Invoice (@AllowPaidHead = 1)
         parent already has the receipt: the head is removed from the invoice, the paid
         amount is reversed (RECV) and kept as student advance (SB, V3M_Fin_StudentBalance),
         adjusted in the next fee. The receipt is NOT changed; V3M_FIN_EditStudentReceipt
         then refuses to edit that receipt (EditStatus -3), so it cannot be reversed again.
      b) View Student Receipt -> Edit (V3M_FIN_EditStudentReceipt)
         the receipt is reversed first and this procedure is called for a line that has
         no money left (@AllowPaidHead = 0 is enough).

    Any other caller (@AllowPaidHead = 0, e.g. Student Invoice Generation -> Edit Invoice):
    a head with ReceivedAmt > 0 is NOT removed, @Result = -2.

    Change  : new optional last parameter @AllowPaidHead (default 0) + one guard block
              before the transaction. Rest of the body unchanged.
    Deploy  : before the API (StudentInvoiceService passes @AllowPaidHead).
    Callers : StudentInvoiceService.RemoveFeeHeadToStudentChallanData (Edit Invoice),
              V3M_FIN_EditStudentReceipt.
              (FIN_Usp_RemoveFeeHeadFromChallanRecal is a separate procedure and is not changed.)
    Rollback: EditStudentReceipt_Rollback.sql (restores the previous body)
*/

SET NOCOUNT ON;
GO

/*
DECLARE @Result INT = 0;
EXEC dbo.FIN_Usp_RemoveFeeHeadFromChallan 80050, 3, 12234, 8, 'V3M', 'Priynka', @Result OUTPUT;
SELECT @Result AS Result;
*/
CREATE OR ALTER PROC [dbo].[FIN_Usp_RemoveFeeHeadFromChallan] @StudentId bigint,@InvoiceId int,@HeadId int,@MonthNo int,
@CreatedBy varchar(50),@Narration1 varchar(200)='',@Result int output,
@AllowPaidHead bit=0   -- 1 only from View Student Dues and Receipt -> Edit Invoice
AS
DECLARE @ErrorCode int=0
DECLARE @InvoiceNo varchar(20)
DECLARE @HeadAmount decimal(18,2)=0
DECLARE @Concession decimal(18,2)=0
DECLARE @WaiveOffAmt decimal(18,2)=0
DECLARE @ReceivedAmt decimal(18,2)=0
DECLARE @ReceivedDate datetime,@ChllanGeneratedDate datetime
DECLARE @GroupCode varchar(50)='',@BranchCode varchar(50)='',@SessionId bigint,@Narration varchar(150)  =''
DECLARE @DiffereceAmount decimal(18,2)=0
DECLARE @StudentBalance decimal(18,2)=0
DECLARE @FeeHeadName varchar(50)=''
Begin
 ------------------------------Paid fee head guard--------------------------------
 -- money already received on this line: removed only when the caller allows it
 -- (View Student Dues and Receipt -> Edit Invoice: the paid amount goes to student advance below,
 --  the parent's receipt is not changed); otherwise use View Student Receipt -> Edit
 IF ISNULL(@AllowPaidHead,0) = 0
    AND EXISTS (SELECT 1 FROM V3M_FIN_DetInvoice
            WHERE InvoiceId=@InvoiceId AND MonthNo=@MonthNo AND FeeHeadId=@HeadId AND ISNULL(ReceivedAmt,0) > 0)
 BEGIN
  SET @Result=-2
  RETURN -2
 END
 ---------------------------------------------------------------------------------

 ------------------------------Affected Tables-------------------------------------
 --------V3M_FIN_MstInvoice
 --------V3M_FIN_DetInvoiceNo
 --------V3M_FIN_StudentLedger
 --------V3M_FIN_FeeHeadLedger
 ---------------------------------------------------------------
 BEGIN TRY
    BEGIN TRANSACTION
    -------------------------------Getting Challan Id----------------------------------
    Select @InvoiceNo=InvoiceNo,@GroupCode=GroupCode,@BranchCode=BranchCode,@SessionId=SessionId,@ChllanGeneratedDate=CreatedDate from V3M_FIN_MstInvoice where InvoiceId=@InvoiceId
    --------------------------------Getting FeeHead-----------------------------------
    Select @HeadAmount=Amount,@Concession=Concession,@ReceivedAmt=ReceivedAmt,@ReceivedDate=ReceivedDate ,@WaiveOffAmt =WaiveOff
    from V3M_FIN_DetInvoice where InvoiceId=@InvoiceId and MonthNo=@MonthNo and FeeHeadId=@HeadId
    ---------------------------------Getting Fee Head Name-------------------------------
    Select @FeeHeadName=FeeHeadName from V3M_FIN_MstFeeHead where FeeHeadId=@HeadId and GroupCode=@GroupCode and BranchCode=@BranchCode

    --------------------------------Updating V3M_FIN_MstInvoice -----------------------
    Update V3M_FIN_MstInvoice Set TotalAmount=TotalAmount-@HeadAmount,TotalConcession=TotalConcession-@Concession,ReceivedAmount=ReceivedAmount-@ReceivedAmt
    where InvoiceId=@InvoiceId and GroupCode=@GroupCode and BranchCode=@BranchCode
	--remove waiveoff amt
	if(@WaiveOffAmt > 0)
	begin
		if exists (select * from V3M_Fin_StudentWaiveOff where GroupCode=@GroupCode and BranchCode=@BranchCode  and StudentId=@StudentId and SessionId =@SessionId and isvalid=1)
		begin
			declare @totAdjustedAmt decimal(18,2)=0
			select @totAdjustedAmt = AdjustedAmount from V3M_Fin_StudentWaiveOff
			where GroupCode=@GroupCode and BranchCode=@BranchCode  and StudentId=@StudentId and SessionId =@SessionId and isvalid=1
			if @WaiveOffAmt < @totAdjustedAmt
			begin
				update V3M_Fin_StudentWaiveOff set  AdjustedAmount = AdjustedAmount - @WaiveOffAmt
				where GroupCode=@GroupCode and BranchCode=@BranchCode  and StudentId=@StudentId and SessionId =@SessionId and isvalid=1

				Update V3M_FIN_MstInvoice Set TotalWaiveOff=TotalWaiveOff-@WaiveOffAmt
				where InvoiceId=@InvoiceId and GroupCode=@GroupCode and BranchCode=@BranchCode

				--Rakesh FeeHeadLedger verification
				insert into V3M_FIN_FeeHeadLedger(GroupCode,BranchCode,CurrentSession,HeadId,MonthNoFor,MonthNoInDue,SessionFor,Amount,RefType,RefId,TransType,
				Narration,IsValid,InvoiceId,TransDate,TransBy,CrDr,EnteredVia)
				values(@GroupCode, @BranchCode, @SessionId, @HeadId, @MonthNo, MONTH(getdate()),@SessionId, (@WaiveOffAmt),'STUDENT', @StudentId, 'WO',
				'Remove Adjusted from waive off for Month No - '+ cast(@MonthNo as varchar), 1, @InvoiceId, GETDATE(),@CreatedBy,'DR','Pro_FIN_Usp_RemoveFeeHeadFromChallan')

			end
			else
			begin
				update V3M_Fin_StudentWaiveOff set  AdjustedAmount = AdjustedAmount - @totAdjustedAmt
				where GroupCode=@GroupCode and BranchCode=@BranchCode  and StudentId=@StudentId and SessionId =@SessionId and isvalid=1

				Update V3M_FIN_MstInvoice Set TotalWaiveOff=TotalWaiveOff-@totAdjustedAmt
				where InvoiceId=@InvoiceId and GroupCode=@GroupCode and BranchCode=@BranchCode

				--Rakesh FeeHeadLedger verification
				insert into V3M_FIN_FeeHeadLedger(GroupCode,BranchCode,CurrentSession,HeadId,MonthNoFor,MonthNoInDue,SessionFor,Amount,RefType,RefId,TransType,
				Narration,IsValid,InvoiceId,TransDate,TransBy,CrDr,EnteredVia)
				values(@GroupCode, @BranchCode, @SessionId, @HeadId, @MonthNo, MONTH(getdate()),@SessionId, (@totAdjustedAmt),'STUDENT', @StudentId, 'WO',
				'Remove all Adjusted from waive off for Month No - '+ cast(@MonthNo as varchar), 1, @InvoiceId, GETDATE(),@CreatedBy,'DR','Pro_FIN_Usp_RemoveFeeHeadFromChallan')

			end
		end
	end

    --select * from V3M_FIN_MstInvoice where InvoiceId=@InvoiceId and GroupCode=@GroupCode and BranchCode=@BranchCode
    --------------------------------Updating V3M_FIN_DetInvoiceNo -----------------------
    --Update V3M_FIN_DetInvoiceNo Set [Status]=0 where InvoiceId=@InvoiceId And FeeHeadId=@HeadId and MonthNo=@MonthNo

    delete from V3M_FIN_DetInvoice where InvoiceId=@InvoiceId And FeeHeadId=@HeadId and MonthNo=@MonthNo
    ------------------------------------------------------------------------------------------------------------------------------
    set @DiffereceAmount=@HeadAmount-@Concession

   set @Narration='Removed fee head='+@FeeHeadName+',  Month='
   set @Narration=@Narration+ DateName(mm,DATEADD(mm,@MonthNo-1,0))+', session='+Cast(@SessionId as varchar)+' Challan No='+Cast(@InvoiceNo as varchar)+'. '+@Narration1
    ---------------------------------Reverse Entry into V3M_FIN_StudentLedger-------------
    insert into V3M_FIN_StudentLedger(GroupCode,BranchCode,SessionId,StudentId,ReferenceNo,ReferenceType,Narration,Amount,TransMode,TransDetail
	,RunningBalance,TransactionDate,TransactionBy)
    Values(@GroupCode,@BranchCode,@SessionId,@StudentId,@InvoiceNo,'CHALLAN',@Narration,0-@DiffereceAmount,'',''
	,dbo.Fn_GetStudentRunningBalance(@GroupCode,@BranchCode,@SessionId,@StudentId,0-@DiffereceAmount),getdate(),@CreatedBy)

    ---------------------------------Reverse Entry into V3M_FIN_FeeHeadLedger-------------
	--Rakesh FeeHeadLedger verification
    insert into V3M_FIN_FeeHeadLedger(GroupCode,BranchCode,CurrentSession,HeadId,MonthNoFor,MonthNoInDue,SessionFor,Amount,RefType,RefId,TransType,Narration
	,IsValid,InvoiceId,TransDate,TransBy,CrDr,EnteredVia)
    Values(@GroupCode,@BranchCode,@SessionId,@HeadId,@MonthNo,Month(getdate()),@SessionId,0-@HeadAmount,'STUDENT',@StudentId,'DUE',@Narration
	,1,@InvoiceId,getdate(),@CreatedBy,'DR','Pro_FIN_Usp_RemoveFeeHeadFromChallan')

    --if(@Concession=@HeadAmount or @ReceivedAmt>0)
    --Begin

      if(@Concession>0)
      begin
		  ---------------------------------Reverse Entry into V3M_FIN_FeeHeadLedger-------------
		  --Rakesh FeeHeadLedger verification
		   insert into V3M_FIN_FeeHeadLedger(GroupCode,BranchCode,CurrentSession,HeadId,MonthNoFor,MonthNoInDue,SessionFor,Amount,RefType,RefId,TransType,Narration
		   ,IsValid,InvoiceId,TransDate,TransBy,CRDr,EnteredVia)
		   Values(@GroupCode,@BranchCode,@SessionId,@HeadId,@MonthNo,Month(getdate()),@SessionId,@Concession,'STUDENT',@StudentId,'CON',@Narration
		   ,1,@InvoiceId,getdate(),@CreatedBy,'DR','Pro_FIN_Usp_RemoveFeeHeadFromChallan')
      end
      --========================================================FeeHead Amount Reverse Entry=================================================================
      if(@ReceivedAmt>0)
      begin
		   ---------------------------------Reverse Entry into V3M_FIN_FeeHeadLedger-------------
			--Rakesh FeeHeadLedger verification
		   insert into V3M_FIN_FeeHeadLedger(GroupCode,BranchCode,CurrentSession,HeadId,MonthNoFor,MonthNoInDue,SessionFor,Amount,RefType,RefId,TransType,Narration
		   ,IsValid,InvoiceId,TransDate,TransBy,CrDr,EnteredVia)
		   Values(@GroupCode,@BranchCode,@SessionId,@HeadId,@MonthNo,Month(getdate()),@SessionId,@ReceivedAmt,'STUDENT',@StudentId,'RECV',@Narration
		   ,1,@InvoiceId,getdate(),@CreatedBy,'CR','Pro_FIN_Usp_RemoveFeeHeadFromChallan')
		   --========================================================FeeHead Amount Reverse Entry=================================================================

			---------------------------------------------------------Managing Balance Amount---------------------------------------------------------------------
			if not exists(select * from V3M_Fin_StudentBalance where GroupCode=@GroupCode and BranchCode=@BranchCode and StudentId=@StudentId and SessionId = @SessionId and IsValid=1)
			begin
				insert into V3M_Fin_StudentBalance(GroupCode,BranchCode,StudentId,Balance,CreatedDate,CreatedBy,SessionId,IsValid)
				values(@GroupCode,@BranchCode,@StudentId,0-@ReceivedAmt,GETDATE(),@CreatedBy,@SessionId,1)

				---------------------------------Reverse Entry into V3M_FIN_FeeHeadLedger-------------
				--Rakesh FeeHeadLedger verification
				insert into V3M_FIN_FeeHeadLedger(GroupCode,BranchCode,CurrentSession,HeadId,MonthNoFor,MonthNoInDue,SessionFor,Amount,RefType,RefId,TransType,Narration
				,IsValid,InvoiceId,TransDate,TransBy,CrDr,EnteredVia)
				Values(@GroupCode,@BranchCode,@SessionId,0,@MonthNo,Month(getdate()),@SessionId,0-@ReceivedAmt,'STUDENT',@StudentId,'SB',@Narration
				,1,@InvoiceId,getdate(),@CreatedBy,'CR','Pro_FIN_Usp_RemoveFeeHeadFromChallan')

			end
			else
			begin

				Update V3M_Fin_StudentBalance set Balance=Balance-@ReceivedAmt where GroupCode=@GroupCode and BranchCode=@BranchCode and StudentId=@StudentId
				and SessionId = @SessionId and IsValid=1

				---------------------------------Reverse Entry into V3M_FIN_FeeHeadLedger-------------
				--Rakesh FeeHeadLedger verification
				insert into V3M_FIN_FeeHeadLedger(GroupCode,BranchCode,CurrentSession,HeadId,MonthNoFor,MonthNoInDue,SessionFor,Amount,RefType,RefId,TransType,Narration
				,IsValid,InvoiceId,TransDate,TransBy,CrDr,EnteredVia)
				Values(@GroupCode,@BranchCode,@SessionId,0,@MonthNo,Month(getdate()),@SessionId,0-@ReceivedAmt,'STUDENT',@StudentId,'SB',@Narration
				,1,@InvoiceId,getdate(),@CreatedBy,'CR','Pro_FIN_Usp_RemoveFeeHeadFromChallan')
			End
       --------------------------------------------------------------------------------------------------------------------------------------------------------
      end
   -- End
 COMMIT TRANSACTION
 set @Result=1
 SELECT  @ErrorCode  = 0
 RETURN @ErrorCode                               -- =0 if success,  <>0 if failure
 END TRY
 BEGIN CATCH
  ROLLBACK TRANSACTION
  RETURN @ErrorCode
  set @Result=0                       -- =0 if success,  <>0 if failure
  insert into Det_ExceptionDetails(ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)
  values('Finance','EditChall_RemoveFeeHead','FIN_Usp_RemoveFeeHeadFromChallan',@@ERROR, GETDATE())
 END CATCH
End
/*
DECLARE @Result int=0
exec FIN_Usp_RemoveFeeHeadFromChallan 60048,7,8,6,'V3M',@Result output
select @Result

Select * from Det_ExceptionDetails order by 1 desc
*/



GO
