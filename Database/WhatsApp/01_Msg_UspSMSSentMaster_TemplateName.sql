/* ============================================================================
   Msg_UspSMSSentMaster - original body (NO TemplateName)
   SMSSentMaster.TemplateName was removed: the WhatsApp template name is not
   saved in the message log. It comes from the selected text
   (DetSMSEmailText.WhatsAppTemplateName) at send time.
   Run this on every database where the earlier version (with @TemplateName)
   was applied, otherwise the insert fails on the missing column.
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
