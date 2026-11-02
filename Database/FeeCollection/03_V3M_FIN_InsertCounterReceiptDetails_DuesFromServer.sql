/*
    V3M_FIN_InsertCounterReceiptDetails  (03, 02-11-2026)  - receipt Total / Balance from the database, not the screen

    Problem : Fee Collection sent the student's dues as they were when the student was selected. After one receipt
              the screen did not reload them, so the next receipt for the same student got the old dues
              (SR0093: Total 1,24,000 instead of 66,000). Edit Receipt (V3M_FIN_EditStudentReceipt) also calls this
              procedure and the receipt it made had Total = fee head lines - late fee and an old Balance
              (SR0095: Total 28,000, Paid 29,220, Balance 91,000).
    Change  : at the start of the transaction the procedure reads the student's dues itself
              (dbo.Fn_GetStudentBalance = open invoice dues + V3M_Fin_StudentBalance, i.e. the dues before this
              receipt) and, after the late fee is added, writes on the receipt master:
                  TotalAmount = dues before (not below 0) + late fee charged by this receipt
                  Balance     = dues before - cash paid by this receipt   (negative = advance left)
              so TotalAmount - PaidAmount = Balance on every receipt.
              @ReceiptTotalAmount is still accepted (the deployed API sends it) but the database value wins.
              @Balance is still passed on unchanged to V3M_FIN_GeneratedStudentCounterAdjustemntReceipt, so the
              student advance (V3M_Fin_StudentBalance) and the SB ledger work exactly as before.
              Nothing else in the procedure is changed.
    Callers : StudentFeeService.AdjustStudentFeeHeadWise (Fee Collection), V3M_FIN_EditStudentReceipt (Edit Receipt).
    Rollback: 01_V3M_FIN_InsertCounterReceiptDetails_ReceiptTotal.sql (the version deployed before this one).
*/
CREATE OR ALTER PROCEDURE [dbo].[V3M_FIN_InsertCounterReceiptDetails]
(
    @StudentId        BIGINT,
    @GroupCode        VARCHAR(10),
    @BranchCode       VARCHAR(10),
    @SessionId        BIGINT,
    @ClassCode        VARCHAR(20),
    @SectionId        INT,
    @PaidAmount       DECIMAL(18,2),          -- sirf fee ka amount (late fee SP khud jodega)
    @Balance          DECIMAL(18,2),
    @ReceiptTotalAmount DECIMAL(18,2) = NULL,   -- receipt master TotalAmount = total dues before this receipt (NULL = fee head total)
    @PaymentMode      VARCHAR(10),
    @ChequeNo         VARCHAR(50) = '',
    @ChequeDate       DATE = NULL,
    @ChequeBank       VARCHAR(80) = '',
    @ChequeType       VARCHAR(20) = '',
    @LateFee          DECIMAL(18,2) = 0,      -- UI ki late fee (0 = SP khud calculate kare)
    @PreviousBalance  DECIMAL(18,2),
    @Remark           VARCHAR(250) = '',
    @SocietyId        INT = 0,
    @CreatedBy        VARCHAR(20),
    @ReceiptDate      DATE,
    @BankAccountId    INT,
    @BankBranch       VARCHAR(100) = '',
    @FeeHeadDetails   dbo.FeeHeadDetailType READONLY,
    @StudentReceiptNo VARCHAR(50) OUTPUT,
    @ReceiptId        BIGINT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        IF @StudentId IS NULL OR @StudentId = 0
            RETURN;

        DECLARE @LumpsumTotal DECIMAL(18,2), @TotalAmount DECIMAL(18,2), @TotalConcession DECIMAL(18,2);

        SELECT
            @LumpsumTotal = ISNULL(SUM(CASE WHEN StatusFlag = '2' THEN LumpsumAmount ELSE 0 END), 0),
            @TotalAmount  = ISNULL(SUM(CASE
                                WHEN StatusFlag = '2' THEN 0
                                WHEN AdjustAmount > 0 THEN (FeeHeadAmount - Deduction - AdjustAmount - Concession)
                                ELSE (FeeHeadAmount - Deduction - AdjustAmount) END), 0),
            @TotalConcession = ISNULL(SUM(CASE
                                WHEN StatusFlag = '2' THEN 0
                                WHEN AdjustAmount > 0 THEN 0
                                ELSE Concession END), 0)
        FROM @FeeHeadDetails;

        DECLARE @FinalPreviousBalance DECIMAL(18,2);

        IF @PreviousBalance < 0 AND @LumpsumTotal > 0
            RETURN;
        ELSE IF @LumpsumTotal <= 0
            SET @FinalPreviousBalance = @PreviousBalance;
        ELSE
            SET @FinalPreviousBalance = @LumpsumTotal;

        -- Fee Collection sends the student's total dues before this receipt (124000 -> next receipt 84000),
        -- so the receipt shows Total, Paid and Balance (= Total - Paid). Ledger / invoice postings do not use @TotalAmount.
        IF @ReceiptTotalAmount IS NOT NULL
            SET @TotalAmount = @ReceiptTotalAmount;

        BEGIN TRANSACTION;

        -- dues of the student before this receipt (open invoices + student balance), read inside the transaction
        DECLARE @DuesBefore DECIMAL(18,2) = ISNULL(dbo.Fn_GetStudentBalance(@GroupCode, @BranchCode, @SessionId, @StudentId), 0);

        --=================================================================
        -- Receipt master (late fee yahan 0; late fee SP baad mein jodega)
        --=================================================================
        EXEC dbo.V3M_FIN_GeneratedStudentCounterAdjustemntReceipt
            @StudentId = @StudentId, @GroupCode = @GroupCode, @BranchCode = @BranchCode,
            @SessionId = @SessionId, @ClassCode = @ClassCode, @SectionId = @SectionId,
            @TotalAmount = @TotalAmount, @PaidAmount = @PaidAmount, @Concession = @TotalConcession,
            @Balance = @Balance, @PaymentMode = @PaymentMode, @ChequeNo = @ChequeNo,
            @ChequeDate = @ChequeDate, @ChequeBank = @ChequeBank,
            @LateFee = 0,
            @PreviousBalance = @FinalPreviousBalance, @Remark = @Remark, @ChequeType = @ChequeType,
            @SocietyId = @SocietyId, @CreatedBy = @CreatedBy, @ReceiptDate = @ReceiptDate,
            @BankAccountId = @BankAccountId, @BankBranch = @BankBranch,
            @StudentReceiptNo = @StudentReceiptNo OUTPUT, @ReceiptId = @ReceiptId OUTPUT;

        IF @StudentReceiptNo IS NULL OR LTRIM(RTRIM(@StudentReceiptNo)) = ''
        BEGIN
            ROLLBACK TRANSACTION;
            RETURN;
        END

        --=================================================================
        -- Month-split + invoice-match
        --=================================================================
        DECLARE @Matched TABLE
        (
            FeeHeadId     INT,
            MonthNo       INT,
            PaidAmount    DECIMAL(18,2),
            FeeHeadAmount DECIMAL(18,2),
            Concession    DECIMAL(18,2),
            IsAdjusted    INT,
            InvoiceId     BIGINT,
            DetInvoiceId  BIGINT
        );

        INSERT INTO @Matched (FeeHeadId, MonthNo, PaidAmount, FeeHeadAmount, Concession, IsAdjusted, InvoiceId, DetInvoiceId)
        SELECT
            fhd.FeeHeadId,
            e.MonthNo,
            fhd.PaidAmount,
            CASE WHEN fhd.StatusFlag = '2' OR fhd.AdjustAmount > 0 THEN fhd.LumpsumAmount ELSE (fhd.FeeHeadAmount - fhd.Deduction) END,
            CASE WHEN fhd.StatusFlag = '2' OR fhd.AdjustAmount > 0 THEN 0 ELSE fhd.Concession END,
            CASE WHEN fhd.StatusFlag = '2' THEN 1 ELSE 0 END,
            x.InvoiceId,
            x.DetInvoiceId
        FROM @FeeHeadDetails fhd
        CROSS APPLY (SELECT CAST(m.value AS INT) AS MonthNo FROM STRING_SPLIT(fhd.MonthNoCsv, ',') m) e
        CROSS APPLY (
            SELECT TOP 1 mcn.InvoiceId, dcn.DetInvoiceId
            FROM V3M_FIN_MstInvoice mcn
            INNER JOIN V3M_FIN_DetInvoice dcn ON mcn.InvoiceId = dcn.InvoiceId
            WHERE mcn.GroupCode = @GroupCode AND mcn.BranchCode = @BranchCode
              AND mcn.SessionId = @SessionId AND mcn.StudentId = @StudentId
              AND dcn.FeeHeadId = fhd.FeeHeadId
              AND ((fhd.DetInvoiceId > 0 AND dcn.DetInvoiceId = fhd.DetInvoiceId) OR (fhd.DetInvoiceId = 0 AND dcn.MonthNo = e.MonthNo))
              AND mcn.Ch_Status > 0
              AND dcn.ReceivedAmt < (dcn.Amount - dcn.Concession - ISNULL(dcn.WaiveOff, 0))
        ) x;

        --=================================================================
        -- Guard: koi bhi DetInvoice line apne due se zyada receive na ho
        --=================================================================
        IF EXISTS
        (
            SELECT 1
            FROM (SELECT DetInvoiceId, SUM(PaidAmount) AS TotalPaid FROM @Matched GROUP BY DetInvoiceId) m
            INNER JOIN V3M_FIN_DetInvoice d ON d.DetInvoiceId = m.DetInvoiceId
            WHERE ISNULL(d.ReceivedAmt, 0) + m.TotalPaid > (d.Amount - d.Concession - ISNULL(d.WaiveOff, 0))
        )
            THROW 50020, 'Receipt amount kisi fee head ke due amount se zyada hai (duplicate line?).', 1;

        --=================================================================
        -- Receipt detail
        --=================================================================
        INSERT INTO V3M_FIN_StudentReceiptDetail
            (ReceiptNo, ReceiptId, InvoiceId, FeeHeadId, MonthNo, PaidAmount, FeeHeadAmount, FeeHeadConcession,
             Valid, CreatedBy, CreatedDate, GroupCode, BranchCode, IsAdjusted)
        SELECT @StudentReceiptNo, @ReceiptId, m.InvoiceId, m.FeeHeadId, m.MonthNo, m.PaidAmount, m.FeeHeadAmount, m.Concession,
               1, @CreatedBy, GETDATE(), @GroupCode, @BranchCode, m.IsAdjusted
        FROM @Matched m;

        --=================================================================
        -- DetInvoice update
        --=================================================================
        UPDATE dcn
        SET dcn.ReceivedAmt  = ISNULL(dcn.ReceivedAmt, 0) + agg.TotalPaid,
            dcn.ReceivedDate = GETDATE(),
            dcn.Status       = CASE WHEN (dcn.Amount - dcn.Concession - (ISNULL(dcn.ReceivedAmt, 0) + agg.TotalPaid)) <= 0 THEN 3 ELSE 2 END
        FROM V3M_FIN_DetInvoice dcn
        INNER JOIN (SELECT DetInvoiceId, SUM(PaidAmount) AS TotalPaid FROM @Matched GROUP BY DetInvoiceId) agg
            ON dcn.DetInvoiceId = agg.DetInvoiceId;

        --=================================================================
        -- MstInvoice update + Ch_Status
        --=================================================================
        UPDATE mcn
        SET mcn.ReceivedAmount = ISNULL(mcn.ReceivedAmount, 0) + agg.TotalPaid,
            mcn.TransDate      = GETDATE(),
            mcn.Ch_Status      = CASE WHEN bal.Balance <= 0 THEN 3 ELSE 2 END
        FROM V3M_FIN_MstInvoice mcn
        INNER JOIN (SELECT InvoiceId, SUM(PaidAmount) AS TotalPaid FROM @Matched GROUP BY InvoiceId) agg
            ON mcn.InvoiceId = agg.InvoiceId
        INNER JOIN (
            SELECT InvoiceId, SUM(Amount) - (SUM(Concession) + SUM(ReceivedAmt)) AS Balance
            FROM V3M_FIN_DetInvoice
            WHERE InvoiceId IN (SELECT DISTINCT InvoiceId FROM @Matched)
            GROUP BY InvoiceId
        ) bal ON mcn.InvoiceId = bal.InvoiceId
        WHERE mcn.GroupCode = @GroupCode AND mcn.BranchCode = @BranchCode;

        --=================================================================
        -- Late fee (UI ka amount; 0 ho to SP khud calculate karega)
        --=================================================================
        DECLARE @LateFeeAmt DECIMAL(18,2) = 0;

        EXEC dbo.V3M_FIN_InsertLateFeeForReceipt
             @GroupCode     = @GroupCode,
             @BranchCode    = @BranchCode,
             @ReceiptId     = @ReceiptId,
             @CreatedBy     = @CreatedBy,
             @LateFeeFromUI = @LateFee,
             @LateFeeAmount = @LateFeeAmt OUTPUT;

        --=================================================================
        -- Fee head ledger (RECV)
        --=================================================================
        INSERT INTO V3M_FIN_FeeHeadLedger
            (GroupCode, BranchCode, CurrentSession, HeadId, MonthNoFor, MonthNoInDue, SessionFor, Amount, RefType, RefId, TransType,
             Narration, IsValid, InvoiceId, TransDate, TransBy, CreatedDate, ReceiptId, IsReceiptOnline, CrDr, EnteredVia, SessionId)
        SELECT @GroupCode, @BranchCode, @SessionId, m.FeeHeadId, m.MonthNo, MONTH(GETDATE()), @SessionId, -m.PaidAmount, 'STUDENT', @StudentId, 'RECV',
               'Receipt for Month No - ' + CAST(m.MonthNo AS VARCHAR), 1, m.InvoiceId, @ReceiptDate, @CreatedBy, GETDATE(), @ReceiptId, 0, 'CR',
               'Pro_V3M_FIN_InsertCounterReceiptDetails', @SessionId
        FROM @Matched m;

        --=================================================================
        -- Refundable liabilities
        --=================================================================
        INSERT INTO V3M_FIN_StudentLibilities (StudentId, HeadId, Amount, Date, Session, ReceiptNo, PaidStatus, GroupCode, BranchCode, CreatedBy, Remarks)
        SELECT @StudentId, m.FeeHeadId, m.PaidAmount, GETDATE(), @SessionId, @StudentReceiptNo, 1, @GroupCode, @BranchCode, @CreatedBy, 'Student Receipt'
        FROM @Matched m
        WHERE EXISTS (
            SELECT 1 FROM V3M_FIN_MstFeeHead fh
            WHERE fh.GroupCode = @GroupCode AND fh.BranchCode = @BranchCode
              AND fh.FeeHeadId = m.FeeHeadId AND fh.FeeHeadType = 'Refundable'
        );


        --=================================================================
        -- Receipt Total / Balance (Total - Paid = Balance). @PaidAmount is the cash part; the late fee SP has
        -- already added the late fee to both PaidAmount and LateFee of this receipt.
        --=================================================================
        UPDATE V3M_FIN_StudentReceiptMaster
        SET TotalAmount = CASE WHEN @DuesBefore > 0 THEN @DuesBefore ELSE 0 END + ISNULL(@LateFeeAmt, 0),
            Balance     = @DuesBefore - ISNULL(@PaidAmount, 0)
        WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND ReceiptId = @ReceiptId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        INSERT INTO Det_ExceptionDetails(ExcModule, ExcPage, ExcMethod, ExcMessage, ExcDate)
        VALUES('Procedure','V3M_FIN_InsertCounterReceiptDetails','V3M_FIN_InsertCounterReceiptDetails',ERROR_MESSAGE(),GETDATE());

        THROW;
    END CATCH
END
GO

