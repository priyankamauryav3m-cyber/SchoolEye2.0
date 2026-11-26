using DomainModel.Admin;
using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationInterface.SuperAdmin
{
    public interface ISendSMS
    {
        public  Task<string> SaveSMSSentDetails(SMSSentModel model);
        Task<bool> SendSmsAsync(string mobileNo, string message);

        // ---------------- WhatsApp (Pinnacle) ----------------

        // student + father's WhatsApp number of a registration (Adm_Registration + ADM_ParentDetails)
        Task<WhatsAppRegistrationContact?> GetRegistrationWhatsAppContactAsync(long registrationId, string groupCode, string branchCode);

        // WHATSAPPPROVIDER / WHATSAPPTOKEN / WHATSAPPAPIKEY of the group / branch (DefaultSettings)
        Task<WhatsAppProviderSettings> GetWhatsAppSettingsAsync(string groupCode, string branchCode);

        // sends the message to the father's WhatsApp number; Success only when Pinnacle accepts it.
        // With SmsEmailSentId the status is saved in Alert_SmsEmailToBeSend (1 sent / 0 not sent).
        Task<WhatsAppSendResult> SendRegistrationWhatsAppAsync(WhatsAppRegistrationMessageRequest request, string? sentBy, CancellationToken cancellationToken = default);

        // ---------------- SMS (Pinnacle) ----------------

        // SMSProvider / SMSUserName / SMSPassword / SMSSenderId of the group / branch (DefaultSettings)
        Task<SmsProviderSettings> GetSmsSettingsAsync(string groupCode, string branchCode);

        // sends the SMS to the parent's SMS mobile number (ADM_ParentDetails.SMSMobileNo) (message text only, no template); Success only when the provider accepts it.
        // With SmsEmailSentId the status is saved in Alert_SmsEmailToBeSend (S, 1 sent / 0 not sent).
        Task<WhatsAppSendResult> SendRegistrationSmsAsync(WhatsAppRegistrationMessageRequest request, string? sentBy, CancellationToken cancellationToken = default);

        // every WhatsApp / SMS attempt (sent and failed) of the given recipients, latest first
        Task<List<MessageHistoryRow>> GetMessageHistoryAsync(MessageHistoryRequest request);

        // SMS balance of the Pinnacle account (SmsSettings:BalanceUrl with SmsSettings UserName / Password)
        Task<SmsBalanceResult> GetSmsBalanceAsync(CancellationToken cancellationToken = default);
    }
}
