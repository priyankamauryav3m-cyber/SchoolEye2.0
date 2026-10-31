/*
    V3M_FIN_vwStudentReceiptEditTrail
    ----------------------------------------------------------------
    Reporting view: every edited receipt next to the receipt that replaced it.

    Reporting rules for edited receipts (V3M_FIN_FeeHeadLedger stays append-only):
      - Head-wise net collection : SUM(Amount) of TransType 'RECV' (IsValid = 1).
                                   Reversal rows of the edited receipt cancel out its
                                   original rows, the revised receipt adds the new ones.
      - Day book / cash report   : by TransDate. The original day keeps the original
                                   amount, the edit day shows the reversal + revised amount.
      - "Current" receipt list   : Valid = 1 receipts only (edited receipts are Valid = 0,
                                   CancelType = 2); this view shows the old -> new pairs.

    Safety  : Read-only view.
    Rollback: EditStudentReceipt_Rollback.sql
*/

SET NOCOUNT ON;
GO

--SELECT * FROM dbo.V3M_FIN_vwStudentReceiptEditTrail WHERE GroupCode = '01' AND BranchCode = '01';
CREATE OR ALTER VIEW dbo.V3M_FIN_vwStudentReceiptEditTrail
AS
SELECT old.GroupCode,
       old.BranchCode,
       old.SessionId,
       old.StudentId,
       old.ReceiptId          AS OldReceiptId,
       old.ReceiptNo          AS OldReceiptNo,
       old.CreatedDate        AS OldReceiptDate,
       old.PaidAmount         AS OldPaidAmount,
       old.PaymentMode        AS OldPaymentMode,
       old.CancelBy           AS EditedBy,
       old.CancelDate         AS EditedDate,
       old.EditReason,
       rev.ReceiptId          AS NewReceiptId,
       rev.ReceiptNo          AS NewReceiptNo,
       rev.CreatedDate        AS NewReceiptDate,
       rev.PaidAmount         AS NewPaidAmount,
       rev.PaymentMode        AS NewPaymentMode,
       ISNULL(rev.PaidAmount, 0) - ISNULL(old.PaidAmount, 0) AS PaidDifference
FROM dbo.V3M_FIN_StudentReceiptMaster old
LEFT JOIN dbo.V3M_FIN_StudentReceiptMaster rev
    ON rev.OriginalReceiptId = old.ReceiptId
   AND rev.GroupCode = old.GroupCode
   AND rev.BranchCode = old.BranchCode
WHERE old.CancelType = 2;   -- 2 = Edited (V3M_FIN_CancelStudentReceipt)
GO
