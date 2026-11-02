/*
    One-time cleanup (02-11-2026): Edit Invoice -> Add Fee Head (Transport, September) ran 3 times before the
    transport branch existed in FIN_Usp_AddFeeHeadToStudentChallan and added rows with FeeHeadId = 0, Amount = 0:
      Invoice 0101IN1602 (InvoiceId 3, StudentId 130303) : V3M_FIN_DetInvoice 34, 35, 36 + 3 V3M_FIN_FeeHeadLedger rows
    Amount is 0, so invoice totals and student balance are not affected.

    Nothing is deleted: the rows are only marked IsValid = 0.
    V3M_FIN_StudentLedger has no IsValid column; its 3 zero-amount entries ("Added fee head = ,  Month = September")
    are kept as they are (no effect on the running balance).

    Rollback: run the same UPDATEs with IsValid = 1.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

UPDATE dbo.V3M_FIN_DetInvoice
SET IsValid = 0
WHERE InvoiceId = 3 AND DetInvoiceId IN (34, 35, 36)
  AND FeeHeadId = 0 AND Amount = 0 AND ISNULL(ReceivedAmt, 0) = 0 AND IsValid = 1;
PRINT CONCAT('V3M_FIN_DetInvoice rows invalidated : ', @@ROWCOUNT, ' (expected 3)');

UPDATE dbo.V3M_FIN_FeeHeadLedger
SET IsValid = 0
WHERE InvoiceId = 3 AND HeadId = 0 AND Amount = 0 AND MonthNoFor = 9 AND IsValid = 1
  AND EnteredVia = 'Pro_FIN_Usp_AddFeeHeadToStudentChallan' AND TransDate >= '2026-11-02';
PRINT CONCAT('V3M_FIN_FeeHeadLedger rows invalidated : ', @@ROWCOUNT, ' (expected 3)');

COMMIT TRANSACTION;
