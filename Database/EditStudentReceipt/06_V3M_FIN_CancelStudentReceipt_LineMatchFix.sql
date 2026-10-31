/*
    V3M_FIN_CancelStudentReceipt — line matching + student balance fixes
    ----------------------------------------------------------------
    1. Line matching
       The DetInvoice reversal joined ReceiptDetail on InvoiceId + FeeHeadId + MonthNo.
       A late fee invoice (V3M_FIN_InsertLateFeeForReceipt) has one line per parent
       invoice with the SAME head and month, e.g. 3 lines of 400 / 400 / 380:
       the join gave 9 matches and each line was reduced by an arbitrary detail row.
       Now the rows are paired by dbo.V3M_FIN_FnReceiptInvoiceLines (01 script).

    2. Student balance (advance) is adjusted, not overwritten
       Old: V3M_Fin_StudentBalance.Balance = receipt.PreviousBalance.
            Cancelling an OLDER receipt wiped every advance created after it
            (e.g. advance -1000 of a later revised receipt became 0).
       New: Balance = Balance - (SB ledger rows written by this receipt), so only this
            receipt's own effect is taken back and the table stays equal to the SB ledger.

    3. Fee heads already removed through View Student Dues and Receipt -> Edit Invoice
       (receipt line without an invoice line; FIN_Usp_RemoveFeeHeadFromChallan already
        reversed its RECV, reduced MstInvoice.ReceivedAmount and moved the paid amount to
        student advance):
         - its RECV rows are NOT reversed again, MstInvoice is NOT reduced again
         - the advance made from it is taken back (SB row + balance), because the money
           of a cancelled receipt was never kept.

    4. Settlement (View Student Receipt -> Cancel, Cancel Type = Other), see 07 script
         @SettleMode 1 = keep as advance : SB row -PaidAmount, student balance -PaidAmount,
                                           student ledger keeps the payment ('Advance' row)
         @SettleMode 2 = refund to parent: OTP checked first (@RefundOtpHash, max 5 attempts),
                                           journal voucher of the receipt made invalid
         @SettleMode 0 = as before (cheque bounce, V3M_FIN_EditStudentReceipt)

    Change  : DetInvoice / MstInvoice reversal, ledger mirror, student balance, settlement.
    Needs   : 07_ReceiptCancelSettlement.sql (table / column) BEFORE this script.
    Callers : StudentFeeReceiptService.CancelStudentReceipt (Cancel button),
              V3M_FIN_EditStudentReceipt (CancelType = 2).
    Needs   : 01_EditStudentReceipt_Schema.sql (function) first.
    Rollback: EditStudentReceipt_Rollback.sql (restores the previous body)
*/

SET NOCOUNT ON;
GO


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
    @Result         INT OUTPUT,
    @SettleMode     TINYINT = 0,   -- 0 = none (cheque bounce / edit), 1 = keep as student advance, 2 = refund to parent
    @RefundOtpHash  VARCHAR(64) = NULL   -- SHA-256 of ReceiptId:OTP, required for @SettleMode = 2
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

    ----------------------------------------------------------------------
    -- Refund: the parent's OTP (latest unused one of this receipt) must match.
    -- Checked before the transaction so a wrong attempt is counted (max 5).
    -- @Result -3 wrong OTP, -4 OTP not found / expired, -5 too many wrong attempts
    ----------------------------------------------------------------------
    DECLARE @RefundOtpId BIGINT;
    IF @SettleMode = 2
    BEGIN
        DECLARE @OtpHashDb VARCHAR(64), @OtpExpiryUtc DATETIME2(0), @OtpAttempts TINYINT;

        SELECT TOP 1 @RefundOtpId = RefundOtpId, @OtpHashDb = OtpHash, @OtpExpiryUtc = ExpiryUtc, @OtpAttempts = Attempts
        FROM dbo.V3M_FIN_ReceiptRefundOtp
        WHERE ReceiptId = @ReceiptId AND GroupCode = @GroupCode AND BranchCode = @BranchCode AND IsUsed = 0
        ORDER BY RefundOtpId DESC;

        IF @RefundOtpId IS NULL OR @OtpExpiryUtc < SYSUTCDATETIME()
        BEGIN SET @Result = -4; RETURN -4; END

        IF @OtpAttempts >= 5
        BEGIN SET @Result = -5; RETURN -5; END

        IF ISNULL(@RefundOtpHash, '') <> @OtpHashDb
        BEGIN
            UPDATE dbo.V3M_FIN_ReceiptRefundOtp SET Attempts = Attempts + 1 WHERE RefundOtpId = @RefundOtpId;
            SET @Result = -3;
            RETURN -3;
        END
    END

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
        -- (ReceiptDetail has no DetInvoiceId column: V3M_FIN_FnReceiptInvoiceLines pairs each
        --  detail row with its invoice line, so a late fee invoice with several lines of the
        --  same head + month is reversed line by line instead of an arbitrary match)
        ----------------------------------------------------------------------
        UPDATE di
        SET di.ReceivedAmt = ISNULL(di.ReceivedAmt, 0) - m.PaidAmount,
            di.UpdatedBy   = @CancelledBy,
            di.UpdatedDate = GETDATE(),
            di.Status = CASE
                WHEN (ISNULL(di.ReceivedAmt, 0) - m.PaidAmount) <= 0 THEN 1
                WHEN (ISNULL(di.ReceivedAmt, 0) - m.PaidAmount + ISNULL(di.Concession, 0) + ISNULL(di.WaiveOff, 0)) >= di.Amount THEN 3
                ELSE 2
            END
        FROM dbo.V3M_FIN_DetInvoice di
        INNER JOIN (
            SELECT DetInvoiceId, SUM(PaidAmount) AS PaidAmount
            FROM dbo.V3M_FIN_FnReceiptInvoiceLines(@ReceiptId)
            WHERE DetInvoiceId IS NOT NULL
            GROUP BY DetInvoiceId
        ) m ON m.DetInvoiceId = di.DetInvoiceId;

        ----------------------------------------------------------------------
        -- Roll back into V3M_FIN_MstInvoice
        ----------------------------------------------------------------------
        -- only lines that still have their invoice line: for a removed fee head
        -- FIN_Usp_RemoveFeeHeadFromChallan already took its amount out of ReceivedAmount
        ;WITH InvoiceTotals AS (
            SELECT InvoiceId, SUM(PaidAmount) AS PaidSum
            FROM dbo.V3M_FIN_FnReceiptInvoiceLines(@ReceiptId)
            WHERE DetInvoiceId IS NOT NULL
            GROUP BY InvoiceId
        )
        UPDATE mi
        SET mi.ReceivedAmount = ISNULL(mi.ReceivedAmount, 0) - it.PaidSum,
            mi.Ch_Status = CASE WHEN ISNULL(mi.ReceivedAmount, 0) - it.PaidSum > 0 THEN 2 ELSE 1 END
        FROM dbo.V3M_FIN_MstInvoice mi
        INNER JOIN InvoiceTotals it ON it.InvoiceId = mi.InvoiceId;

        ----------------------------------------------------------------------
        -- Fee heads of this receipt already removed from their invoice
        -- (View Student Dues and Receipt -> Edit Invoice): no invoice line left
        ----------------------------------------------------------------------
        DECLARE @Removed TABLE (InvoiceId BIGINT, FeeHeadId INT, MonthNo INT, PaidAmount DECIMAL(18,2));
        INSERT INTO @Removed (InvoiceId, FeeHeadId, MonthNo, PaidAmount)
        SELECT InvoiceId, FeeHeadId, MonthNo, SUM(PaidAmount)
        FROM dbo.V3M_FIN_FnReceiptInvoiceLines(@ReceiptId)
        WHERE DetInvoiceId IS NULL
        GROUP BY InvoiceId, FeeHeadId, MonthNo;

        DECLARE @RemovedPaid DECIMAL(18,2) = ISNULL((SELECT SUM(PaidAmount) FROM @Removed), 0);

        -- this receipt's own effect on the student advance (taken before the mirror rows are added)
        DECLARE @ReceiptSBEffect DECIMAL(18,2) = ISNULL((
            SELECT SUM(Amount) FROM dbo.V3M_FIN_FeeHeadLedger
            WHERE ReceiptId = @ReceiptId AND TransType = 'SB' AND IsValid = 1
              AND ISNULL(EnteredVia, '') <> 'Pro_V3M_FIN_CancelStudentReceipt'), 0);

        ----------------------------------------------------------------------
        -- Mirror-reverse every FeeHeadLedger row this receipt created
        -- (RECV of a removed fee head was already reversed by FIN_Usp_RemoveFeeHeadFromChallan)
        ----------------------------------------------------------------------
        INSERT INTO dbo.V3M_FIN_FeeHeadLedger
            (GroupCode, BranchCode, CurrentSession, HeadId, MonthNoFor, MonthNoInDue, SessionFor, Amount, RefType, RefId, TransType, Narration, IsValid, InvoiceId, TransDate, TransBy, ReceiptId, IsReceiptOnline, CrDr, EnteredVia, SessionId)
        SELECT
            l.GroupCode, l.BranchCode, l.CurrentSession, l.HeadId, l.MonthNoFor, l.MonthNoInDue, l.SessionFor, -l.Amount, l.RefType, l.RefId, l.TransType,
            'Reversal (Cancel Receipt ' + @ReceiptNo + ') - ' + ISNULL(l.Narration, ''), 1, l.InvoiceId, GETDATE(), @CancelledBy, l.ReceiptId, l.IsReceiptOnline, l.CrDr, 'Pro_V3M_FIN_CancelStudentReceipt', l.SessionId
        FROM dbo.V3M_FIN_FeeHeadLedger l
        WHERE l.ReceiptId = @ReceiptId AND l.IsValid = 1
          AND ISNULL(l.EnteredVia, '') <> 'Pro_V3M_FIN_CancelStudentReceipt'
          AND NOT (l.TransType = 'RECV' AND EXISTS (
                SELECT 1 FROM @Removed r
                WHERE r.InvoiceId = l.InvoiceId AND r.FeeHeadId = l.HeadId AND r.MonthNo = l.MonthNoFor));

        -- the advance made from a removed fee head's money is taken back
        IF @RemovedPaid > 0
            INSERT INTO dbo.V3M_FIN_FeeHeadLedger
                (GroupCode, BranchCode, CurrentSession, HeadId, MonthNoFor, MonthNoInDue, SessionFor, Amount, RefType, RefId, TransType, Narration, IsValid, InvoiceId, TransDate, TransBy, ReceiptId, IsReceiptOnline, CrDr, EnteredVia, SessionId)
            SELECT @GroupCode, @BranchCode, @SessionId, 0, r.MonthNo, MONTH(GETDATE()), @SessionId, r.PaidAmount, 'STUDENT', @StudentId, 'SB',
                   'Reversal (Cancel Receipt ' + @ReceiptNo + ') - advance of removed fee head', 1, r.InvoiceId, GETDATE(), @CancelledBy, @ReceiptId, 0, 'CR', 'Pro_V3M_FIN_CancelStudentReceipt', @SessionId
            FROM @Removed r
            WHERE r.PaidAmount > 0;

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
        -- V3M_Fin_StudentBalance: take back only this receipt's effect
        -- (not "= PreviousBalance", which wiped advances created after this receipt)
        ----------------------------------------------------------------------
        UPDATE dbo.V3M_Fin_StudentBalance
        SET Balance = ISNULL(Balance, 0) - @ReceiptSBEffect + @RemovedPaid
        WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND StudentId = @StudentId AND SessionId = @SessionId AND IsValid = 1;

        ----------------------------------------------------------------------
        -- Settlement of the money received in this receipt (dues are already back on the invoices)
        ----------------------------------------------------------------------
        DECLARE @ReceiptPaid DECIMAL(18,2), @ReceiptDate DATE;
        SELECT @ReceiptPaid = ISNULL(PaidAmount, 0), @ReceiptDate = CAST(CreatedDate AS DATE)
        FROM dbo.V3M_FIN_StudentReceiptMaster
        WHERE ReceiptId = @ReceiptId AND GroupCode = @GroupCode AND BranchCode = @BranchCode;

        IF @SettleMode = 1 AND @ReceiptPaid > 0
        BEGIN
            -- 1. keep as advance: SB ledger row + student balance (negative = advance), adjusted in the next fee
            INSERT INTO dbo.V3M_FIN_FeeHeadLedger
                (GroupCode, BranchCode, CurrentSession, HeadId, MonthNoFor, MonthNoInDue, SessionFor, Amount, RefType, RefId, TransType, Narration, IsValid, InvoiceId, TransDate, TransBy, ReceiptId, IsReceiptOnline, CrDr, EnteredVia, SessionId)
            VALUES (@GroupCode, @BranchCode, @SessionId, 0, MONTH(GETDATE()), MONTH(GETDATE()), @SessionId, -@ReceiptPaid, 'STUDENT', @StudentId, 'SB',
                    'Cancel Receipt ' + @ReceiptNo + ' - amount kept as student advance', 1, NULL, GETDATE(), @CancelledBy, @ReceiptId, 0, 'CR', 'Pro_V3M_FIN_CancelStudentReceipt', @SessionId);

            IF EXISTS (SELECT 1 FROM dbo.V3M_Fin_StudentBalance
                       WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND StudentId = @StudentId AND SessionId = @SessionId AND IsValid = 1)
                UPDATE dbo.V3M_Fin_StudentBalance
                SET Balance = ISNULL(Balance, 0) - @ReceiptPaid
                WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND StudentId = @StudentId AND SessionId = @SessionId AND IsValid = 1;
            ELSE
                INSERT INTO dbo.V3M_Fin_StudentBalance (GroupCode, BranchCode, StudentId, Balance, CreatedDate, CreatedBy, SessionId, IsValid)
                VALUES (@GroupCode, @BranchCode, @StudentId, -@ReceiptPaid, GETDATE(), @CancelledBy, @SessionId, 1);

            -- the payment stays in the student ledger (the reversal above brought the dues back)
            DECLARE @TransResult INT, @NegPaid DECIMAL(18,2) = -@ReceiptPaid,
                    @AdvanceNarration VARCHAR(250) = 'Receipt ' + @ReceiptNo + ' cancelled - amount kept as advance';
            EXEC dbo.V3M_FIN_StudentTrans @GroupCode, @BranchCode, @SessionId, @StudentId, @ReceiptNo, 'Advance',
                 @AdvanceNarration, @NegPaid, '', '', @ReceiptDate, @CancelledBy, @TransResult OUTPUT;
        END

        IF @SettleMode = 2
        BEGIN
            -- 2. refund: money goes back to the parent -> journal voucher of the receipt is not valid any more
            UPDATE jd
            SET jd.IsValid = 0, jd.UpdatedBy = @CancelledBy, jd.UpdatedDate = GETDATE()
            FROM dbo.V3M_Fin_DetJournalLedger jd
            INNER JOIN dbo.V3M_Fin_MstJournalLedger jm ON jm.JLSeedId = jd.JLID
            WHERE jm.GroupCode = @GroupCode AND jm.BranchCode = @BranchCode AND jm.TransType = 'REC'
              AND jm.BillNo = CAST(@ReceiptId AS VARCHAR(20)) AND jm.IsValid = 1;

            UPDATE dbo.V3M_Fin_MstJournalLedger
            SET IsValid = 0, IsModified = 1, UpdatedBy = @CancelledBy, UpdatedDate = GETDATE()
            WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND TransType = 'REC'
              AND BillNo = CAST(@ReceiptId AS VARCHAR(20)) AND IsValid = 1;

            UPDATE dbo.V3M_FIN_ReceiptRefundOtp SET IsUsed = 1, UsedDate = GETDATE() WHERE RefundOtpId = @RefundOtpId;
        END

        ----------------------------------------------------------------------
        -- Mark the receipt cancelled (activates existing, previously-unused columns)
        ----------------------------------------------------------------------
        UPDATE dbo.V3M_FIN_StudentReceiptMaster
        SET Valid = 0,
            CancelBy = @CancelledBy,
            CancelDate = GETDATE(),
            CancelType = @CancelType,
            CancelSettlement = NULLIF(@SettleMode, 0),
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
                ('CancelReason', NULL, @CancelReason),
                ('Settlement', NULL, CASE @SettleMode WHEN 1 THEN 'Advance ' WHEN 2 THEN 'Refund ' END + CAST(@ReceiptPaid AS NVARCHAR(20)));

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
