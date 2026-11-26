/* ============================================================================
   WhatsApp / SMS (Pinnacle) - row of a message to one recipient
   Flow of one send:
     1. before the provider is called: @SmsEmailId NULL, @Status 0
          -> NEW row in Alert_SmsEmailToBeSend with Status 0
     2. after the provider answered:   @SmsEmailId = that row
          sent   -> Status 1
          failed -> Status stays 0, FailCount + 1 (@AddFail = 1)
   Resend of a failed row: step 2 again on the same row (no new row).
   SMSSentMaster.TotalDelivered = number of recipients of the message that got it.
   Needs 04_Alert_SmsEmailToBeSend_FailCount.sql.
   Rollback: 99_WhatsApp_Rollback.sql
   ============================================================================ */
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROC [dbo].[Alert_Usp_SaveWhatsAppStatus]
    @GroupCode          varchar(10),
    @BranchCode         varchar(10),
    @SMS_EmailSentId    varchar(50),
    @MessageTypeId      int,
    @MessageText        nvarchar(1100),
    @MobileNo           varchar(50),
    @SmsEmailRecipient  varchar(50),
    @RecipientType      varchar(50),
    @RecipientId        varchar(50),
    @Status             tinyint,               -- 0 = not sent (yet), 1 = sent
    @SentBy             varchar(50),
    @SessionId          bigint = NULL,
    @SmsEmailId         int = NULL OUTPUT,     -- in: row to update (step 2 / resend), out: the row
    @MessageTextType    char(1) = 'W',         -- W = WhatsApp, S = SMS
    @AddFail            bit = 0,               -- 1 = this attempt failed (FailCount + 1)
    @FailCount          int = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @Status NOT IN (0, 1)
        THROW 50001, 'Status must be 0 (not sent) or 1 (sent).', 1;

    IF @MessageTextType NOT IN ('W', 'S')
        THROW 50002, 'MessageTextType must be W (WhatsApp) or S (SMS).', 1;

    BEGIN TRAN;

        DECLARE @RowId int = NULL;

        -- step 2 / resend: only the same message to the same recipient of this branch
        IF @SmsEmailId IS NOT NULL
            SELECT @RowId = SmsEmailId
            FROM Alert_SmsEmailToBeSend WITH (UPDLOCK, HOLDLOCK)
            WHERE SmsEmailId      = @SmsEmailId
              AND GroupCode       = @GroupCode
              AND BranchCode      = @BranchCode
              AND SMS_EmailSentId = @SMS_EmailSentId
              AND MessageTextType = @MessageTextType
              AND RecipientType   = @RecipientType
              AND RecipientId     = @RecipientId;

        IF @RowId IS NULL
        BEGIN
            -- same id rule as Alert_Usp_SmsEmailToBeSend (table has no identity)
            SELECT @RowId = ISNULL(MAX(SmsEmailId), 0) + 1
            FROM Alert_SmsEmailToBeSend WITH (UPDLOCK, HOLDLOCK);

            INSERT INTO Alert_SmsEmailToBeSend (SmsEmailId, GroupCode, BranchCode, SMS_EmailSentId, MessageTextType, MessageTypeId, MessageText,
                                                EmailId, MobileNo, SmsEmailRecipient, RecipientType, RecipientId, [Status], [Priority], SentBy, SentDate,
                                                SessionId, FailCount)
            VALUES (@RowId, @GroupCode, @BranchCode, @SMS_EmailSentId, @MessageTextType, @MessageTypeId, @MessageText,
                    NULL, @MobileNo, @SmsEmailRecipient, @RecipientType, @RecipientId, @Status, 0, @SentBy, GETDATE(),
                    @SessionId, CASE WHEN @AddFail = 1 THEN 1 ELSE 0 END);
        END
        ELSE
        BEGIN
            UPDATE Alert_SmsEmailToBeSend
               SET [Status]          = @Status,
                   FailCount         = ISNULL(FailCount, 0) + CASE WHEN @AddFail = 1 THEN 1 ELSE 0 END,
                   MobileNo          = ISNULL(@MobileNo, MobileNo),
                   SmsEmailRecipient = ISNULL(@SmsEmailRecipient, SmsEmailRecipient),
                   SessionId         = ISNULL(@SessionId, SessionId),
                   SentBy            = @SentBy,
                   SentDate          = GETDATE()
             WHERE SmsEmailId = @RowId;
        END

        SET @SmsEmailId = @RowId;

        SELECT @FailCount = ISNULL(FailCount, 0)
        FROM Alert_SmsEmailToBeSend
        WHERE SmsEmailId = @RowId;

        UPDATE SMSSentMaster
           SET TotalDelivered = (SELECT COUNT(DISTINCT A.RecipientType + '|' + A.RecipientId)
                                 FROM Alert_SmsEmailToBeSend A
                                 WHERE A.SMS_EmailSentId = @SMS_EmailSentId
                                   AND A.MessageTextType = @MessageTextType
                                   AND A.[Status] = 1)
         WHERE CAST(SMS_EmailSentId AS varchar(50)) = @SMS_EmailSentId;

    COMMIT TRAN;
END
GO
