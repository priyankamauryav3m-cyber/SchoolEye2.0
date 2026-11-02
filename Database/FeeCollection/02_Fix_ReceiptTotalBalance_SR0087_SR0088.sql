/*
    One-time data fix (02-11-2026), student 130303 (StudentId), session 1043:
    the two Fee Collection receipts were saved before 01_V3M_FIN_InsertCounterReceiptDetails_ReceiptTotal.sql
    with TotalAmount = 0 and Balance = 124000 (dues before the payment).

        Receipt   Paid      TotalAmount (now -> fixed)   Balance (now -> fixed)
        SR0087    40000     0 -> 124000                  124000 -> 84000
        SR0088    18000     0 -> 84000                   124000 -> 66000      (66000 = Fn_GetStudentBalance today)

    Only these two display columns of V3M_FIN_StudentReceiptMaster are changed. Invoices, ledgers,
    student balance and receipt details are already correct and are not touched.
    The WHERE clauses check the current wrong values, so running it twice changes nothing.

    Rollback: UPDATE ... SET TotalAmount = 0, Balance = 124000 for the same two receipts.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

UPDATE dbo.V3M_FIN_StudentReceiptMaster
SET TotalAmount = 124000, Balance = 84000
WHERE ReceiptNo = 'SR0087' AND StudentId = 130303 AND SessionId = 1043
  AND PaidAmount = 40000 AND TotalAmount = 0 AND Balance = 124000;
PRINT CONCAT('SR0087 updated : ', @@ROWCOUNT, ' (expected 1)');

UPDATE dbo.V3M_FIN_StudentReceiptMaster
SET TotalAmount = 84000, Balance = 66000
WHERE ReceiptNo = 'SR0088' AND StudentId = 130303 AND SessionId = 1043
  AND PaidAmount = 18000 AND TotalAmount = 0 AND Balance = 124000;
PRINT CONCAT('SR0088 updated : ', @@ROWCOUNT, ' (expected 1)');

COMMIT TRANSACTION;
