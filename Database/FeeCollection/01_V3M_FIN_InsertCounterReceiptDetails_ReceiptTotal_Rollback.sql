/*
    Rollback: puts back V3M_FIN_InsertCounterReceiptDetails exactly as deployed on 27-10-2026 (no @ReceiptTotalAmount).
    Deploy the previous API with it (the new API sends @ReceiptTotalAmount).
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

        BEGIN TRANSACTION;

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

