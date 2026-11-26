/* ============================================================================
   Rollback of 01 / 02 / 03 (WhatsApp / SMS message log)
   - Msg_UspSMSSentMaster back to its original body (no @TemplateName)
   - Alert_Usp_SaveWhatsAppStatus and Alert_Usp_GetRecipientMessageHistory dropped
   - SMSSentMaster.TemplateName is NOT dropped here (it holds saved data);
     drop it by hand only after approval:
       ALTER TABLE dbo.SMSSentMaster DROP COLUMN TemplateName;
   ============================================================================ */
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROC [dbo].[Msg_UspSMSSentMaster]
@GroupCode varchar(50),@BranchCode varchar(50),@SMS_Or_Mail varchar(50),@MessageType varchar(50),@MessageText nvarchar(1100),
@SentDate varchar(50),@SentBy varchar(50),@TotalMsg varchar(50),@TotalDelivered varchar(50),@SMSVendor varchar(50),@IsValid bit,@isAttachment bit,
@SMS_EmailSentId varchar(50) OUTPUT
as
begin
	select  @SMS_EmailSentId=Isnull(max(SMS_EmailSentId),0)+1 from SMSSentMaster
	insert into SMSSentMaster(SMS_EmailSentId,GroupCode,BranchCode,SMS_Or_Mail,MessageType,MessageText,SentDate,SentBy,TotalMsg,TotalDelivered,
	SMSVendor,IsValid,isAttachment)
	values(@SMS_EmailSentId,@GroupCode,@BranchCode,@SMS_Or_Mail,@MessageType,@MessageText,getdate(),@SentBy,@TotalMsg,@TotalDelivered,
	@SMSVendor,@IsValid,@isAttachment)
end
GO

DROP PROCEDURE IF EXISTS [dbo].[Alert_Usp_SaveWhatsAppStatus];
GO

DROP PROCEDURE IF EXISTS [dbo].[Alert_Usp_GetRecipientMessageHistory];
GO
