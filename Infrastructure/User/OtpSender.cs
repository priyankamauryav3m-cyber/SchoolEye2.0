using ApplicationInterface.User;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.User
{
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = "V3M School";
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
    }
    public class OtpSender : IOtpSenderService
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<OtpSender> _logger;

        public OtpSender(IOptions<SmtpSettings> settings, ILogger<OtpSender> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.SenderEmail))
            {
                throw new InvalidOperationException(
                    "SMTP settings missing. Check appsettings.json 'Smtp' section and Program.cs Configure<SmtpSettings>() call.");
            }
        }

        public async Task SendAsync(string toEmail, string otpCode)
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = _settings.EnableSsl
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = "Your Login Verification Code",
                Body = $"Your OTP for login is: {otpCode}\n\nThis code is valid for 5 minutes. Do not share it with anyone.",
                IsBodyHtml = false
            };
            mail.To.Add(toEmail);
            try
            {
                await client.SendMailAsync(mail);
                _logger.LogInformation("OTP email sent to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP email to {Email}", toEmail);
                throw new Exception("OTP email send failed. Please try again.");
            }
        }

        public async Task SendPasswordResetLinkAsync(string toEmail, IReadOnlyList<(string UserName, string ResetLink)> links, int validMinutes)
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = _settings.EnableSsl
            };

            var body = new StringBuilder();
            body.Append("<p>We received a request to reset the password of your SchoolEye account.</p>");
            // only the link — the username is not written in the mail
            foreach (var (_, resetLink) in links)
            {
                body.Append("<p><a href=\"").Append(WebUtility.HtmlEncode(resetLink)).Append("\">Reset your password</a></p>");
            }
            body.Append($"<p>This link is valid for {validMinutes} minutes and can be used only once.</p>")
                .Append("<p>If you did not request this, please ignore this email. Your password will not change.</p>");

            var mail = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = "Reset your SchoolEye password",
                Body = body.ToString(),
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);
            try
            {
                await client.SendMailAsync(mail);
                // the link is a credential — never logged
                _logger.LogInformation("Password reset email sent to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
                throw new Exception("Password reset email send failed. Please try again.");
            }
        }

        public async Task SendReceiptRefundOtpAsync(string toEmail, string otpCode, string receiptNo, decimal amount, int validMinutes)
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = _settings.EnableSsl
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = $"Fee refund verification code - Receipt {receiptNo}",
                Body = $"Your OTP to confirm the refund of Rs. {amount:N2} against fee receipt {receiptNo} is: {otpCode}\n\n" +
                       $"This code is valid for {validMinutes} minutes. Share it only with the school fee counter while taking the refund.\n" +
                       "If you did not ask for a refund, please contact the school.",
                IsBodyHtml = false
            };
            mail.To.Add(toEmail);
            try
            {
                await client.SendMailAsync(mail);
                // the OTP is a credential — never logged
                _logger.LogInformation("Receipt refund OTP email sent for receipt {ReceiptNo}", receiptNo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send receipt refund OTP email for receipt {ReceiptNo}", receiptNo);
                throw new Exception("OTP email send failed. Please try again.");
            }
        }

        public async Task SendReceiptRefundConfirmationAsync(string toEmail, string studentName, string receiptNo, decimal amount, DateTime refundDate, byte[] refundReceiptPdf)
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = _settings.EnableSsl
            };

            var name = WebUtility.HtmlEncode(studentName);
            var receipt = WebUtility.HtmlEncode(receiptNo);
            var body = new StringBuilder();
            body.Append("<div style=\"font-family:Segoe UI,Arial,sans-serif;max-width:560px;border:1px solid #99f6e4;border-radius:10px;overflow:hidden\">")
                .Append("<div style=\"background:#0f766e;color:#fff;padding:14px 18px;font-size:18px;font-weight:700\">&#10227; Fee Refunded</div>")
                .Append("<div style=\"padding:16px 18px;color:#1f2937;font-size:14px\">")
                .Append("<p>Dear Parent,</p>")
                .Append($"<p>The fee of <b>{name}</b> paid against receipt <b>{receipt}</b> has been refunded.</p>")
                .Append("<table style=\"border-collapse:collapse;width:100%;margin:10px 0\">")
                .Append($"<tr><td style=\"padding:6px 0;color:#64748b\">Amount refunded</td><td style=\"padding:6px 0;text-align:right;font-size:18px;font-weight:700;color:#0f766e\">Rs. {amount:N2}</td></tr>")
                .Append($"<tr><td style=\"padding:6px 0;color:#64748b\">Refund date</td><td style=\"padding:6px 0;text-align:right\">{refundDate:dd/MM/yyyy}</td></tr>")
                .Append($"<tr><td style=\"padding:6px 0;color:#64748b\">Receipt no.</td><td style=\"padding:6px 0;text-align:right\">{receipt}</td></tr>")
                .Append("</table>")
                .Append("<p>The refund receipt is attached. The fee of this receipt is due again and will be collected with the next fee.</p>")
                .Append("<p style=\"color:#64748b;font-size:12px\">If you did not take this refund, please contact the school immediately.</p>")
                .Append("</div></div>");

            using var mail = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = $"Fee refund confirmation - Receipt {receiptNo}",
                Body = body.ToString(),
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);
            using var pdf = new System.IO.MemoryStream(refundReceiptPdf);
            mail.Attachments.Add(new Attachment(pdf, $"RefundReceipt_{receiptNo}.pdf", "application/pdf"));
            try
            {
                await client.SendMailAsync(mail);
                _logger.LogInformation("Receipt refund confirmation email sent for receipt {ReceiptNo}", receiptNo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send receipt refund confirmation email for receipt {ReceiptNo}", receiptNo);
                throw new Exception("Refund confirmation email send failed.");
            }
        }
    }


    //public class OtpSender : IOtpSender
    //{
    //    private readonly ILogger<OtpSender> _logger;
    //    private readonly HttpClient _httpClient;
    //    private readonly Fast2SmsSettings _settings;
    //    private readonly IConfiguration _config;
    //    public OtpSender(HttpClient httpClient, IConfiguration config, ILogger<OtpSender> logger, IOptions<Fast2SmsSettings> settings)
    //    {
    //        _httpClient = httpClient;
    //        _settings = settings.Value;
    //        _logger = logger;

    //        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
    //        {
    //            // fail fast, DI setup galat hai to yahin pata chal jayega
    //            throw new InvalidOperationException(
    //                "Fast2Sms:ApiKey missing. Check appsettings.json and Program.cs Configure<Fast2SmsSettings>() call.");
    //        }
    //    }


    //        public async Task SendAsync(string mobileNo, string otpCode)
    //        {
    //            var url = "https://www.fast2sms.com/dev/bulkV2" +
    //                      $"?authorization={_settings.ApiKey}" +
    //                      "&route=otp" +
    //                      $"&variables_values={otpCode}" +
    //                      $"&numbers={mobileNo}";

    //            var response = await _httpClient.GetAsync(url);
    //            var body = await response.Content.ReadAsStringAsync();

    //            if (!response.IsSuccessStatusCode)
    //            {
    //                _logger.LogError("Fast2SMS failed: {Body}", body);
    //                throw new Exception("OTP SMS send failed. Please try again.");
    //            }

    //            _logger.LogInformation("Fast2SMS response: {Body}", body);
    //        }


    //    //public async Task SendAsync(string mobileNo, string otpCode)
    //    //{
    //    //    var apiKey = _config["Fast2Sms:ApiKey"];   // appsettings.json me daalo

    //    //    var url = "https://www.fast2sms.com/dev/bulkV2" +  $"?authorization={apiKey}" +
    //    //              "&route=otp" +
    //    //              $"&variables_values={otpCode}" +
    //    //              $"&numbers={mobileNo}";

    //    //    var response = await _httpClient.GetAsync(url);
    //    //    var body = await response.Content.ReadAsStringAsync();

    //    //    if (!response.IsSuccessStatusCode)
    //    //    {
    //    //        _logger.LogError("Fast2SMS failed: {Body}", body);
    //    //        throw new Exception("OTP SMS send failed. Please try again.");
    //    //    }

    //    //    _logger.LogInformation("OTP sent to {MobileNo}", mobileNo);
    //    //}
    //}
}
