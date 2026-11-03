/*
    Manual cheque receipt (one student) through the same save procedure Fee Collection uses
    -----------------------------------------------------------------------------------------
    What   : pays the student's open dues from @FromMonth up to @TillMonth (now July - December), net of the
             concession already on those invoice lines, adjusts the student's advance (18000), charges the late
             fee, and saves the receipt as a CHEQUE receipt by calling dbo.V3M_FIN_InsertCounterReceiptDetails.
             Cheque = dues (after concession) - advance adjusted + late fee   (expected 31820).
             That procedure writes every table (receipt master/detail, DetInvoice/MstInvoice received,
             late fee, fee head ledger, student ledger, student balance / advance, journal) exactly as the
             Fee Collection screen does.

    How    : 1. Fill the values in the "INPUT" block (ControlNo, SessionId, cheque details, late fee).
             2. Run with @Commit = 0 (dry run): it shows the dues, advance, cheque amount and the receipt it
                would create, then ROLLS BACK. Nothing is saved.
             3. If everything is right, set @Commit = 1 and run again -> receipt is saved.

    Safety : - stops if the cheque amount (dues + late fee - advance) is not @ExpectedChequeAmount
             - stops if the advance adjusted is not @ExpectedAdvance
             - stops if the concession on the July-December lines is not @ExpectedConcession
               (map + approve the concession from Fee Collection -> "Map concession" first; the save
                procedure does not write a new concession onto the invoice lines)
             - stops if @LateFee is 0 (0 makes the save procedure calculate its own late fee)
             - stops if the student / session is not found, or there are no dues
             - stops if the receipt lines do not add up to the dues paid
             - whole run is one transaction: any error -> nothing is saved

    Same calculation as Fee Collection (FeeCollection.razor -> StudentFee/AdjustStudentFeeCollection):
             advance          = -(V3M_Fin_StudentBalance.Balance)  when negative
             advance adjusted = MIN(advance, dues + late fee)
             @PaidAmount      = dues - advance adjusted          (cheque part of the fee heads; the save SP adds the late fee)
             cheque amount    = @PaidAmount + late fee
             @Balance         = -(advance left)  when advance is more than dues + late fee, else MAX(0, dues before - @PaidAmount)
             fee head lines   = full due of every open line up to @TillMonth (cheque + advance), oldest month first
    Rollback of a committed receipt: cancel it from View Student Receipt (Cancel) - do not delete rows by hand.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

--=====================================================================================
-- INPUT  (fill these)
--=====================================================================================
DECLARE @GroupCode            VARCHAR(10)   = '01';
DECLARE @BranchCode           VARCHAR(10)   = '01';
DECLARE @SessionId            BIGINT        = 1043;              -- <<< session id of the student (V3M session master)
DECLARE @ControlNo            VARCHAR(50)   = '0101CON0363';             -- <<< student control no., e.g. '0101CON0363'
DECLARE @FromMonth            INT           = 7;              -- pay dues from this month (7 = July)
DECLARE @TillMonth            INT           = 12;             -- pay dues up to this month (12 = December)

DECLARE @ReceiptDate          DATE          = CAST(GETDATE() AS DATE);
DECLARE @ChequeNo             VARCHAR(50)   = '0001';             -- <<< cheque no.
DECLARE @ChequeDate           DATE          = CAST(GETDATE() AS DATE);   -- <<< cheque date
DECLARE @ChequeBank           VARCHAR(80)   = '';             -- <<< bank name on the cheque
DECLARE @ChequeType           VARCHAR(20)   = '';             -- cheque type name from the Cheque Type master (optional)
DECLARE @BankBranch           VARCHAR(100)  = '';
DECLARE @BankAccountId        INT           = 1;              -- same as Fee Collection
DECLARE @LateFee              DECIMAL(18,2) = 0;              -- <<< late fee to charge (exact amount shown on Fee Collection, July-December)
DECLARE @ExpectedChequeAmount DECIMAL(18,2) = 31820;          -- cheque amount you expect (dues - concession - advance + late fee)
DECLARE @ExpectedAdvance      DECIMAL(18,2) = 18000;          -- advance that must be adjusted in this receipt
DECLARE @ExpectedConcession   DECIMAL(18,2) = 2000;           -- concession on the July-December invoice lines
DECLARE @Remark               VARCHAR(250)  = 'Cheque receipt (manual script) - dues Jul to Dec';
DECLARE @CreatedBy            VARCHAR(20)   = 'admin';        -- <<< user name to show as "received by"
DECLARE @Commit               BIT           = 0;              -- 0 = dry run (rollback), 1 = save

--=====================================================================================
-- 1. student
--=====================================================================================
IF @SessionId <= 0 OR LTRIM(RTRIM(@ControlNo)) = '' OR LTRIM(RTRIM(@ChequeNo)) = ''
    THROW 51000, 'Fill @SessionId, @ControlNo and @ChequeNo in the INPUT block first.', 1;

IF @LateFee <= 0
    THROW 51007, 'Fill @LateFee with the late fee shown on Fee Collection for July-December (0 makes the save procedure calculate its own late fee and the cheque would not match).', 1;

IF @FromMonth NOT BETWEEN 1 AND 12 OR @TillMonth NOT BETWEEN 1 AND 12
   OR (@FromMonth - 4 + 12) % 12 > (@TillMonth - 4 + 12) % 12
    THROW 51008, '@FromMonth / @TillMonth are not a valid session range (April = 4 ... March = 3).', 1;

DECLARE @StudentId BIGINT, @ClassCode VARCHAR(20), @SectionId INT, @StudentName VARCHAR(200);

SELECT TOP 1 @StudentId = vs.StudentId, @ClassCode = vs.ClassCode, @SectionId = vs.SectionId, @StudentName = vs.StudentName
FROM dbo.vwStudentDetails vs
WHERE vs.GroupCode = @GroupCode AND vs.BranchCode = @BranchCode
  AND vs.SessionId = @SessionId AND vs.ControlNo = @ControlNo;

IF @StudentId IS NULL
    THROW 51001, 'Student not found for this control no. and session.', 1;

--=====================================================================================
-- 2. open dues @FromMonth .. @TillMonth (same lines the save procedure accepts)
--    session order: April .. March; one-time heads (month 0) only when the range starts in April
--=====================================================================================
DECLARE @FromOrder INT = (@FromMonth - 4 + 12) % 12;
DECLARE @TillOrder INT = (@TillMonth - 4 + 12) % 12;

DECLARE @Dues TABLE
(
    RowNo        INT IDENTITY(1,1),
    InvoiceId    BIGINT,
    DetInvoiceId BIGINT,
    FeeHeadId    INT,
    MonthNo      INT,
    Amount       DECIMAL(18,2),
    Concession   DECIMAL(18,2),
    WaiveOff     DECIMAL(18,2),
    ReceivedAmt  DECIMAL(18,2),
    Payable      DECIMAL(18,2)
);

INSERT INTO @Dues (InvoiceId, DetInvoiceId, FeeHeadId, MonthNo, Amount, Concession, WaiveOff, ReceivedAmt, Payable)
SELECT d.InvoiceId, d.DetInvoiceId, d.FeeHeadId, d.MonthNo,
       ISNULL(d.Amount, 0), ISNULL(d.Concession, 0), ISNULL(d.WaiveOff, 0), ISNULL(d.ReceivedAmt, 0),
       ISNULL(d.Amount, 0) - ISNULL(d.Concession, 0) - ISNULL(d.WaiveOff, 0) - ISNULL(d.ReceivedAmt, 0)
FROM dbo.V3M_FIN_MstInvoice m
INNER JOIN dbo.V3M_FIN_DetInvoice d ON d.InvoiceId = m.InvoiceId
WHERE m.GroupCode = @GroupCode AND m.BranchCode = @BranchCode
  AND m.SessionId = @SessionId AND m.StudentId = @StudentId
  AND m.Ch_Status > 0
  AND ISNULL(d.IsValid, 1) = 1
  AND ISNULL(d.ReceivedAmt, 0) < (ISNULL(d.Amount, 0) - ISNULL(d.Concession, 0) - ISNULL(d.WaiveOff, 0))
  AND ((d.MonthNo = 0 AND @FromOrder = 0)
       OR (d.MonthNo > 0 AND (d.MonthNo - 4 + 12) % 12 BETWEEN @FromOrder AND @TillOrder))
ORDER BY CASE WHEN d.MonthNo = 0 THEN -1 ELSE (d.MonthNo - 4 + 12) % 12 END, m.InvoiceId, d.DisplayOrder, d.DetInvoiceId;

DECLARE @DuesTotal DECIMAL(18,2) = ISNULL((SELECT SUM(Payable) FROM @Dues), 0);
IF @DuesTotal <= 0
    THROW 51002, 'No open dues found for the selected months.', 1;

-- concession on every valid line of the range (also lines the concession fully covers, which are not "open")
DECLARE @ConcessionInRange DECIMAL(18,2) = ISNULL((
    SELECT SUM(ISNULL(d.Concession, 0))
    FROM dbo.V3M_FIN_MstInvoice m
    INNER JOIN dbo.V3M_FIN_DetInvoice d ON d.InvoiceId = m.InvoiceId
    WHERE m.GroupCode = @GroupCode AND m.BranchCode = @BranchCode
      AND m.SessionId = @SessionId AND m.StudentId = @StudentId
      AND m.Ch_Status > 0
      AND ISNULL(d.IsValid, 1) = 1
      AND ((d.MonthNo = 0 AND @FromOrder = 0)
           OR (d.MonthNo > 0 AND (d.MonthNo - 4 + 12) % 12 BETWEEN @FromOrder AND @TillOrder))), 0);

-- months before @FromMonth that are still open (shown only; they stay due)
DECLARE @OpenBeforeRange DECIMAL(18,2) = ISNULL((
    SELECT SUM(ISNULL(d.Amount, 0) - ISNULL(d.Concession, 0) - ISNULL(d.WaiveOff, 0) - ISNULL(d.ReceivedAmt, 0))
    FROM dbo.V3M_FIN_MstInvoice m
    INNER JOIN dbo.V3M_FIN_DetInvoice d ON d.InvoiceId = m.InvoiceId
    WHERE m.GroupCode = @GroupCode AND m.BranchCode = @BranchCode
      AND m.SessionId = @SessionId AND m.StudentId = @StudentId
      AND m.Ch_Status > 0
      AND ISNULL(d.IsValid, 1) = 1
      AND ISNULL(d.ReceivedAmt, 0) < (ISNULL(d.Amount, 0) - ISNULL(d.Concession, 0) - ISNULL(d.WaiveOff, 0))
      AND ((d.MonthNo = 0 AND @FromOrder > 0)
           OR (d.MonthNo > 0 AND (d.MonthNo - 4 + 12) % 12 < @FromOrder))), 0);

--=====================================================================================
-- 3. advance, dues before, cheque amount (Fee Collection rules)
--=====================================================================================
DECLARE @PreviousBalance DECIMAL(18,2) = ISNULL((
    SELECT TOP 1 Balance FROM dbo.V3M_Fin_StudentBalance
    WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND StudentId = @StudentId
      AND SessionId = @SessionId AND IsValid = 1), 0);

DECLARE @Advance         DECIMAL(18,2) = CASE WHEN @PreviousBalance < 0 THEN -@PreviousBalance ELSE 0 END;
DECLARE @AdvanceAdjusted DECIMAL(18,2) = CASE WHEN @Advance < @DuesTotal + @LateFee THEN @Advance ELSE @DuesTotal + @LateFee END;
DECLARE @PaidAmount      DECIMAL(18,2) = CASE WHEN @DuesTotal - @AdvanceAdjusted > 0 THEN @DuesTotal - @AdvanceAdjusted ELSE 0 END;
DECLARE @ChequeAmount    DECIMAL(18,2) = @PaidAmount + @LateFee;
DECLARE @AdvanceLeft     DECIMAL(18,2) = @Advance - @AdvanceAdjusted;
DECLARE @DuesBefore      DECIMAL(18,2) = ISNULL(dbo.Fn_GetStudentBalance(@GroupCode, @BranchCode, @SessionId, @StudentId), 0);
IF @DuesBefore < 0 SET @DuesBefore = 0;
DECLARE @Balance         DECIMAL(18,2) = CASE WHEN @AdvanceLeft > 0 THEN -@AdvanceLeft
                                              WHEN @DuesBefore - @PaidAmount > 0 THEN @DuesBefore - @PaidAmount ELSE 0 END;
DECLARE @ReceiptTotal    DECIMAL(18,2) = @DuesBefore + @LateFee;

-- preview
SELECT @ControlNo AS ControlNo, @StudentName AS StudentName, @StudentId AS StudentId, @ClassCode AS ClassCode, @SectionId AS SectionId,
       @FromMonth AS FromMonth, @TillMonth AS TillMonth,
       @DuesTotal + @ConcessionInRange AS FeeBeforeConcession, @ConcessionInRange AS Concession, @ExpectedConcession AS ExpectedConcession,
       @DuesTotal AS DuesAfterConcession, @Advance AS AdvanceAvailable, @AdvanceAdjusted AS AdvanceAdjusted, @ExpectedAdvance AS ExpectedAdvance,
       @LateFee AS LateFee, @PaidAmount AS FeePaidByCheque, @ChequeAmount AS ChequeAmount, @ExpectedChequeAmount AS ExpectedChequeAmount,
       @OpenBeforeRange AS StillDueBeforeFromMonth, @DuesBefore AS DuesBeforeReceipt, @Balance AS ReceiptBalance;

SELECT d.InvoiceId, d.DetInvoiceId, d.FeeHeadId, d.MonthNo, d.Amount, d.Concession, d.WaiveOff, d.ReceivedAmt, d.Payable AS PayingNow
FROM @Dues d ORDER BY d.RowNo;

DECLARE @Msg NVARCHAR(400);

IF @ConcessionInRange <> @ExpectedConcession
BEGIN
    SET @Msg = CONCAT('Concession on the invoice lines is ', @ConcessionInRange, ' but @ExpectedConcession is ', @ExpectedConcession,
                      '. Map and approve the concession from Fee Collection (Map concession) so the invoice lines carry it, then run again. Nothing saved.');
    THROW 51009, @Msg, 1;
END

IF @AdvanceAdjusted <> @ExpectedAdvance
BEGIN
    SET @Msg = CONCAT('Advance adjusted is ', @AdvanceAdjusted, ' (available ', @Advance, ') but @ExpectedAdvance is ', @ExpectedAdvance,
                      '. Check the student balance (V3M_Fin_StudentBalance). Nothing saved.');
    THROW 51010, @Msg, 1;
END

IF @ChequeAmount <> @ExpectedChequeAmount
BEGIN
    SET @Msg = CONCAT('Cheque amount is ', @ChequeAmount, ' (dues ', @DuesTotal, ' - advance ', @AdvanceAdjusted, ' + late fee ', @LateFee,
                      ') but @ExpectedChequeAmount is ', @ExpectedChequeAmount, '. Check @LateFee in the preview above. Nothing saved.');
    THROW 51003, @Msg, 1;
END

--=====================================================================================
-- 4. fee head lines (full due of every line, oldest first) -> save procedure
--=====================================================================================
DECLARE @FeeHeads dbo.FeeHeadDetailType;

INSERT INTO @FeeHeads (FeeHeadId, PaidAmount, LumpsumAmount, MonthNoCsv, FeeHeadAmount, Concession, Deduction, StatusFlag, DetInvoiceId, AdjustAmount)
SELECT FeeHeadId, Payable, Amount, CAST(MonthNo AS VARCHAR(10)), Amount, Concession, WaiveOff, '1', CAST(DetInvoiceId AS INT), 0
FROM @Dues
WHERE Payable > 0
ORDER BY RowNo;

DECLARE @ReceiptNo VARCHAR(50), @ReceiptId BIGINT;

BEGIN TRY
    BEGIN TRANSACTION;

    EXEC dbo.V3M_FIN_InsertCounterReceiptDetails
        @StudentId          = @StudentId,
        @GroupCode          = @GroupCode,
        @BranchCode         = @BranchCode,
        @SessionId          = @SessionId,
        @ClassCode          = @ClassCode,
        @SectionId          = @SectionId,
        @PaidAmount         = @PaidAmount,
        @Balance            = @Balance,
        @ReceiptTotalAmount = @ReceiptTotal,
        @PaymentMode        = 'Cheque',
        @ChequeNo           = @ChequeNo,
        @ChequeDate         = @ChequeDate,
        @ChequeBank         = @ChequeBank,
        @ChequeType         = @ChequeType,
        @LateFee            = @LateFee,
        @PreviousBalance    = @PreviousBalance,
        @Remark             = @Remark,
        @SocietyId          = 0,
        @CreatedBy          = @CreatedBy,
        @ReceiptDate        = @ReceiptDate,
        @BankAccountId      = @BankAccountId,
        @BankBranch         = @BankBranch,
        @FeeHeadDetails     = @FeeHeads,
        @StudentReceiptNo   = @ReceiptNo OUTPUT,
        @ReceiptId          = @ReceiptId OUTPUT;

    -- the save procedure must leave our transaction open (it has its own BEGIN/COMMIT inside)
    DECLARE @TranAfterSave INT = @@TRANCOUNT;

    IF ISNULL(@ReceiptNo, '') = '' OR ISNULL(@ReceiptId, 0) = 0
        THROW 51004, 'Receipt was not created by V3M_FIN_InsertCounterReceiptDetails (no receipt no. returned).', 1;

    IF @TranAfterSave = 0
        THROW 51006, 'The save procedure closed the transaction itself (@@TRANCOUNT = 0 after EXEC). Check V3M_FIN_StudentReceiptMaster for this receipt no. before running again.', 1;

    -- every line must have reached its invoice line (late fee line is not counted here)
    DECLARE @LinesSaved DECIMAL(18,2) = ISNULL((
        SELECT SUM(rd.PaidAmount) FROM dbo.V3M_FIN_StudentReceiptDetail rd
        INNER JOIN @Dues d ON d.InvoiceId = rd.InvoiceId AND d.FeeHeadId = rd.FeeHeadId AND d.MonthNo = rd.MonthNo
        WHERE rd.ReceiptId = @ReceiptId AND rd.GroupCode = @GroupCode AND rd.BranchCode = @BranchCode), 0);

    -- diagnostics (always shown)
    SELECT @ReceiptNo AS ReceiptNo, @ReceiptId AS ReceiptId, @TranAfterSave AS TranCountAfterSave,
           @DuesTotal AS DuesSentToSave, @LinesSaved AS LinesSavedOnReceipt, @Commit AS CommitFlag;

    IF @LinesSaved < @DuesTotal
    BEGIN
        DECLARE @LineMsg NVARCHAR(400) = CONCAT('Receipt lines saved ', @LinesSaved, ' but dues sent ', @DuesTotal,
                                                ' - the save procedure skipped some fee head lines. Nothing saved.');
        THROW 51005, @LineMsg, 1;
    END

    -- result
    SELECT ReceiptId, ReceiptNo, StudentId, PaymentMode, ChequeNo, ChequeDate, ChequeBank,
           TotalAmount, PaidAmount, LateFee, Balance, PreviousBalance, Valid, CreatedBy, CreatedDate
    FROM dbo.V3M_FIN_StudentReceiptMaster
    WHERE ReceiptId = @ReceiptId AND GroupCode = @GroupCode AND BranchCode = @BranchCode;

    SELECT InvoiceId, FeeHeadId, MonthNo, PaidAmount, FeeHeadAmount, FeeHeadConcession
    FROM dbo.V3M_FIN_StudentReceiptDetail
    WHERE ReceiptId = @ReceiptId AND GroupCode = @GroupCode AND BranchCode = @BranchCode
    ORDER BY MonthNo, FeeHeadId;

    SELECT Balance AS StudentBalanceAfter_NegativeIsAdvance
    FROM dbo.V3M_Fin_StudentBalance
    WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND StudentId = @StudentId AND SessionId = @SessionId AND IsValid = 1;

    IF @Commit = 1
    BEGIN
        -- commit every open level (the save procedure's inner COMMIT only lowers the count)
        WHILE @@TRANCOUNT > 0
            COMMIT TRANSACTION;

        SELECT 'SAVED' AS Result, @ReceiptNo AS ReceiptNo, @ReceiptId AS ReceiptId, @ChequeNo AS ChequeNo, @ChequeAmount AS ChequeAmount,
               (SELECT COUNT(1) FROM dbo.V3M_FIN_StudentReceiptMaster
                WHERE ReceiptId = @ReceiptId AND GroupCode = @GroupCode AND BranchCode = @BranchCode) AS RowsInReceiptMaster;
    END
    ELSE
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT 'DRY RUN - ROLLED BACK (set @Commit = 1 to save)' AS Result, @ReceiptNo AS ReceiptNo, @ChequeAmount AS ChequeAmount;
    END
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    -- exact reason, shown as a result row and in Messages
    SELECT 'FAILED - NOTHING SAVED' AS Result, ERROR_NUMBER() AS ErrorNumber, ERROR_MESSAGE() AS ErrorMessage,
           ERROR_PROCEDURE() AS ErrorProcedure, ERROR_LINE() AS ErrorLine, @ReceiptNo AS ReceiptNoTried;
    THROW;
END CATCH;
