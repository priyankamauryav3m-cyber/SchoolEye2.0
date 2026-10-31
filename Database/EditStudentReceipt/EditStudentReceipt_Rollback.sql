/*
    Edit Student Receipt — ROLLBACK
    ----------------------------------------------------------------
    Run in this order. Data already written by edited receipts is NOT deleted:
    the reversal / revised rows in V3M_FIN_FeeHeadLedger, V3M_FIN_StudentLedger,
    V3M_FIN_StudentReceiptMaster are real accounting entries and must stay.

    1. FIN_Usp_RemoveFeeHeadFromChallan -> previous body (paid fee heads removable again)
       V3M_FIN_CancelStudentReceipt     -> previous body (old InvoiceId+FeeHeadId+MonthNo join)
    2. drop V3M_FIN_EditStudentReceipt, V3M_FIN_GetStudentReceiptForEdit, V3M_FIN_vwStudentReceiptEditTrail,
       V3M_FIN_FnReceiptInvoiceLines
    3. drop type dbo.EditReceiptLineType (after the procedure that uses it)
    4. columns OriginalReceiptId / EditReason are KEPT (they hold the edit trail).
       Drop them only with project-owner approval — see the commented block at the end.
*/

SET NOCOUNT ON;
GO

---------------------------------------------------------------------------
-- 1. FIN_Usp_RemoveFeeHeadFromChallan : previous body
---------------------------------------------------------------------------
/*
DECLARE @Result INT = 0;
EXEC dbo.FIN_Usp_RemoveFeeHeadFromChallan 80050, 3, 12234, 8, 'V3M', 'Priynka', @Result OUTPUT;
SELECT @Result AS Result;
*/
CREATE OR ALTER PROC [dbo].[FIN_Usp_RemoveFeeHeadFromChallan] @StudentId bigint,@InvoiceId int,@HeadId int,@MonthNo int,
@CreatedBy varchar(50),@Narration1 varchar(200)='',@Result int output
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


---------------------------------------------------------------------------
-- 1b. V3M_FIN_CancelStudentReceipt : previous body (before the function is dropped)
---------------------------------------------------------------------------

-- ---------------------------------------------------------------------------------------------------
-- V3M_FIN_CancelStudentReceipt
-- ---------------------------------------------------------------------------------------------------

CREATE OR ALTER PROCEDURE dbo.V3M_FIN_CancelStudentReceipt
    @ReceiptId      BIGINT,
    @GroupCode      VARCHAR(10),
    @BranchCode     VARCHAR(10),
    @CancelledBy    VARCHAR(50),
    @CancelReason   VARCHAR(250) = '',
    @CancelType     TINYINT = 1,   -- 1 = Cancelled, 2 = Edited (superseded)
    @Result         INT OUTPUT
AS
BEGIN
    ------------------------------ Affected Tables -------------------------------------
    -- V3M_FIN_StudentReceiptMaster (Valid/CancelBy/CancelDate/CancelType set)
    -- V3M_FIN_DetInvoice, V3M_FIN_MstInvoice (payment reversed)
    -- V3M_FIN_FeeHeadLedger, V3M_FIN_StudentLedger (reversal rows appended)
    -- V3M_Fin_StudentBalance (restored to PreviousBalance)
    -- Det_TransactionLog (audit, via Usp_SaveTransactionLog)
    -------------------------------------------------------------------------------------
    SET NOCOUNT ON;
    SET @Result = 0;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.V3M_FIN_StudentReceiptMaster
            WHERE ReceiptId = @ReceiptId AND GroupCode = @GroupCode AND BranchCode = @BranchCode AND Valid = 1
        )
        BEGIN
            ROLLBACK TRANSACTION;
            SET @Result = -1;   -- receipt not found, or already cancelled
            RETURN -1;
        END

        DECLARE @StudentId BIGINT, @SessionId BIGINT, @ReceiptNo VARCHAR(50), @PreviousBalance DECIMAL(18,2);
        SELECT @StudentId = StudentId, @SessionId = SessionId, @ReceiptNo = ReceiptNo, @PreviousBalance = PreviousBalance
        FROM dbo.V3M_FIN_StudentReceiptMaster
        WHERE ReceiptId = @ReceiptId AND GroupCode = @GroupCode AND BranchCode = @BranchCode;

        ----------------------------------------------------------------------
        -- Reverse V3M_FIN_DetInvoice for every line this receipt paid
        -- (matched the same way FIN_Usp_RemoveFeeHeadFromChallan matches lines:
        --  InvoiceId + FeeHeadId + MonthNo, since ReceiptDetail has no DetInvoiceId column)
        ----------------------------------------------------------------------
        UPDATE di
        SET di.ReceivedAmt = ISNULL(di.ReceivedAmt, 0) - rd.PaidAmount,
            di.UpdatedBy   = @CancelledBy,
            di.UpdatedDate = GETDATE(),
            di.Status = CASE
                WHEN (ISNULL(di.ReceivedAmt, 0) - rd.PaidAmount) <= 0 THEN 1
                WHEN (ISNULL(di.ReceivedAmt, 0) - rd.PaidAmount + ISNULL(di.Concession, 0) + ISNULL(di.WaiveOff, 0)) >= di.Amount THEN 3
                ELSE 2
            END
        FROM dbo.V3M_FIN_DetInvoice di
        INNER JOIN dbo.V3M_FIN_StudentReceiptDetail rd
            ON rd.InvoiceId = di.InvoiceId AND rd.FeeHeadId = di.FeeHeadId AND rd.MonthNo = di.MonthNo
        WHERE rd.ReceiptId = @ReceiptId AND rd.InvoiceId IS NOT NULL AND rd.Valid = 1;

        ----------------------------------------------------------------------
        -- Roll back into V3M_FIN_MstInvoice
        ----------------------------------------------------------------------
        ;WITH InvoiceTotals AS (
            SELECT InvoiceId, SUM(PaidAmount) AS PaidSum
            FROM dbo.V3M_FIN_StudentReceiptDetail
            WHERE ReceiptId = @ReceiptId AND InvoiceId IS NOT NULL AND Valid = 1
            GROUP BY InvoiceId
        )
        UPDATE mi
        SET mi.ReceivedAmount = ISNULL(mi.ReceivedAmount, 0) - it.PaidSum,
            mi.Ch_Status = CASE WHEN ISNULL(mi.ReceivedAmount, 0) - it.PaidSum > 0 THEN 2 ELSE 1 END
        FROM dbo.V3M_FIN_MstInvoice mi
        INNER JOIN InvoiceTotals it ON it.InvoiceId = mi.InvoiceId;

        ----------------------------------------------------------------------
        -- Mirror-reverse every FeeHeadLedger row this receipt created
        ----------------------------------------------------------------------
        INSERT INTO dbo.V3M_FIN_FeeHeadLedger
            (GroupCode, BranchCode, CurrentSession, HeadId, MonthNoFor, MonthNoInDue, SessionFor, Amount, RefType, RefId, TransType, Narration, IsValid, InvoiceId, TransDate, TransBy, ReceiptId, IsReceiptOnline, CrDr, EnteredVia, SessionId)
        SELECT
            GroupCode, BranchCode, CurrentSession, HeadId, MonthNoFor, MonthNoInDue, SessionFor, -Amount, RefType, RefId, TransType,
            'Reversal (Cancel Receipt ' + @ReceiptNo + ') - ' + ISNULL(Narration, ''), 1, InvoiceId, GETDATE(), @CancelledBy, ReceiptId, IsReceiptOnline, CrDr, 'Pro_V3M_FIN_CancelStudentReceipt', SessionId
        FROM dbo.V3M_FIN_FeeHeadLedger
        WHERE ReceiptId = @ReceiptId AND IsValid = 1;

        ----------------------------------------------------------------------
        -- Mirror-reverse the StudentLedger entry this receipt created
        ----------------------------------------------------------------------
        INSERT INTO dbo.V3M_FIN_StudentLedger
            (GroupCode, BranchCode, SessionId, StudentId, ReferenceNo, ReferenceType, Narration, Amount, TransMode, TransDetail, RunningBalance, TransactionDate, TransactionBy, CreatedDate)
        SELECT
            GroupCode, BranchCode, SessionId, StudentId, ReferenceNo, ReferenceType,
            'Reversal (Cancel Receipt ' + @ReceiptNo + ') - ' + ISNULL(Narration, ''), -Amount, TransMode, TransDetail,
            dbo.Fn_GetStudentRunningBalance(GroupCode, BranchCode, SessionId, StudentId, -Amount), GETDATE(), @CancelledBy, GETDATE()
        FROM dbo.V3M_FIN_StudentLedger
        WHERE ReferenceNo = @ReceiptNo AND StudentId = @StudentId;

        ----------------------------------------------------------------------
        -- Restore V3M_Fin_StudentBalance to its value before this receipt
        ----------------------------------------------------------------------
        UPDATE dbo.V3M_Fin_StudentBalance
        SET Balance = @PreviousBalance
        WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND StudentId = @StudentId AND SessionId = @SessionId AND IsValid = 1;

        ----------------------------------------------------------------------
        -- Mark the receipt cancelled (activates existing, previously-unused columns)
        ----------------------------------------------------------------------
        UPDATE dbo.V3M_FIN_StudentReceiptMaster
        SET Valid = 0,
            CancelBy = @CancelledBy,
            CancelDate = GETDATE(),
            CancelType = @CancelType,
            Remark = ISNULL(Remark, '') + ' | Cancelled: ' + ISNULL(@CancelReason, '')
        WHERE ReceiptId = @ReceiptId AND GroupCode = @GroupCode AND BranchCode = @BranchCode;

        UPDATE dbo.V3M_FIN_StudentReceiptDetail
        SET Valid = 0
        WHERE ReceiptId = @ReceiptId;

        ----------------------------------------------------------------------
        -- Header-level audit trail
        ----------------------------------------------------------------------
        IF OBJECT_ID('dbo.Usp_SaveTransactionLog', 'P') IS NOT NULL AND TYPE_ID('dbo.ColumnChangeType') IS NOT NULL
        BEGIN
            DECLARE @AuditChanges dbo.ColumnChangeType;
            INSERT INTO @AuditChanges (ColumnName, OldValue, NewValue)
            VALUES
                ('Valid', '1', '0'),
                ('CancelType', NULL, CAST(@CancelType AS NVARCHAR(10))),
                ('CancelReason', NULL, @CancelReason);

            DECLARE @OpType VARCHAR(20) = CASE WHEN @CancelType = 2 THEN 'EDIT-CANCEL' ELSE 'CANCEL' END;
            EXEC dbo.Usp_SaveTransactionLog
                @ReferenceKey  = 'V3M_FIN_StudentReceiptMaster',
                @ReferenceId   = @ReceiptNo,
                @TableName     = 'V3M_FIN_StudentReceiptMaster',
                @OperationType = @OpType,
                @TransBy       = @CancelledBy,
                @IP            = NULL,
                @Changes       = @AuditChanges;
        END

        COMMIT TRANSACTION;
        SET @Result = 1;
        RETURN 0;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        INSERT INTO dbo.Det_ExceptionDetails (ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)
        VALUES ('Finance', 'FeeCollection', 'V3M_FIN_CancelStudentReceipt', ERROR_MESSAGE() + ' Line:' + CAST(ERROR_LINE() AS VARCHAR(10)), GETDATE());

        SET @Result = 0;
        THROW;
    END CATCH
END


GO

---------------------------------------------------------------------------
-- 2. new objects
---------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS dbo.V3M_FIN_EditStudentReceipt;
GO
DROP PROCEDURE IF EXISTS dbo.V3M_FIN_GetStudentReceiptForEdit;
GO
DROP VIEW IF EXISTS dbo.V3M_FIN_vwStudentReceiptEditTrail;
GO
DROP FUNCTION IF EXISTS dbo.V3M_FIN_FnReceiptInvoiceLines;
GO
-- 07: cancel settlement / refund OTP (the OTP table holds only short-lived hashed OTPs)
DROP PROCEDURE IF EXISTS dbo.V3M_FIN_GetReceiptRefundContacts;
GO
DROP PROCEDURE IF EXISTS dbo.V3M_FIN_SaveReceiptRefundOtp;
GO
DROP PROCEDURE IF EXISTS dbo.V3M_FIN_GetReceiptRefundDetails;
GO
DROP TABLE IF EXISTS dbo.V3M_FIN_ReceiptRefundOtp;
GO
-- V3M_FIN_StudentReceiptMaster.CancelSettlement is KEPT (records advance / refund of cancelled receipts)

---------------------------------------------------------------------------
-- 3. table type
---------------------------------------------------------------------------
IF TYPE_ID('dbo.EditReceiptLineType') IS NOT NULL
    DROP TYPE dbo.EditReceiptLineType;
GO

---------------------------------------------------------------------------
-- 4. columns (DESTRUCTIVE — only with project-owner approval)
---------------------------------------------------------------------------
-- ALTER TABLE dbo.V3M_FIN_StudentReceiptMaster DROP COLUMN OriginalReceiptId;
-- ALTER TABLE dbo.V3M_FIN_StudentReceiptMaster DROP COLUMN EditReason;
-- GO
