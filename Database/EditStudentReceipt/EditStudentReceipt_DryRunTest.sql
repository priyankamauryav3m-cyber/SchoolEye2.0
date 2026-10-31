/*
    Edit Student Receipt — DRY RUN TEST (nothing is saved)
    ----------------------------------------------------------------
    Deploys scripts 01..06 and runs the scenarios INSIDE one transaction, then ROLLBACK.
    Data used: dev DB SchoolEyeV2, student 130303, receipts 3 / 4 (change ids for another DB).

    Run from this folder (sqlcmd -b stops at the first error; the open transaction is
    rolled back when the connection closes):
        sqlcmd -S <server> -d SchoolEyeV2 -U <user> -C -I -b -W -s"|" -i EditStudentReceipt_DryRunTest.sql

    Expected:
      T1  3 late fee rows paired 400/400/380 -> DetInvoice 3559/3560/3561
      T2  receipt 4 EditStatus = -3 (head 12234 month 3 already removed through Edit Invoice)
      T3  Edit Invoice remove of a paid head -> -2
      T4  edit receipt 3 while receipt 4 is active -> -2
      T5  Result 1, new receipt no; ledger: Pro_V3M_FIN_CancelStudentReceipt mirror rows,
          Pro_FIN_Usp_RemoveFeeHeadFromChallan only DUE (no RECV / SB), new RECV rows,
          student advance -11000, invoice 1388 Ch_Status 3, journal of receipt 3 IsValid 0
      T6  details-only edit -> 2 (same receipt no), then -8
*/

SET NOCOUNT ON;
BEGIN TRANSACTION;
GO
:r .\01_EditStudentReceipt_Schema.sql
:r .\02_V3M_FIN_GetStudentReceiptForEdit.sql
:r .\03_V3M_FIN_EditStudentReceipt.sql
:r .\04_FIN_Usp_RemoveFeeHeadFromChallan_BlockPaidHead.sql
:r .\05_V3M_FIN_vwStudentReceiptEditTrail.sql
:r .\06_V3M_FIN_CancelStudentReceipt_LineMatchFix.sql

PRINT '== T1 pairing of late fee lines';
SELECT Sid, InvoiceId, FeeHeadId, MonthNo, PaidAmount, DetInvoiceId FROM dbo.V3M_FIN_FnReceiptInvoiceLines(4) WHERE InvoiceId = 1396;
PRINT '== T2 load receipt 4';
EXEC dbo.V3M_FIN_GetStudentReceiptForEdit '01', '01', 4;
GO

PRINT '== T3 Edit Invoice remove of a PAID head';
DECLARE @r INT = 0;
EXEC dbo.FIN_Usp_RemoveFeeHeadFromChallan 130303, 1385, 13236, 10, 'DRYRUN', '', @r OUTPUT;
SELECT @r AS RemoveResult;
GO

PRINT '== T4 edit receipt 3 while receipt 4 is active';
DECLARE @L dbo.EditReceiptLineType, @id BIGINT, @no VARCHAR(50), @r INT;
EXEC dbo.V3M_FIN_EditStudentReceipt '01', '01', 3, 'Cash', '', NULL, '', '', '', 1, '2026-10-19', '', 'dry run', 1, 'DRYRUN', @L, @id OUTPUT, @no OUTPUT, @r OUTPUT;
SELECT @r AS Result;
GO

PRINT '== T5 cancel receipt 4, then edit receipt 3: remove 3542 (10000) from invoice, 3470 paid 1000 -> 0, keep as advance';
DECLARE @c INT;
EXEC dbo.V3M_FIN_CancelStudentReceipt 4, '01', '01', 'DRYRUN', 'dry run', 1, @c OUTPUT;
SELECT @c AS CancelReceipt4;
SELECT DetInvoiceId, ReceivedAmt, Status FROM dbo.V3M_FIN_DetInvoice WHERE DetInvoiceId IN (3559, 3560, 3561);

DECLARE @L dbo.EditReceiptLineType, @id BIGINT, @no VARCHAR(50), @r INT;
INSERT INTO @L (DetInvoiceId, PaidAmount, RemoveFromInvoice)
SELECT f.DetInvoiceId,
       CASE WHEN f.DetInvoiceId IN (3470, 3542) THEN 0 ELSE f.PaidAmount END,
       CASE WHEN f.DetInvoiceId = 3542 THEN 1 ELSE 0 END
FROM dbo.V3M_FIN_FnReceiptInvoiceLines(3) f;

DECLARE @Today DATE = CAST(GETDATE() AS DATE);
EXEC dbo.V3M_FIN_EditStudentReceipt '01', '01', 3, 'Cash', '', NULL, '', '', '', 1, @Today, 'revised', 'fee head removed', 1, 'DRYRUN', @L, @id OUTPUT, @no OUTPUT, @r OUTPUT;
SELECT @r AS Result, @id AS NewReceiptId, @no AS NewReceiptNo;

SELECT ReceiptId, ReceiptNo, Valid, CancelType, IsEdited, OriginalReceiptId, EditReason, PreviousBalance, TotalAmount, Concession, LateFee, PaidAmount, Balance
FROM dbo.V3M_FIN_StudentReceiptMaster WHERE ReceiptId IN (3, @id);

PRINT '-- ledger rows written in this test';
SELECT TransType, CrDr, EnteredVia, ReceiptId, InvoiceId, HeadId, COUNT(*) AS Cnt, SUM(Amount) AS Amt
FROM dbo.V3M_FIN_FeeHeadLedger WHERE TransBy = 'DRYRUN'
GROUP BY TransType, CrDr, EnteredVia, ReceiptId, InvoiceId, HeadId
ORDER BY EnteredVia, TransType;

PRINT '-- net RECV per head of student 130303';
SELECT HeadId, SUM(Amount) AS NetRecv FROM dbo.V3M_FIN_FeeHeadLedger WHERE RefId = '130303' AND TransType = 'RECV' AND IsValid = 1 GROUP BY HeadId;
SELECT Balance AS StudentAdvance FROM dbo.V3M_Fin_StudentBalance WHERE StudentId = 130303 AND IsValid = 1;
SELECT DetInvoiceId, InvoiceId, FeeHeadId, MonthNo, Amount, ReceivedAmt, Status FROM dbo.V3M_FIN_DetInvoice
WHERE DetInvoiceId IN (3470, 3542) OR (InvoiceId = 1388 AND FeeHeadId = 5 AND MonthNo = 10);
SELECT InvoiceId, TotalAmount, ReceivedAmount, Ch_Status FROM dbo.V3M_FIN_MstInvoice WHERE InvoiceId IN (1385, 1388);
SELECT BillNo, IsValid, IsModified, Amount FROM dbo.V3M_Fin_MstJournalLedger WHERE TransType = 'REC' AND BillNo IN ('3', CAST(@id AS VARCHAR(20)));
SELECT ColumnName, OldColumnValue, NewColumnValue, OperationType FROM dbo.Det_TransactionLog WHERE TransBy = 'DRYRUN' ORDER BY LogId;
SELECT * FROM dbo.V3M_FIN_vwStudentReceiptEditTrail WHERE OldReceiptId = 3;

PRINT '== T6 details-only edit on the revised receipt, then the same again (no change)';
DELETE FROM @L;
INSERT INTO @L (DetInvoiceId, PaidAmount, RemoveFromInvoice)
SELECT f.DetInvoiceId, SUM(f.PaidAmount), 0 FROM dbo.V3M_FIN_FnReceiptInvoiceLines(@id) f GROUP BY f.DetInvoiceId;
DECLARE @id2 BIGINT, @no2 VARCHAR(50), @r2 INT;
EXEC dbo.V3M_FIN_EditStudentReceipt '01', '01', @id, 'Cash', '', NULL, '', '', '', 1, @Today, 'remark changed', 'typo', 1, 'DRYRUN', @L, @id2 OUTPUT, @no2 OUTPUT, @r2 OUTPUT;
SELECT @r2 AS DetailsResult, @no2 AS SameReceiptNo;
EXEC dbo.V3M_FIN_EditStudentReceipt '01', '01', @id, 'Cash', '', NULL, '', '', '', 1, @Today, 'remark changed', 'typo', 1, 'DRYRUN', @L, @id2 OUTPUT, @no2 OUTPUT, @r2 OUTPUT;
SELECT @r2 AS NoChangeResult;
GO

PRINT '== ROLLBACK';
IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
SELECT COUNT(*) AS LeftoverDryRunRows FROM dbo.V3M_FIN_FeeHeadLedger WHERE TransBy = 'DRYRUN';
SELECT OBJECT_ID('dbo.V3M_FIN_EditStudentReceipt') AS EditSpAfterRollback;   -- NULL = nothing was saved
GO
