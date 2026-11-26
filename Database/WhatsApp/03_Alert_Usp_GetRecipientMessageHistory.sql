/* ============================================================================
   WhatsApp / SMS - every send attempt (sent and failed) of the given recipients
   @RecipientIds : comma separated ids, e.g. '100132,100091' (RegistrationId)
   TemplateName  : WhatsApp template of the sent text (DetSMSEmailText.WhatsAppTemplateName)
   FailCount     : how many times the message to that recipient failed
   Status        : 1 = sent, 0 = not sent / failed (99 = failed, older rows)
   CanResend = 1 : the LAST attempt of a message (SMS_EmailSentId + type) to a
                   recipient, and it failed (older failed attempts are history only)
   Rollback: 99_WhatsApp_Rollback.sql
   ============================================================================ */
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROC [dbo].[Alert_Usp_GetRecipientMessageHistory]
    @GroupCode      varchar(10),
    @BranchCode     varchar(10),
    @RecipientType  varchar(50),
    @RecipientIds   varchar(max)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Ids TABLE (RecipientId varchar(50) PRIMARY KEY);
    INSERT INTO @Ids (RecipientId)
    SELECT DISTINCT LTRIM(RTRIM(value))
    FROM STRING_SPLIT(@RecipientIds, ',')
    WHERE LTRIM(RTRIM(value)) <> '';

    SELECT A.SmsEmailId,
           A.SMS_EmailSentId,
           A.MessageTextType,
           A.MessageTypeId,
           A.MessageText,
           T.WhatsAppTemplateName AS TemplateName,
           A.MobileNo,
           A.RecipientType,
           A.RecipientId,
           A.[Status],
           A.SentBy,
           A.SentDate,
           A.SessionId,
           ISNULL(A.FailCount, CASE WHEN ISNULL(A.[Status], 0) <> 1 THEN 1 ELSE 0 END) AS FailCount,
           CAST(CASE WHEN ISNULL(A.[Status], 0) <> 1   -- 0 = not sent (99 = failed, older rows)
                      AND NOT EXISTS (SELECT 1
                                      FROM Alert_SmsEmailToBeSend L WITH (NOLOCK)
                                      WHERE L.SMS_EmailSentId = A.SMS_EmailSentId
                                        AND L.MessageTextType = A.MessageTextType
                                        AND L.RecipientType   = A.RecipientType
                                        AND L.RecipientId     = A.RecipientId
                                        AND L.SmsEmailId      > A.SmsEmailId)
                     THEN 1 ELSE 0 END AS bit) AS CanResend
    FROM Alert_SmsEmailToBeSend A WITH (NOLOCK)
    INNER JOIN @Ids I ON I.RecipientId = A.RecipientId COLLATE DATABASE_DEFAULT
    -- template name of the text that was sent (not saved in the log)
    OUTER APPLY (SELECT TOP (1) NULLIF(LTRIM(RTRIM(D.WhatsAppTemplateName)), '') AS WhatsAppTemplateName
                 FROM DetSMSEmailText D WITH (NOLOCK)
                 WHERE A.MessageTextType = 'W'
                   AND D.GroupCode       = A.GroupCode COLLATE DATABASE_DEFAULT
                   AND D.BranchCode      = A.BranchCode COLLATE DATABASE_DEFAULT
                   AND D.MessageTypeId   = A.MessageTypeId
                   AND CAST(D.SMSEmailText AS nvarchar(1500)) = A.MessageText COLLATE DATABASE_DEFAULT
                 ORDER BY D.IsValid DESC, D.SmsEmailTextId DESC) T
    WHERE A.GroupCode     = @GroupCode
      AND A.BranchCode    = @BranchCode
      AND A.RecipientType = @RecipientType
    ORDER BY A.SentDate DESC, A.SmsEmailId DESC;
END
GO
