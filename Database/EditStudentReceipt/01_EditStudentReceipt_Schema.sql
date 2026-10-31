/*
    Edit Student Receipt — schema
    ----------------------------------------------------------------
    View Student Receipt -> Edit works as "cancel & re-issue":
      old receipt  -> cancelled with CancelType = 2 (Edited), IsEdited = 1
      new receipt  -> new receipt number, OriginalReceiptId = old ReceiptId
    so every rupee stays traceable in V3M_FIN_FeeHeadLedger (append-only).

    Change  : two new NULLable columns on V3M_FIN_StudentReceiptMaster
              + one table type for the edited fee head lines
              + one inline function that pairs receipt detail rows with their invoice line.
    Safety  : Additive only. Existing rows / procedures are not affected
              (all existing INSERTs name their columns).
    Index   : none — OriginalReceiptId is read only for one receipt at a time
              (edit trail), the table is always filtered by ReceiptId / StudentId.
    Rollback: EditStudentReceipt_Rollback.sql
*/

SET NOCOUNT ON;
GO

IF COL_LENGTH('dbo.V3M_FIN_StudentReceiptMaster', 'OriginalReceiptId') IS NULL
    ALTER TABLE dbo.V3M_FIN_StudentReceiptMaster ADD OriginalReceiptId INT NULL;   -- revised receipt -> the receipt it replaced
GO

IF COL_LENGTH('dbo.V3M_FIN_StudentReceiptMaster', 'EditReason') IS NULL
    ALTER TABLE dbo.V3M_FIN_StudentReceiptMaster ADD EditReason VARCHAR(250) NULL;
GO

-- one row per DetInvoice line of the revised receipt
IF TYPE_ID('dbo.EditReceiptLineType') IS NULL
    CREATE TYPE dbo.EditReceiptLineType AS TABLE
    (
        DetInvoiceId      BIGINT        NOT NULL PRIMARY KEY,
        PaidAmount        DECIMAL(18,2) NOT NULL,
        RemoveFromInvoice BIT           NOT NULL DEFAULT 0   -- 1 = also remove this fee head from the invoice
    );
GO

/*
    V3M_FIN_FnReceiptInvoiceLines
    Receipt detail rows of one receipt with the V3M_FIN_DetInvoice line each row paid.
    ReceiptDetail has no DetInvoiceId, so rows are matched on InvoiceId + FeeHeadId + MonthNo.
    A late fee invoice has one line per parent invoice with the SAME head and month,
    so a plain join multiplies rows (3 lines -> 9 matches). Rows are paired in order
    instead (detail Sid order = DetInvoiceId order, both inserted per parent invoice).
    DetInvoiceId is NULL when the invoice line no longer exists.
*/
--SELECT * FROM dbo.V3M_FIN_FnReceiptInvoiceLines(4);
CREATE OR ALTER FUNCTION dbo.V3M_FIN_FnReceiptInvoiceLines (@ReceiptId BIGINT)
RETURNS TABLE
AS
RETURN
(
    WITH rdx AS
    (
        SELECT rd.Sid, rd.GroupCode, rd.BranchCode, rd.InvoiceId, rd.FeeHeadId, rd.MonthNo,
               ISNULL(rd.PaidAmount, 0) AS PaidAmount,
               ROW_NUMBER() OVER (PARTITION BY rd.InvoiceId, rd.FeeHeadId, rd.MonthNo ORDER BY rd.Sid) AS rn
        FROM dbo.V3M_FIN_StudentReceiptDetail rd
        WHERE rd.ReceiptId = @ReceiptId AND rd.Valid = 1 AND rd.InvoiceId IS NOT NULL
    ),
    dix AS
    (
        SELECT di.DetInvoiceId, di.InvoiceId, di.FeeHeadId, di.MonthNo,
               ROW_NUMBER() OVER (PARTITION BY di.InvoiceId, di.FeeHeadId, di.MonthNo ORDER BY di.DetInvoiceId) AS rn
        FROM dbo.V3M_FIN_DetInvoice di
        WHERE di.InvoiceId IN (SELECT InvoiceId FROM rdx)
    )
    SELECT rdx.Sid, rdx.GroupCode, rdx.BranchCode, rdx.InvoiceId,
           CAST(rdx.FeeHeadId AS INT) AS FeeHeadId, rdx.MonthNo, rdx.PaidAmount, dix.DetInvoiceId
    FROM rdx
    LEFT JOIN dix
        ON dix.InvoiceId = rdx.InvoiceId AND dix.FeeHeadId = rdx.FeeHeadId
       AND dix.MonthNo = rdx.MonthNo AND dix.rn = rdx.rn
);
GO
