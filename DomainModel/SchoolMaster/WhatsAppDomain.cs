using System.ComponentModel.DataAnnotations;

namespace DomainModel.SchoolMaster
{
    #region -------------------------- WhatsApp (Pinnacle / Pinbot) -------------

    // appsettings.json -> "WhatsAppSettings". Token / ApiKey here are only the fallback:
    // the branch's own values in DefaultSettings (WHATSAPPTOKEN / WHATSAPPAPIKEY) are used first.
    public class WhatsAppSettings
    {
        public const string SectionName = "WhatsAppSettings";
        public const string HttpClientName = "PinnacleWhatsApp";

        // e.g. https://partnersv1.pinbot.ai/v3/{Token}/messages  ({Token} is replaced at send time)
        public string BaseUrl { get; set; } = "";
        public string? Token { get; set; }
        public string? ApiKey { get; set; }
        // header that carries the API key (Pinnacle account setting; keep in config, not in code)
        public string ApiKeyHeaderName { get; set; } = "apikey";
        public int TimeoutSeconds { get; set; } = 30;
        public string CountryCode { get; set; } = "91";
        // language of the approved templates when the request does not give one
        public string TemplateLanguageCode { get; set; } = "en";
    }

    // appsettings.json -> "SmsSettings" (Pinnacle SMS, username / password URL API).
    // Credentials: the branch's DefaultSettings (SMSProvider / SMSUserName / SMSPassword / SMSSenderId) when all
    // of them are filled there, otherwise the values here.
    public class SmsSettings
    {
        public const string SectionName = "SmsSettings";
        public const string HttpClientName = "PinnacleSms";

        // GET url with placeholders (values are URL-encoded at send time):
        // {UserName} {Password} {SenderId} {MobileNo} {Message}
        public string Url { get; set; } = "";
        // balance url: {UserName} {Password} (credentials of this appsettings section)
        public string BalanceUrl { get; set; } = "";
        public string? Provider { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? SenderId { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
        public string CountryCode { get; set; } = "91";
        // false: 10-digit number (9129346284); true: with country code (919129346284)
        public bool MobileWithCountryCode { get; set; }
        // a plain-text response containing one of these words is a failure
        public List<string> FailureKeywords { get; set; } = new() { "error", "invalid", "fail", "insufficient", "denied", "EC1" };
    }

    // SMS balance of the Pinnacle account
    public class SmsBalanceResult
    {
        public bool Success { get; set; }
        // e.g. "1520" (as given by the provider)
        public string? Balance { get; set; }
        public string? Message { get; set; }
    }

    // SMS credentials of one group / branch (DefaultSettings)
    public class SmsProviderSettings
    {
        public string? Provider { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? SenderId { get; set; }
    }

    // credentials of one group / branch (DefaultSettings)
    public class WhatsAppProviderSettings
    {
        public string? Provider { get; set; }
        public string? Token { get; set; }
        public string? ApiKey { get; set; }
    }

    public class WhatsAppRegistrationMessageRequest
    {
        [Range(1, long.MaxValue, ErrorMessage = "RegistrationId is required.")]
        public long RegistrationId { get; set; }

        [Required(ErrorMessage = "GroupCode is required."), StringLength(10)]
        public string GroupCode { get; set; } = "";

        [Required(ErrorMessage = "BranchCode is required."), StringLength(10)]
        public string BranchCode { get; set; } = "";

        // free text (only delivered inside the 24-hour customer window); required when no template is given
        [StringLength(4096, ErrorMessage = "Message can have at most 4096 characters.")]
        public string? Message { get; set; }

        // WhatsApp: approved template (e.g. fee_reminder5). When given, the template is sent instead of free text:
        // WhatsApp shows the template's approved text, Message is kept only as the text for reference.
        // SMS: not used (only Message is sent).
        [StringLength(100)]
        public string? TemplateName { get; set; }

        // template language as approved on Pinnacle (en, en_US, hi ...); empty = WhatsAppSettings:TemplateLanguageCode
        [StringLength(10)]
        public string? LanguageCode { get; set; }

        // values for {{1}}, {{2}} ... of the template body, in order (empty when the template has no variables)
        public List<string> TemplateParameters { get; set; } = new();

        // SMS_EmailSentId of the message log (SMSSentMaster, from SendSMS/send-sms-log).
        // When given, this student's status is saved in Alert_SmsEmailToBeSend (1 sent / 0 not sent);
        // a resend with the same id updates the same row.
        [Range(0, int.MaxValue)]
        public int? SmsEmailSentId { get; set; }

        // MessageTypeId of the log (MstMessageType)
        public int MessageTypeId { get; set; }

        public long? SessionId { get; set; }

        // resend: the Alert_SmsEmailToBeSend row of the first send; that row is updated (no new row)
        public int? ResendSmsEmailId { get; set; }

        // whose id is RegistrationId: Student or Employee (saved as RecipientType, the id as RecipientId)
        [RegularExpression("^(Student|Employee)$", ErrorMessage = "RecipientType must be Student or Employee.")]
        public string RecipientType { get; set; } = WhatsAppRecipientType.Student;
    }

    public static class WhatsAppRecipientType
    {
        public const string Student = "Student";
        public const string Employee = "Employee";
    }

    // Adm_Registration + ADM_ParentDetails
    public class WhatsAppRegistrationContact
    {
        public long RegistrationId { get; set; }
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }
        public string? RegistrationNo { get; set; }
        public string? ChildFirstName { get; set; }
        public string? ChildMiddleName { get; set; }
        public string? ChildLastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ClassCode { get; set; }
        public string? FatherName { get; set; }
        public string? FatherWhatsAppNo { get; set; }
        public string? FatherContactNo { get; set; }
        // ADM_ParentDetails.SMSMobileNo: number for SMS
        public string? SMSMobileNo { get; set; }
    }

    public class WhatsAppSendResult
    {
        public bool Success { get; set; }
        public long RegistrationId { get; set; }
        public string? RegistrationNo { get; set; }
        public string? StudentName { get; set; }
        // masked (DPDP): 91******84
        public string? MobileNo { get; set; }
        public string? Provider { get; set; }
        // "template" or "text"
        public string? MessageType { get; set; }
        public string? TemplateName { get; set; }
        public int? ProviderStatusCode { get; set; }
        // message id returned by Pinnacle when it accepts the message
        public string? ProviderMessageId { get; set; }
        public string? ProviderMessageStatus { get; set; }
        // provider's response body as received (no credentials in it)
        public string? ProviderResponse { get; set; }
        public string? Message { get; set; }

        // Alert_SmsEmailToBeSend row of this student (when SmsEmailSentId was given)
        public int? SmsEmailId { get; set; }
        // 1 = sent, 0 = not sent (saved in Alert_SmsEmailToBeSend.Status)
        public int LogStatus { get; set; }
        // how many times this message to this recipient failed
        public int FailCount { get; set; }
        public bool StatusSaved { get; set; }

        // why it failed, for the HTTP status of the API
        public WhatsAppFailure Failure { get; set; } = WhatsAppFailure.None;
    }

    // one selected student passed to the Send Message popup
    public class MessageRecipient
    {
        public long RegistrationId { get; set; }
        public string? RegistrationNo { get; set; }
        public string? StudentName { get; set; }
        public string? FatherName { get; set; }
        // Student / Employee (RegistrationId is the id of this type)
        public string RecipientType { get; set; } = WhatsAppRecipientType.Student;
        // session of the student (saved as SessionId of every attempt)
        public long SessionId { get; set; }
    }

    // send history (every attempt) of the selected recipients
    public class MessageHistoryRequest
    {
        [Required(ErrorMessage = "GroupCode is required."), StringLength(10)]
        public string GroupCode { get; set; } = "";

        [Required(ErrorMessage = "BranchCode is required."), StringLength(10)]
        public string BranchCode { get; set; } = "";

        [RegularExpression("^(Student|Employee)$", ErrorMessage = "RecipientType must be Student or Employee.")]
        public string RecipientType { get; set; } = WhatsAppRecipientType.Student;

        [Required, MinLength(1, ErrorMessage = "Give at least one recipient."), MaxLength(2000, ErrorMessage = "At most 2000 recipients.")]
        public List<long> RecipientIds { get; set; } = new();
    }

    // one send attempt (Alert_SmsEmailToBeSend + SMSSentMaster.TemplateName)
    public class MessageHistoryRow
    {
        public int SmsEmailId { get; set; }
        public string? SMS_EmailSentId { get; set; }
        // W = WhatsApp, S = SMS, E = Email
        public string? MessageTextType { get; set; }
        public int? MessageTypeId { get; set; }
        public string? MessageText { get; set; }
        public string? TemplateName { get; set; }
        // masked by the API (DPDP)
        public string? MobileNo { get; set; }
        public string? RecipientType { get; set; }
        public string? RecipientId { get; set; }
        // 1 sent, 0 not sent / failed (99 = failed, older rows)
        public int? Status { get; set; }
        public string? SentBy { get; set; }
        public DateTime? SentDate { get; set; }
        public long? SessionId { get; set; }
        // how many times this message to this recipient failed
        public int FailCount { get; set; }
        // last attempt of that message to this recipient and it failed
        public bool CanResend { get; set; }
    }

    public enum WhatsAppFailure
    {
        None,
        RegistrationNotFound,
        MobileNotAvailable,
        InvalidMobile,
        SettingsMissing,
        InvalidCredentials,
        Timeout,
        ProviderRejected,
        ProviderUnreachable,
        // SMS text does not match a registered (DLT) SMS template: not sent
        TemplateMismatch
    }

    #endregion
}
