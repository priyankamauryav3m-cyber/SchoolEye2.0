/*
    V3M_FIN_GetStudentReceiptForEdit
    ----------------------------------------------------------------
    View Student Receipt -> Edit popup.
      Result 1 : receipt header + student info + EditStatus
      Result 2 : fee head lines paid in this receipt (one row per DetInvoice line)
      Result 3 : other pending dues of the student (for "Add Fee Head")

    EditStatus : 1 = can edit
                -1 = receipt is cancelled / not found
                -2 = a later active receipt exists for the student (only the latest receipt can be edited,
                     because the cancel restores V3M_Fin_StudentBalance to this receipt's PreviousBalance)
                -3 = a fee head of this receipt was already removed from its invoice (old Edit Invoice flow)

    Safety  : Read-only procedure.
    Rollback: EditStudentReceipt_Rollback.sql
*/

SET NOCOUNT ON;
GO

--EXEC dbo.V3M_FIN_GetStudentReceiptForEdit @GroupCode = '01', @BranchCode = '01', @ReceiptId = 4;
CREATE OR ALTER PROCEDURE dbo.V3M_FIN_GetStudentReceiptForEdit
    @GroupCode  VARCHAR(10),
    @BranchCode VARCHAR(10),
    @ReceiptId  BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StudentId BIGINT, @SessionId BIGINT, @Valid BIT;

    SELECT @StudentId = StudentId, @SessionId = SessionId, @Valid = Valid
    FROM dbo.V3M_FIN_StudentReceiptMaster WITH (NOLOCK)
    WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND ReceiptId = @ReceiptId;

    ----------------------------------------------------------------------
    -- lines of this receipt paired with their invoice line (V3M_FIN_FnReceiptInvoiceLines)
    ----------------------------------------------------------------------
    DECLARE @Lines TABLE
    (
        DetInvoiceId BIGINT NULL, InvoiceId BIGINT, FeeHeadId INT, MonthNo INT, PaidInReceipt DECIMAL(18,2)
    );

    INSERT INTO @Lines (DetInvoiceId, InvoiceId, FeeHeadId, MonthNo, PaidInReceipt)
    SELECT f.DetInvoiceId, f.InvoiceId, f.FeeHeadId, f.MonthNo, SUM(f.PaidAmount)
    FROM dbo.V3M_FIN_FnReceiptInvoiceLines(@ReceiptId) f
    WHERE f.GroupCode = @GroupCode AND f.BranchCode = @BranchCode
    GROUP BY f.DetInvoiceId, f.InvoiceId, f.FeeHeadId, f.MonthNo;

    DECLARE @EditStatus INT =
        CASE
            WHEN ISNULL(@Valid, 0) = 0 THEN -1
            WHEN EXISTS (SELECT 1 FROM dbo.V3M_FIN_StudentReceiptMaster WITH (NOLOCK)
                         WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode
                           AND StudentId = @StudentId AND SessionId = @SessionId
                           AND Valid = 1 AND ReceiptId > @ReceiptId) THEN -2
            WHEN EXISTS (SELECT 1 FROM @Lines WHERE DetInvoiceId IS NULL) THEN -3
            ELSE 1
        END;

    ----------------------------------------------------------------------
    -- Result 1 : header
    ----------------------------------------------------------------------
    SELECT rm.ReceiptId,
           rm.ReceiptNo,
           rm.SessionId,
           rm.StudentId,
           rm.ClassCode,
           rm.SectionId,
           vs.StudentNo,
           vs.ControlNo,
           vs.RollNo,
           vs.StudentName,
           vs.ClassName,
           vs.ClassSection,
           vs.FatherName,
           vs.NextToKinContactNo,
           rm.PreviousBalance,
           rm.TotalAmount,
           rm.Concession,
           rm.LateFee,
           rm.PaidAmount,
           rm.Balance,
           (ISNULL(rm.PreviousBalance, 0) + ISNULL(rm.TotalAmount, 0) + ISNULL(rm.LateFee, 0)) - ISNULL(rm.Concession, 0) AS PayableAmount,
           rm.PaymentMode,
           rm.ChequeNo,
           rm.ChequeDate,
           rm.ChequeBank,
           rm.ChequeBankBranch,
           rm.ChequeType,
           rm.BankAccountId,
           rm.Remark,
           rm.Valid,
           rm.CreatedDate,
           rm.CreatedBy,
           rm.OriginalReceiptId,
           orm.ReceiptNo AS OriginalReceiptNo,
           ISNULL(sb.Balance, 0) AS CurrentAdvance,
           @EditStatus AS EditStatus
    FROM dbo.V3M_FIN_StudentReceiptMaster rm WITH (NOLOCK)
    INNER JOIN dbo.vwStudentDetails vs WITH (NOLOCK)
        ON vs.GroupCode COLLATE DATABASE_DEFAULT = rm.GroupCode COLLATE DATABASE_DEFAULT
       AND vs.BranchCode COLLATE DATABASE_DEFAULT = rm.BranchCode COLLATE DATABASE_DEFAULT
       AND vs.SessionId = rm.SessionId
       AND vs.StudentId = rm.StudentId
    LEFT JOIN dbo.V3M_FIN_StudentReceiptMaster orm WITH (NOLOCK)
        ON orm.ReceiptId = rm.OriginalReceiptId
    LEFT JOIN dbo.V3M_Fin_StudentBalance sb WITH (NOLOCK)
        ON sb.GroupCode = rm.GroupCode AND sb.BranchCode = rm.BranchCode
       AND sb.StudentId = rm.StudentId AND sb.SessionId = rm.SessionId AND sb.IsValid = 1
    WHERE rm.GroupCode = @GroupCode AND rm.BranchCode = @BranchCode AND rm.ReceiptId = @ReceiptId;

    ----------------------------------------------------------------------
    -- Result 2 : lines of this receipt
    --   PaidElsewhere = received on the same invoice line through other receipts
    ----------------------------------------------------------------------
    SELECT l.DetInvoiceId,
           l.InvoiceId,
           mi.InvoiceNo,
           l.FeeHeadId,
           fh.FeeHeadName,
           l.MonthNo,
           ISNULL(LEFT(md.[MonthName], 3), '') AS [MonthName],
           ISNULL(di.Amount, 0)       AS Amount,
           ISNULL(di.Concession, 0)   AS Concession,
           ISNULL(di.WaiveOff, 0)     AS WaiveOff,
           ISNULL(di.ReceivedAmt, 0)  AS ReceivedAmt,
           l.PaidInReceipt,
           ISNULL(di.ReceivedAmt, 0) - l.PaidInReceipt AS PaidElsewhere,
           CAST(1 AS BIT) AS IsInReceipt,
           ISNULL(fh.DisplayOrder, 0) AS DisplayOrder
    FROM @Lines l
    LEFT JOIN dbo.V3M_FIN_DetInvoice di WITH (NOLOCK) ON di.DetInvoiceId = l.DetInvoiceId
    LEFT JOIN dbo.V3M_FIN_MstInvoice mi WITH (NOLOCK) ON mi.InvoiceId = l.InvoiceId
    LEFT JOIN dbo.V3M_FIN_MstFeeHead fh WITH (NOLOCK)
        ON fh.FeeHeadId = l.FeeHeadId AND fh.GroupCode = @GroupCode AND fh.BranchCode = @BranchCode
    LEFT JOIN dbo.V3M_FIN_Mst_MonthDisplayOrder md WITH (NOLOCK)
        ON md.MonthNo = l.MonthNo AND md.GroupCode = @GroupCode AND md.BranchCode = @BranchCode
    ORDER BY ISNULL(fh.DisplayOrder, 0), ISNULL(md.DisplayOrder, l.MonthNo);

    ----------------------------------------------------------------------
    -- Result 3 : other pending dues of the student (Add Fee Head)
    ----------------------------------------------------------------------
    SELECT di.DetInvoiceId,
           di.InvoiceId,
           mi.InvoiceNo,
           di.FeeHeadId,
           fh.FeeHeadName,
           CAST(di.MonthNo AS INT) AS MonthNo,
           ISNULL(LEFT(md.[MonthName], 3), '') AS [MonthName],
           di.Amount,
           ISNULL(di.Concession, 0)  AS Concession,
           ISNULL(di.WaiveOff, 0)    AS WaiveOff,
           ISNULL(di.ReceivedAmt, 0) AS ReceivedAmt,
           CAST(0 AS DECIMAL(18,2))  AS PaidInReceipt,
           ISNULL(di.ReceivedAmt, 0) AS PaidElsewhere,
           CAST(0 AS BIT) AS IsInReceipt,
           ISNULL(fh.DisplayOrder, 0) AS DisplayOrder
    FROM dbo.V3M_FIN_MstInvoice mi WITH (NOLOCK)
    INNER JOIN dbo.V3M_FIN_DetInvoice di WITH (NOLOCK) ON di.InvoiceId = mi.InvoiceId
    LEFT JOIN dbo.V3M_FIN_MstFeeHead fh WITH (NOLOCK)
        ON fh.FeeHeadId = di.FeeHeadId AND fh.GroupCode = @GroupCode AND fh.BranchCode = @BranchCode
    LEFT JOIN dbo.V3M_FIN_Mst_MonthDisplayOrder md WITH (NOLOCK)
        ON md.MonthNo = di.MonthNo AND md.GroupCode = @GroupCode AND md.BranchCode = @BranchCode
    WHERE mi.GroupCode = @GroupCode AND mi.BranchCode = @BranchCode
      AND mi.StudentId = @StudentId AND mi.SessionId = @SessionId
      AND mi.Ch_Status > 0
      AND di.Amount - ISNULL(di.Concession, 0) - ISNULL(di.WaiveOff, 0) - ISNULL(di.ReceivedAmt, 0) > 0
      AND NOT EXISTS (SELECT 1 FROM @Lines l WHERE l.DetInvoiceId = di.DetInvoiceId)
    ORDER BY mi.InvoiceId, ISNULL(fh.DisplayOrder, 0), ISNULL(md.DisplayOrder, di.MonthNo);
END
GO
