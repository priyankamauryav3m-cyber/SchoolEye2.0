/*
    V3M_FIN_EditStudentReceipt
    ----------------------------------------------------------------
    View Student Receipt -> Edit -> Save Revised Receipt.

    A receipt is never updated in place when money / fee heads change. It is
    "cancelled & re-issued" in ONE transaction, so V3M_FIN_FeeHeadLedger stays
    append-only and every report can be rebuilt from it:

      1. V3M_FIN_CancelStudentReceipt @CancelType = 2 (Edited)
           -> mirror (negative) rows of every ledger row of the old receipt,
              invoice payment rolled back, student balance restored
      2. FIN_Usp_RemoveFeeHeadFromChallan for lines marked RemoveFromInvoice
           -> runs AFTER step 1, so the line has no money left and only the
              DUE / CON reversal is written (no second RECV / SB reversal)
      3. V3M_FIN_InsertCounterReceiptDetails
           -> revised receipt with a NEW receipt number, OriginalReceiptId = old
           -> V3M_Fin_StudentBalance = balance after step 1 + SB rows of the revised receipt
              (the save SP overwrites it; other advances of the student are kept)
      4. journal voucher of the old receipt made invalid (revised receipt posts its own)
      5. Det_TransactionLog audit (OperationType = 'EDIT')

    When only cheque / bank / remark details change (same heads, amounts, mode,
    bank account and date) the receipt is updated in place + audit, no ledger rows.

    Amount reduced -> @DifferenceMode 1 = keep the difference as student advance
                                       2 = refund (revised receipt shows the lower amount)
    Amount increased -> the extra is collected in the revised receipt.

    @Result :  1 = revised receipt generated (@NewReceiptId / @NewReceiptNo)
               2 = payment details updated in the same receipt
               3 = receipt cancelled, nothing left to re-issue (all heads removed and refunded)
              -1 = receipt not found / already cancelled
              -2 = a later active receipt exists (only the latest receipt can be edited)
              -3 = a fee head of this receipt was already removed from its invoice
              -4 = invalid fee head line (not a due of this student / negative amount)
              -5 = paid amount is more than the due of a fee head
              -6 = a fee head that still has money paid cannot be removed from the invoice
              -7 = refund is more than the amount received
              -8 = nothing changed
    SQL errors are logged in Det_ExceptionDetails and re-thrown.

    Callers : StudentFeeReceiptService.EditStudentReceipt
    Rollback: EditStudentReceipt_Rollback.sql
*/

SET NOCOUNT ON;
GO

CREATE OR ALTER PROCEDURE dbo.V3M_FIN_EditStudentReceipt
    @GroupCode      VARCHAR(10),
    @BranchCode     VARCHAR(10),
    @ReceiptId      BIGINT,
    @PaymentMode    VARCHAR(10),
    @ChequeNo       VARCHAR(50)  = '',
    @ChequeDate     DATE         = NULL,
    @ChequeBank     VARCHAR(50)  = '',
    @ChequeType     VARCHAR(20)  = '',
    @BankBranch     VARCHAR(100) = '',
    @BankAccountId  INT          = 0,
    @ReceiptDate    DATE,
    @Remark         VARCHAR(200) = '',
    @EditReason     VARCHAR(250),
    @DifferenceMode TINYINT      = 1,
    @EditedBy       VARCHAR(20),
    @Lines          dbo.EditReceiptLineType READONLY,
    @NewReceiptId   BIGINT       OUTPUT,
    @NewReceiptNo   VARCHAR(50)  OUTPUT,
    @Result         INT          OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SELECT @NewReceiptId = 0, @NewReceiptNo = '', @Result = 0;

    DECLARE @StudentId BIGINT, @SessionId BIGINT, @ClassCode VARCHAR(20), @SectionId INT, @SocietyId INT,
            @ReceiptNo VARCHAR(50), @PreviousBalance DECIMAL(18,2), @OldPaid DECIMAL(18,2), @OldBalance DECIMAL(18,2),
            @OldPaymentMode VARCHAR(50), @OldBankAccountId INT, @OldReceiptDate DATE, @OldRemark VARCHAR(250),
            @OldChequeNo VARCHAR(50), @OldChequeDate DATE, @OldChequeBank VARCHAR(50), @OldChequeType VARCHAR(50), @OldBankBranch VARCHAR(100);

    SELECT @StudentId = StudentId, @SessionId = SessionId, @ClassCode = ClassCode, @SectionId = SectionId,
           @SocietyId = ISNULL(SocietyId, 0), @ReceiptNo = ReceiptNo,
           @PreviousBalance = ISNULL(PreviousBalance, 0), @OldPaid = ISNULL(PaidAmount, 0), @OldBalance = ISNULL(Balance, 0),
           @OldPaymentMode = PaymentMode, @OldBankAccountId = ISNULL(BankAccountId, 0), @OldReceiptDate = CAST(CreatedDate AS DATE),
           @OldRemark = Remark, @OldChequeNo = ChequeNo, @OldChequeDate = CAST(ChequeDate AS DATE), @OldChequeBank = ChequeBank,
           @OldChequeType = ChequeType, @OldBankBranch = ChequeBankBranch
    FROM dbo.V3M_FIN_StudentReceiptMaster
    WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND ReceiptId = @ReceiptId AND Valid = 1;

    IF @StudentId IS NULL
    BEGIN SET @Result = -1; RETURN; END

    IF EXISTS (SELECT 1 FROM dbo.V3M_FIN_StudentReceiptMaster
               WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND StudentId = @StudentId
                 AND SessionId = @SessionId AND Valid = 1 AND ReceiptId > @ReceiptId)
    BEGIN SET @Result = -2; RETURN; END

    ----------------------------------------------------------------------
    -- old lines paired with their invoice line (same pairing the cancel SP uses)
    ----------------------------------------------------------------------
    DECLARE @Old TABLE (DetInvoiceId BIGINT NULL, PaidAmount DECIMAL(18,2));

    INSERT INTO @Old (DetInvoiceId, PaidAmount)
    SELECT f.DetInvoiceId, SUM(f.PaidAmount)
    FROM dbo.V3M_FIN_FnReceiptInvoiceLines(@ReceiptId) f
    WHERE f.GroupCode = @GroupCode AND f.BranchCode = @BranchCode
    GROUP BY f.DetInvoiceId, f.InvoiceId, f.FeeHeadId, f.MonthNo;

    IF EXISTS (SELECT 1 FROM @Old WHERE DetInvoiceId IS NULL)
    BEGIN SET @Result = -3; RETURN; END

    ----------------------------------------------------------------------
    -- new lines: only dues of this student / session on a live invoice
    ----------------------------------------------------------------------
    DECLARE @New TABLE
    (
        DetInvoiceId BIGINT PRIMARY KEY, InvoiceId BIGINT, FeeHeadId INT, MonthNo INT,
        Amount DECIMAL(18,2), Concession DECIMAL(18,2), WaiveOff DECIMAL(18,2),
        PaidElsewhere DECIMAL(18,2), OldPaid DECIMAL(18,2), PaidAmount DECIMAL(18,2),
        RemoveFromInvoice BIT, IsLateFee BIT
    );

    INSERT INTO @New (DetInvoiceId, InvoiceId, FeeHeadId, MonthNo, Amount, Concession, WaiveOff, PaidElsewhere, OldPaid, PaidAmount, RemoveFromInvoice, IsLateFee)
    SELECT l.DetInvoiceId, di.InvoiceId, di.FeeHeadId, di.MonthNo,
           ISNULL(di.Amount, 0), ISNULL(di.Concession, 0), ISNULL(di.WaiveOff, 0),
           ISNULL(di.ReceivedAmt, 0) - ISNULL(o.PaidAmount, 0), ISNULL(o.PaidAmount, 0),
           l.PaidAmount, l.RemoveFromInvoice,
           -- late fee invoices are created by V3M_FIN_InsertLateFeeForReceipt (PeriodId 0, Narration 'Late Fee|...')
           CASE WHEN mi.PeriodId = 0 AND mi.Narration LIKE 'Late Fee|%' THEN 1 ELSE 0 END
    FROM @Lines l
    INNER JOIN dbo.V3M_FIN_DetInvoice di ON di.DetInvoiceId = l.DetInvoiceId
    INNER JOIN dbo.V3M_FIN_MstInvoice mi
        ON mi.InvoiceId = di.InvoiceId AND mi.GroupCode = @GroupCode AND mi.BranchCode = @BranchCode
       AND mi.StudentId = @StudentId AND mi.SessionId = @SessionId AND mi.Ch_Status > 0
    LEFT JOIN @Old o ON o.DetInvoiceId = l.DetInvoiceId;

    IF (SELECT COUNT(*) FROM @New) <> (SELECT COUNT(*) FROM @Lines)
       OR EXISTS (SELECT 1 FROM @Lines WHERE PaidAmount < 0)
    BEGIN SET @Result = -4; RETURN; END

    IF EXISTS (SELECT 1 FROM @New WHERE PaidAmount > Amount - Concession - WaiveOff - PaidElsewhere)
    BEGIN SET @Result = -5; RETURN; END

    IF EXISTS (SELECT 1 FROM @New WHERE RemoveFromInvoice = 1 AND (PaidAmount > 0 OR PaidElsewhere > 0))
    BEGIN SET @Result = -6; RETURN; END

    ----------------------------------------------------------------------
    -- amounts of the revised receipt
    ----------------------------------------------------------------------
    DECLARE @OldLinesTotal DECIMAL(18,2) = ISNULL((SELECT SUM(PaidAmount) FROM @Old), 0);
    DECLARE @NewLinesTotal DECIMAL(18,2) = ISNULL((SELECT SUM(PaidAmount) FROM @New), 0);
    DECLARE @Reduced       DECIMAL(18,2) = @OldLinesTotal - @NewLinesTotal;   -- > 0 heads reduced, < 0 heads increased
    DECLARE @NewPaid DECIMAL(18,2), @NewBalance DECIMAL(18,2);

    IF @DifferenceMode = 2
    BEGIN
        -- refund: the revised receipt holds only what is kept
        SET @NewPaid    = @OldPaid - @Reduced;
        SET @NewBalance = @OldBalance;
    END
    ELSE
    BEGIN
        -- advance: the money received stays in the receipt, the reduced part becomes student advance (negative balance)
        SET @NewPaid    = @OldPaid + CASE WHEN @Reduced < 0 THEN -@Reduced ELSE 0 END;
        SET @NewBalance = CASE WHEN @Reduced > 0
                               THEN (CASE WHEN @OldBalance > 0 THEN 0 ELSE @OldBalance END) - @Reduced
                               ELSE @OldBalance END;
    END

    IF @NewPaid < 0
    BEGIN SET @Result = -7; RETURN; END

    DECLARE @AmountChanged BIT =
        CASE WHEN EXISTS (SELECT 1 FROM @New n FULL JOIN @Old o ON o.DetInvoiceId = n.DetInvoiceId
                          WHERE ISNULL(n.PaidAmount, 0) <> ISNULL(o.PaidAmount, 0) OR ISNULL(n.RemoveFromInvoice, 0) = 1)
             THEN 1 ELSE 0 END;

    DECLARE @PostingChanged BIT =
        CASE WHEN @AmountChanged = 1
               OR @NewPaid <> @OldPaid
               OR ISNULL(@PaymentMode, '') <> ISNULL(@OldPaymentMode, '')
               OR ISNULL(@BankAccountId, 0) <> @OldBankAccountId
               OR @ReceiptDate <> @OldReceiptDate
             THEN 1 ELSE 0 END;

    DECLARE @DetailsChanged BIT =
        CASE WHEN ISNULL(@ChequeNo, '')   <> ISNULL(@OldChequeNo, '')
               OR ISNULL(@ChequeDate, '19000101') <> ISNULL(@OldChequeDate, '19000101')
               OR ISNULL(@ChequeBank, '') <> ISNULL(@OldChequeBank, '')
               OR ISNULL(@ChequeType, '') <> ISNULL(@OldChequeType, '')
               OR ISNULL(@BankBranch, '') <> ISNULL(@OldBankBranch, '')
               OR ISNULL(@Remark, '')     <> ISNULL(@OldRemark, '')
             THEN 1 ELSE 0 END;

    IF @PostingChanged = 0 AND @DetailsChanged = 0
    BEGIN SET @Result = -8; RETURN; END

    BEGIN TRY
        BEGIN TRANSACTION;

        -- lock the receipt row and re-check inside the transaction (two users editing at once)
        IF NOT EXISTS (SELECT 1 FROM dbo.V3M_FIN_StudentReceiptMaster WITH (UPDLOCK, HOLDLOCK)
                       WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND ReceiptId = @ReceiptId AND Valid = 1)
        BEGIN
            ROLLBACK TRANSACTION;
            SET @Result = -1;
            RETURN;
        END

        DECLARE @Audit dbo.ColumnChangeType;

        ------------------------------------------------------------------
        -- only payment details changed -> same receipt, audit only
        ------------------------------------------------------------------
        IF @PostingChanged = 0
        BEGIN
            INSERT INTO @Audit (ColumnName, OldValue, NewValue)
            VALUES ('ChequeNo', @OldChequeNo, @ChequeNo),
                   ('ChequeDate', CONVERT(VARCHAR(10), @OldChequeDate, 103), CONVERT(VARCHAR(10), @ChequeDate, 103)),
                   ('ChequeBank', @OldChequeBank, @ChequeBank),
                   ('ChequeType', @OldChequeType, @ChequeType),
                   ('ChequeBankBranch', @OldBankBranch, @BankBranch),
                   ('Remark', @OldRemark, @Remark),
                   ('EditReason', NULL, @EditReason);

            UPDATE dbo.V3M_FIN_StudentReceiptMaster
            SET ChequeNo = @ChequeNo, ChequeDate = @ChequeDate, ChequeBank = @ChequeBank, ChequeType = @ChequeType,
                ChequeBankBranch = @BankBranch, Remark = @Remark, EditReason = @EditReason
            WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND ReceiptId = @ReceiptId;

            EXEC dbo.Usp_SaveTransactionLog
                @ReferenceKey = 'V3M_FIN_StudentReceiptMaster', @ReferenceId = @ReceiptNo,
                @TableName = 'V3M_FIN_StudentReceiptMaster', @OperationType = 'EDIT',
                @TransBy = @EditedBy, @IP = NULL, @Changes = @Audit;

            COMMIT TRANSACTION;
            SELECT @NewReceiptId = @ReceiptId, @NewReceiptNo = @ReceiptNo, @Result = 2;
            RETURN;
        END

        ------------------------------------------------------------------
        -- 1. cancel the old receipt as "Edited"
        --    (the cancel SP appends ' | Cancelled: <reason>' to Remark, VARCHAR(250);
        --     the full old remark is kept in the audit log below)
        ------------------------------------------------------------------
        DECLARE @CancelNote VARCHAR(30) = 'Edited';
        IF LEN(ISNULL(@OldRemark, '')) > 250 - LEN(' | Cancelled: ' + @CancelNote)
            UPDATE dbo.V3M_FIN_StudentReceiptMaster
            SET Remark = LEFT(Remark, 250 - LEN(' | Cancelled: ' + @CancelNote))
            WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND ReceiptId = @ReceiptId;

        DECLARE @CancelResult INT = 0;
        EXEC dbo.V3M_FIN_CancelStudentReceipt
            @ReceiptId = @ReceiptId, @GroupCode = @GroupCode, @BranchCode = @BranchCode,
            @CancelledBy = @EditedBy, @CancelReason = @CancelNote, @CancelType = 2,
            @Result = @CancelResult OUTPUT;

        IF ISNULL(@CancelResult, 0) <> 1
            THROW 50101, 'Receipt could not be cancelled for edit.', 1;

        -- student advance after the cancel took back the old receipt's own effect; the revised receipt's
        -- save SP overwrites the balance, so it is set again below from this value + the new receipt's SB rows
        DECLARE @BalanceAfterCancel DECIMAL(18,2) = ISNULL((
            SELECT TOP 1 Balance FROM dbo.V3M_Fin_StudentBalance
            WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND StudentId = @StudentId
              AND SessionId = @SessionId AND IsValid = 1), 0);

        ------------------------------------------------------------------
        -- 2. fee heads removed from the invoice as well
        ------------------------------------------------------------------
        DECLARE @RmInvoiceId BIGINT, @RmHeadId INT, @RmMonthNo INT, @RmResult INT;
        DECLARE @RmNarration VARCHAR(200) = LEFT('Edit receipt ' + @ReceiptNo + ': ' + @EditReason, 200);

        DECLARE curRemove CURSOR LOCAL FAST_FORWARD FOR
            SELECT InvoiceId, FeeHeadId, MonthNo FROM @New WHERE RemoveFromInvoice = 1;
        OPEN curRemove;
        FETCH NEXT FROM curRemove INTO @RmInvoiceId, @RmHeadId, @RmMonthNo;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            SET @RmResult = 0;
            EXEC dbo.FIN_Usp_RemoveFeeHeadFromChallan
                @StudentId = @StudentId, @InvoiceId = @RmInvoiceId, @HeadId = @RmHeadId, @MonthNo = @RmMonthNo,
                @CreatedBy = @EditedBy, @Narration1 = @RmNarration, @Result = @RmResult OUTPUT;

            IF ISNULL(@RmResult, 0) <> 1
                THROW 50102, 'Fee head could not be removed from the invoice.', 1;

            FETCH NEXT FROM curRemove INTO @RmInvoiceId, @RmHeadId, @RmMonthNo;
        END
        CLOSE curRemove;
        DEALLOCATE curRemove;

        -- invoices that lost lines: status from what is left on them
        UPDATE mi
        SET mi.Ch_Status = CASE WHEN ISNULL(b.Due, 0) <= 0 THEN 3 WHEN ISNULL(b.Received, 0) > 0 THEN 2 ELSE 1 END
        FROM dbo.V3M_FIN_MstInvoice mi
        INNER JOIN (SELECT DISTINCT InvoiceId FROM @New WHERE RemoveFromInvoice = 1) r ON r.InvoiceId = mi.InvoiceId
        OUTER APPLY (
            SELECT SUM(di.Amount - ISNULL(di.Concession, 0) - ISNULL(di.WaiveOff, 0) - ISNULL(di.ReceivedAmt, 0)) AS Due,
                   SUM(ISNULL(di.ReceivedAmt, 0)) AS Received
            FROM dbo.V3M_FIN_DetInvoice di
            WHERE di.InvoiceId = mi.InvoiceId
        ) b
        WHERE mi.Ch_Status > 0;

        ------------------------------------------------------------------
        -- 3. revised receipt (new receipt number)
        ------------------------------------------------------------------
        IF @NewLinesTotal > 0 OR @NewPaid > 0
        BEGIN
            DECLARE @FeeHeads dbo.FeeHeadDetailType;

            -- normal lines (StatusFlag 1, no adjustment): receipt TotalAmount = head amount, Concession = head concession
            INSERT INTO @FeeHeads (FeeHeadId, PaidAmount, LumpsumAmount, MonthNoCsv, FeeHeadAmount, Concession, Deduction, StatusFlag, DetInvoiceId, AdjustAmount)
            SELECT FeeHeadId, PaidAmount, Amount, CAST(MonthNo AS VARCHAR(10)), Amount, Concession, WaiveOff, '1', CAST(DetInvoiceId AS INT), 0
            FROM @New
            WHERE PaidAmount > 0;

            DECLARE @RcptNo VARCHAR(50), @RcptId BIGINT;

            EXEC dbo.V3M_FIN_InsertCounterReceiptDetails
                @StudentId = @StudentId, @GroupCode = @GroupCode, @BranchCode = @BranchCode,
                @SessionId = @SessionId, @ClassCode = @ClassCode, @SectionId = @SectionId,
                @PaidAmount = @NewPaid, @Balance = @NewBalance, @PaymentMode = @PaymentMode,
                @ChequeNo = @ChequeNo, @ChequeDate = @ChequeDate, @ChequeBank = @ChequeBank, @ChequeType = @ChequeType,
                @LateFee = 0, @PreviousBalance = @PreviousBalance, @Remark = @Remark, @SocietyId = @SocietyId,
                @CreatedBy = @EditedBy, @ReceiptDate = @ReceiptDate, @BankAccountId = @BankAccountId, @BankBranch = @BankBranch,
                @FeeHeadDetails = @FeeHeads, @StudentReceiptNo = @RcptNo OUTPUT, @ReceiptId = @RcptId OUTPUT;

            IF ISNULL(@RcptNo, '') = '' OR ISNULL(@RcptId, 0) = 0
                THROW 50103, 'Revised receipt could not be generated.', 1;

            -- every line must have reached its invoice line (the save SP skips lines it cannot match)
            IF ISNULL((SELECT SUM(PaidAmount) FROM dbo.V3M_FIN_StudentReceiptDetail
                       WHERE ReceiptId = @RcptId AND GroupCode = @GroupCode AND BranchCode = @BranchCode
                         AND InvoiceId IN (SELECT InvoiceId FROM @New)), 0) < @NewLinesTotal
                THROW 50104, 'Revised receipt lines do not match the selected fee heads.', 1;

            -- link to the old receipt; late fee lines shown as late fee, the same as the receipt they replace
            DECLARE @NewLateFee DECIMAL(18,2) = ISNULL((SELECT SUM(PaidAmount) FROM @New WHERE IsLateFee = 1 AND PaidAmount > 0), 0);

            UPDATE dbo.V3M_FIN_StudentReceiptMaster
            SET OriginalReceiptId = @ReceiptId,
                EditReason        = @EditReason,
                LateFee           = ISNULL(LateFee, 0) + @NewLateFee,
                TotalAmount       = ISNULL(TotalAmount, 0) - @NewLateFee
            WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND ReceiptId = @RcptId;

            -- V3M_FIN_GeneratedStudentCounterAdjustemntReceipt sets Balance = @NewBalance (absolute);
            -- keep every other advance of the student: balance after cancel + this receipt's SB rows
            UPDATE dbo.V3M_Fin_StudentBalance
            SET Balance = @BalanceAfterCancel + ISNULL((
                    SELECT SUM(Amount) FROM dbo.V3M_FIN_FeeHeadLedger
                    WHERE ReceiptId = @RcptId AND TransType = 'SB' AND IsValid = 1), 0)
            WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND StudentId = @StudentId
              AND SessionId = @SessionId AND IsValid = 1;

            SELECT @NewReceiptId = @RcptId, @NewReceiptNo = @RcptNo;
        END

        UPDATE dbo.V3M_FIN_StudentReceiptMaster
        SET IsEdited = 1, EditReason = @EditReason
        WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND ReceiptId = @ReceiptId;

        ------------------------------------------------------------------
        -- 4. journal voucher of the old receipt (V3M_Fin_ReceiptJournalEntry, BillNo = ReceiptId)
        ------------------------------------------------------------------
        UPDATE jd
        SET jd.IsValid = 0, jd.UpdatedBy = @EditedBy, jd.UpdatedDate = GETDATE()
        FROM dbo.V3M_Fin_DetJournalLedger jd
        INNER JOIN dbo.V3M_Fin_MstJournalLedger jm ON jm.JLSeedId = jd.JLID
        WHERE jm.GroupCode = @GroupCode AND jm.BranchCode = @BranchCode AND jm.TransType = 'REC'
          AND jm.BillNo = CAST(@ReceiptId AS VARCHAR(20)) AND jm.IsValid = 1;

        UPDATE dbo.V3M_Fin_MstJournalLedger
        SET IsValid = 0, IsModified = 1, UpdatedBy = @EditedBy, UpdatedDate = GETDATE()
        WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND TransType = 'REC'
          AND BillNo = CAST(@ReceiptId AS VARCHAR(20)) AND IsValid = 1;

        ------------------------------------------------------------------
        -- 5. audit
        ------------------------------------------------------------------
        INSERT INTO @Audit (ColumnName, OldValue, NewValue)
        VALUES ('ReceiptNo', @ReceiptNo, NULLIF(@NewReceiptNo, '')),
               ('PaidAmount', CAST(@OldPaid AS VARCHAR(20)), CAST(@NewPaid AS VARCHAR(20))),
               ('FeeHeadTotal', CAST(@OldLinesTotal AS VARCHAR(20)), CAST(@NewLinesTotal AS VARCHAR(20))),
               ('PaymentMode', @OldPaymentMode, @PaymentMode),
               ('ReceiptDate', CONVERT(VARCHAR(10), @OldReceiptDate, 103), CONVERT(VARCHAR(10), @ReceiptDate, 103)),
               ('Remark', @OldRemark, @Remark),
               ('Difference', NULL, CASE WHEN @Reduced > 0 THEN CASE WHEN @DifferenceMode = 2 THEN 'Refund ' ELSE 'Advance ' END + CAST(@Reduced AS VARCHAR(20)) END),
               ('EditReason', NULL, @EditReason);

        EXEC dbo.Usp_SaveTransactionLog
            @ReferenceKey = 'V3M_FIN_StudentReceiptMaster', @ReferenceId = @ReceiptNo,
            @TableName = 'V3M_FIN_StudentReceiptMaster', @OperationType = 'EDIT',
            @TransBy = @EditedBy, @IP = NULL, @Changes = @Audit;

        COMMIT TRANSACTION;
        SET @Result = CASE WHEN @NewReceiptId > 0 THEN 1 ELSE 3 END;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        INSERT INTO dbo.Det_ExceptionDetails (ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)
        VALUES ('Finance', 'ViewStudentReceipt', 'V3M_FIN_EditStudentReceipt', ERROR_MESSAGE() + ' Line:' + CAST(ERROR_LINE() AS VARCHAR(10)), GETDATE());

        SELECT @NewReceiptId = 0, @NewReceiptNo = '', @Result = 0;
        THROW;
    END CATCH
END
GO
