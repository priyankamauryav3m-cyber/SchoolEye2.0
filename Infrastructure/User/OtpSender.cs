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
