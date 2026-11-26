/* ============================================================================
   WhatsApp / SMS - how many times a message to a recipient failed
   Alert_SmsEmailToBeSend gets a nullable FailCount column (existing rows untouched).
   A resend updates the same row: failed again -> FailCount + 1, sent -> Status 1.
   Run before 02 / 03.
   Rollback: the column holds data, drop it by hand only after approval:
     ALTER TABLE dbo.Alert_SmsEmailToBeSend DROP COLUMN FailCount;
   ============================================================================ */
IF COL_LENGTH('dbo.Alert_SmsEmailToBeSend', 'FailCount') IS NULL
BEGIN
    ALTER TABLE dbo.Alert_SmsEmailToBeSend ADD FailCount int NULL;
END
GO
