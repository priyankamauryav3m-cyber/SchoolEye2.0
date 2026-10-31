/*
    Data fix — student advance of StudentId 130303 (GroupCode 01, BranchCode 01, Session 1043)
    ----------------------------------------------------------------
    V3M_Fin_StudentBalance.Balance = -5000, but the SB rows of V3M_FIN_FeeHeadLedger add up to -6000:
        -1000  SR0071 revised receipt (advance)
        +1000  SR0071 cancelled as Edited
        -1000  SR0072 revised receipt (advance)
        -5000  Tuition Fee (Dec) removed through Edit Invoice (paid amount -> advance)
    Cause: SR0069 (older receipt) was cancelled at 11:59 after SR0072 (11:50); the old
    V3M_FIN_CancelStudentReceipt set Balance = SR0069.PreviousBalance (0) and wiped the -1000.
    Fixed for the future in 06_V3M_FIN_CancelStudentReceipt_LineMatchFix.sql.

    Safety  : one row, only when the table is still -5000 AND the ledger is still -6000
              (otherwise nothing is changed and the values are printed). Transaction + audit
              row in Det_TransactionLog (OperationType 'DATAFIX').
    Rollback: bottom of this file (commented).
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @GroupCode VARCHAR(10) = '01', @BranchCode VARCHAR(10) = '01',
        @StudentId BIGINT = 130303, @SessionId BIGINT = 1043,
        @Expected DECIMAL(18,2) = -5000, @Ledger DECIMAL(18,2), @Current DECIMAL(18,2);

BEGIN TRANSACTION;

SELECT @Ledger = ISNULL(SUM(Amount), 0)
FROM dbo.V3M_FIN_FeeHeadLedger
WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode
  AND RefType = 'STUDENT' AND RefId = CAST(@StudentId AS VARCHAR(20))
  AND TransType = 'SB' AND IsValid = 1
  AND SessionFor = CAST(@SessionId AS VARCHAR(20));

SELECT @Current = Balance
FROM dbo.V3M_Fin_StudentBalance WITH (UPDLOCK, HOLDLOCK)
WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND StudentId = @StudentId
  AND SessionId = @SessionId AND IsValid = 1;

SELECT @Current AS BalanceBefore, @Ledger AS SBLedgerTotal;

IF @Current = @Expected AND @Ledger = -6000
BEGIN
    UPDATE dbo.V3M_Fin_StudentBalance
    SET Balance = @Ledger
    WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND StudentId = @StudentId
      AND SessionId = @SessionId AND IsValid = 1 AND Balance = @Expected;

    DECLARE @Audit dbo.ColumnChangeType;
    INSERT INTO @Audit (ColumnName, OldValue, NewValue)
    VALUES ('Balance', CAST(@Current AS VARCHAR(20)), CAST(@Ledger AS VARCHAR(20))),
           ('Reason', NULL, 'Advance wiped by cancel of older receipt SR0069; set equal to SB ledger');

    EXEC dbo.Usp_SaveTransactionLog
        @ReferenceKey = 'V3M_Fin_StudentBalance', @ReferenceId = '130303',
        @TableName = 'V3M_Fin_StudentBalance', @OperationType = 'DATAFIX',
        @TransBy = 'DataFix', @IP = NULL, @Changes = @Audit;

    COMMIT TRANSACTION;
    PRINT 'Balance corrected.';
END
ELSE
BEGIN
    ROLLBACK TRANSACTION;
    PRINT 'Values are not as expected — nothing changed.';
END

SELECT Balance AS BalanceAfter
FROM dbo.V3M_Fin_StudentBalance
WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND StudentId = @StudentId
  AND SessionId = @SessionId AND IsValid = 1;

/* ---------------- Rollback ----------------
UPDATE dbo.V3M_Fin_StudentBalance SET Balance = -5000
WHERE GroupCode = '01' AND BranchCode = '01' AND StudentId = 130303 AND SessionId = 1043 AND IsValid = 1 AND Balance = -6000;
*/
