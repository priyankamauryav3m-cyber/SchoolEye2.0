/*
    One-time data fix (02-11-2026), student 130341 (StudentId), session 1043.
    Only TotalAmount and Balance of V3M_FIN_StudentReceiptMaster are changed (display columns).
    Invoices, ledgers, receipt details and V3M_Fin_StudentBalance are already correct and are not touched.

    Why wrong:
      - SR0092..SR0094: Fee Collection kept the student's dues from when the student was selected
        (1,24,000) and did not reload them after each receipt.
      - SR0095..SR0097: Edit Receipt saved Total = fee head lines - late fee and the old receipt's Balance.

    Correct values = dues just before the receipt (open invoices + student balance), worked out from the
    invoice / ledger history. Fee heads taken off the invoice during each edit (from V3M_FIN_FeeHeadLedger):
      SR0095 removed Transport Feb (5,000), SR0096 removed Library Feb (1,000), SR0097 removed Transport Jan (5,000).

      Receipt  Valid  Paid     Total now -> fixed       Balance now -> fixed
      SR0092   yes    18000    124000 -> 84000          106000 -> 66000    (SR0091 40,000 was still valid then)
      SR0093   yes    33000    124000 -> 66000           91000 -> 33000
      SR0094   no     34220    125220 -> 34220           91000 -> 0        (33,000 dues + 1,220 late fee)
      SR0095   no     29220     28000 -> 69220           91000 -> 40000    (after SR0094 cancel + SR0091 refund)
      SR0096   no     29220     27000 -> 68220           -1000 -> 39000
      SR0097   yes    24220     22000 -> 63220           -1000 -> 39000    (39,000 = Fn_GetStudentBalance today:
                                                                            40,000 invoice dues - 1,000 advance)
    SR0091 (124000 / 40000 / 84000) is already right.

    Each UPDATE checks the current wrong values, so running the script twice changes nothing.
    Rollback: run the same UPDATEs with the "now" values above.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @Fix TABLE (ReceiptNo VARCHAR(50), Paid DECIMAL(18,2), OldTotal DECIMAL(18,2), OldBalance DECIMAL(18,2),
                    NewTotal DECIMAL(18,2), NewBalance DECIMAL(18,2));
INSERT INTO @Fix VALUES
    ('SR0092', 18000, 124000, 106000, 84000, 66000),
    ('SR0093', 33000, 124000,  91000, 66000, 33000),
    ('SR0094', 34220, 125220,  91000, 34220,     0),
    ('SR0095', 29220,  28000,  91000, 69220, 40000),
    ('SR0096', 29220,  27000,  -1000, 68220, 39000),
    ('SR0097', 24220,  22000,  -1000, 63220, 39000);

UPDATE rm
SET rm.TotalAmount = f.NewTotal,
    rm.Balance     = f.NewBalance
FROM dbo.V3M_FIN_StudentReceiptMaster rm
INNER JOIN @Fix f ON f.ReceiptNo COLLATE DATABASE_DEFAULT = rm.ReceiptNo COLLATE DATABASE_DEFAULT
WHERE rm.GroupCode = '01' AND rm.BranchCode = '01'
  AND rm.StudentId = 130341 AND rm.SessionId = 1043
  AND rm.PaidAmount = f.Paid AND rm.TotalAmount = f.OldTotal AND rm.Balance = f.OldBalance;

PRINT CONCAT('Receipts updated : ', @@ROWCOUNT, ' (expected 6)');

SELECT ReceiptNo, Valid, TotalAmount, PaidAmount, Balance, TotalAmount - PaidAmount AS TotalMinusPaid
FROM dbo.V3M_FIN_StudentReceiptMaster
WHERE GroupCode = '01' AND BranchCode = '01' AND StudentId = 130341 AND SessionId = 1043
ORDER BY ReceiptId;

COMMIT TRANSACTION;
