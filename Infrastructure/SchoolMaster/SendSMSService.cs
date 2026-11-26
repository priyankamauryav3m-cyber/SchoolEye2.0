using ApplicationInterface.SuperAdmin;
using Dapper;
using DomainModel.Admin;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainModel.Common;
using DomainModel.SchoolMaster;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Net.WebRequestMethods;

namespace Infrastructure.SchoolMaster
{
    public class SendSMSService : ISendSMS
    {

        private readonly string _connectionString;
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly WhatsAppSettings _whatsApp;
        private readonly SmsSettings _sms;
        public SendSMSService(IConfiguration configuration, IHttpClientFactory httpClientFactory, HttpClient http = null, IConfiguration config = null)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
            ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
            _http = http;
            _config = config;
            _httpClientFactory = httpClientFactory;
            _whatsApp = configuration.GetSection(WhatsAppSettings.SectionName).Get<WhatsAppSettings>() ?? new WhatsAppSettings();
            _sms = configuration.GetSection(SmsSettings.SectionName).Get<SmsSettings>() ?? new SmsSettings();
        }
        public async Task<string> SaveSMSSentDetails(SMSSentModel model)
        {
            try
            {
                using var db = new SqlConnection(_connectionString);
                await db.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", model.GroupCode);
                parameters.Add("@BranchCode", model.BranchCode);
                parameters.Add("@SMS_Or_Mail", model.SMS_Or_Mail);
                parameters.Add("@MessageType", model.MessageType);
                parameters.Add("@MessageText", model.MessageText);
                parameters.Add("@SentDate", model.SentDate);
                parameters.Add("@SentBy", model.SentBy);
                parameters.Add("@TotalMsg", model.TotalMsg);
                parameters.Add("@TotalDelivered", model.TotalDelivered);
                parameters.Add("@SMSVendor", model.SMSVendor);
                parameters.Add("@IsValid", model.IsValid);
                parameters.Add("@isAttachment", model.isAttachment);
                // OUTPUT parameter
                parameters.Add("@SMS_EmailSentId",
                    dbType: DbType.String,
                    direction: ParameterDirection.Output,
                    size: 50
                );
                await db.ExecuteAsync(
                    "Msg_UspSMSSentMaster",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
                return parameters.Get<string>("@SMS_EmailSentId");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<bool> SendSmsAsync(string mobileNo, string message)
        {
            if (string.IsNullOrWhiteSpace(mobileNo) || string.IsNullOrWhiteSpace(message))
                return false;
            try
            {
                var apiUrl = _config["SmsGateway:Url"];
                var apiKey = _config["SmsGateway:ApiKey"];
                var senderId = _config["SmsGateway:SenderId"];
                var payload = new Dictionary<string, string>
                {
                    ["apikey"] = apiKey,
                    ["numbers"] = mobileNo,
                    ["sender"] = senderId,
                    ["message"] = message
                };
                var content = new FormUrlEncodedContent(payload);
                var response = await _http.PostAsync(apiUrl, content);
                if (!response.IsSuccessStatusCode)
                    return false;
                var responseText = await response.Content.ReadAsStringAsync();
                return responseText.Contains("success", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        // ======================================================================
        // WhatsApp (Pinnacle / Pinbot)
        // ======================================================================

        public async Task<WhatsAppRegistrationContact?> GetRegistrationWhatsAppContactAsync(long registrationId, string groupCode, string branchCode)
        {
            using var db = new SqlConnection(_connectionString);
            const string sql = @"SELECT TOP (1)
                                        R.RegistrationId, R.GroupCode, R.BranchCode, R.RegistrationNo,
                                        R.ChildFirstName, R.ChildMiddleName, R.ChildLastName, R.DateOfBirth, R.ClassCode,
                                        P.FatherName, P.FatherWhatsAppNo, P.FatherContactNo, P.SMSMobileNo
                                 FROM Adm_Registration R WITH (NOLOCK)
                                 INNER JOIN ADM_ParentDetails P WITH (NOLOCK) ON P.RegistrationId = R.RegistrationId
                                 WHERE R.RegistrationId = @RegistrationId
                                   AND R.GroupCode = @GroupCode
                                   AND R.BranchCode = @BranchCode";
            return await db.QueryFirstOrDefaultAsync<WhatsAppRegistrationContact>(sql,
                new { RegistrationId = registrationId, GroupCode = groupCode, BranchCode = branchCode });
        }

        public async Task<WhatsAppProviderSettings> GetWhatsAppSettingsAsync(string groupCode, string branchCode)
        {
            using var db = new SqlConnection(_connectionString);
            // a key can be saved more than once -> the latest row (highest DID) wins
            const string sql = @"SELECT KeyName, KeyValue
                                 FROM (SELECT KeyName, KeyValue,
                                              ROW_NUMBER() OVER (PARTITION BY KeyName ORDER BY DID DESC) AS Rn
                                       FROM DefaultSettings WITH (NOLOCK)
                                       WHERE GroupCode = @GroupCode
                                         AND BranchCode = @BranchCode
                                         AND KeyName IN ('WHATSAPPPROVIDER', 'WHATSAPPTOKEN', 'WHATSAPPAPIKEY')) S
                                 WHERE S.Rn = 1";
            var rows = (await db.QueryAsync<(string KeyName, string? KeyValue)>(sql,
                new { GroupCode = groupCode, BranchCode = branchCode })).ToList();

            string? Value(string key) => rows.FirstOrDefault(r => string.Equals(r.KeyName, key, StringComparison.OrdinalIgnoreCase)).KeyValue?.Trim();

            return new WhatsAppProviderSettings
            {
                Provider = Value("WHATSAPPPROVIDER"),
                Token = Value("WHATSAPPTOKEN"),
                ApiKey = Value("WHATSAPPAPIKEY")
            };
        }

        public async Task<WhatsAppSendResult> SendRegistrationWhatsAppAsync(WhatsAppRegistrationMessageRequest request, string? sentBy, CancellationToken cancellationToken = default)
        {
            // resend of a row that is already sent: nothing is sent again
            var alreadySent = await GetAlreadySentAsync(request, "W");
            if (alreadySent != null) return alreadySent;

            return await SendAndLogAsync(request, sentBy, "W", cancellationToken);
        }

        // 1. row in Alert_SmsEmailToBeSend with Status 0 (a resend uses its existing row)
        // 2. send (WhatsApp / SMS)
        // 3. same row: sent -> Status 1, failed -> Status stays 0 and FailCount + 1
        private async Task<WhatsAppSendResult> SendAndLogAsync(WhatsAppRegistrationMessageRequest request, string? sentBy, string messageTextType, CancellationToken cancellationToken)
        {
            var log = request.SmsEmailSentId > 0;
            int? rowId = request.ResendSmsEmailId > 0 ? request.ResendSmsEmailId : null;
            var rowSaved = rowId != null;

            if (log && rowId == null)
            {
                try
                {
                    (rowId, _) = await SaveRecipientStatusAsync(request, null, null, request.Message, 0, false, null, sentBy, messageTextType);
                    rowSaved = rowId != null;
                }
                catch (Exception ex)
                {
                    // the message is still sent; step 3 then inserts the row
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }

            var context = new WhatsAppSendContext();
            var result = messageTextType == "S"
                ? await SendSmsToProviderAsync(request, context, cancellationToken)
                : await SendToProviderAsync(request, context, cancellationToken);

            if (log)
            {
                result.LogStatus = result.Success ? 1 : 0;
                try
                {
                    var mobile = messageTextType == "S" ? context.To : context.To ?? context.Contact?.FatherWhatsAppNo;
                    var (id, failCount) = await SaveRecipientStatusAsync(request, context.Contact, mobile, request.Message,
                        (byte)result.LogStatus, !result.Success, rowSaved ? rowId : null, sentBy, messageTextType);
                    result.SmsEmailId = id;
                    result.FailCount = failCount;
                    result.StatusSaved = true;
                }
                catch (Exception ex)
                {
                    // the message itself is already sent / failed: report it, only the log could not be saved
                    Console.WriteLine($"Exception: {ex.Message}");
                    result.SmsEmailId = rowId;
                    result.StatusSaved = false;
                }
            }
            return result;
        }

        // row of a message to one student in Alert_SmsEmailToBeSend (W = WhatsApp, S = SMS):
        // rowId NULL -> new row, otherwise that row is updated. Status 1 sent / 0 not sent.
        private async Task<(int? SmsEmailId, int FailCount)> SaveRecipientStatusAsync(WhatsAppRegistrationMessageRequest request, WhatsAppRegistrationContact? contact,
            string? mobileNo, string? messageText, byte status, bool addFail, int? rowId, string? sentBy, string messageTextType)
        {
            var message = string.IsNullOrWhiteSpace(messageText)
                ? (string.IsNullOrWhiteSpace(request.TemplateName) ? null : $"Template: {request.TemplateName.Trim()}")
                : messageText.Trim();

            using var db = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@GroupCode", request.GroupCode.Trim());
            parameters.Add("@BranchCode", request.BranchCode.Trim());
            parameters.Add("@SMS_EmailSentId", request.SmsEmailSentId!.Value.ToString());
            parameters.Add("@MessageTypeId", request.MessageTypeId);
            parameters.Add("@MessageText", Cut(message, 1100));
            // saved as the 10-digit number (918080869713 -> 8080869713), for WhatsApp and SMS alike
            parameters.Add("@MobileNo", Cut(ToLocalMobile(mobileNo), 50));
            parameters.Add("@SmsEmailRecipient", Cut(contact?.FatherName, 50));
            parameters.Add("@RecipientType", string.Equals(request.RecipientType, WhatsAppRecipientType.Employee, StringComparison.OrdinalIgnoreCase)
                ? WhatsAppRecipientType.Employee : WhatsAppRecipientType.Student);
            parameters.Add("@RecipientId", request.RegistrationId.ToString());
            parameters.Add("@Status", status);
            parameters.Add("@SentBy", Cut(string.IsNullOrWhiteSpace(sentBy) ? "System" : sentBy, 50));
            parameters.Add("@SessionId", request.SessionId);
            // in: row to update (NULL = new row), out: the row
            parameters.Add("@SmsEmailId", rowId, DbType.Int32, ParameterDirection.InputOutput);
            parameters.Add("@MessageTextType", messageTextType);
            parameters.Add("@AddFail", addFail);
            parameters.Add("@FailCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await db.ExecuteAsync("Alert_Usp_SaveWhatsAppStatus", parameters, commandType: CommandType.StoredProcedure);
            return (parameters.Get<int?>("@SmsEmailId"), parameters.Get<int?>("@FailCount") ?? 0);
        }

        // resend of a row that is already sent (Status 1) -> that result, without sending again
        private async Task<WhatsAppSendResult?> GetAlreadySentAsync(WhatsAppRegistrationMessageRequest request, string messageTextType)
        {
            if (!(request.ResendSmsEmailId > 0)) return null;

            using var db = new SqlConnection(_connectionString);
            const string sql = @"SELECT TOP (1) SmsEmailId, [Status], ISNULL(FailCount, 0) AS FailCount, MobileNo
                                 FROM Alert_SmsEmailToBeSend WITH (NOLOCK)
                                 WHERE SmsEmailId = @SmsEmailId
                                   AND GroupCode = @GroupCode
                                   AND BranchCode = @BranchCode
                                   AND MessageTextType = @MessageTextType";
            var row = await db.QueryFirstOrDefaultAsync<SentRow>(sql, new
            {
                SmsEmailId = request.ResendSmsEmailId,
                GroupCode = request.GroupCode.Trim(),
                BranchCode = request.BranchCode.Trim(),
                MessageTextType = messageTextType
            });
            if (row == null || row.Status != 1) return null;

            return new WhatsAppSendResult
            {
                Success = true,
                RegistrationId = request.RegistrationId,
                SmsEmailId = row.SmsEmailId,
                LogStatus = 1,
                StatusSaved = true,
                FailCount = row.FailCount,
                MobileNo = PiiMask.Mobile(row.MobileNo),
                Message = "Message was already sent."
            };
        }

        // ======================================================================
        // SMS (Pinnacle, username / password URL API)
        // ======================================================================

        public async Task<SmsProviderSettings> GetSmsSettingsAsync(string groupCode, string branchCode)
        {
            using var db = new SqlConnection(_connectionString);
            // a key can be saved more than once -> the latest row (highest DID) wins
            const string sql = @"SELECT KeyName, KeyValue
                                 FROM (SELECT KeyName, KeyValue,
                                              ROW_NUMBER() OVER (PARTITION BY KeyName ORDER BY DID DESC) AS Rn
                                       FROM DefaultSettings WITH (NOLOCK)
                                       WHERE GroupCode = @GroupCode
                                         AND BranchCode = @BranchCode
                                         AND KeyName IN ('SMSPROVIDER', 'SMSUSERNAME', 'SMSPASSWORD', 'SMSSENDERID')) S
                                 WHERE S.Rn = 1";
            var rows = (await db.QueryAsync<(string KeyName, string? KeyValue)>(sql,
                new { GroupCode = groupCode, BranchCode = branchCode })).ToList();

            string? Value(string key) => rows.FirstOrDefault(r => string.Equals(r.KeyName, key, StringComparison.OrdinalIgnoreCase)).KeyValue?.Trim();

            return new SmsProviderSettings
            {
                Provider = Value("SMSPROVIDER"),
                UserName = Value("SMSUSERNAME"),
                Password = Value("SMSPASSWORD"),
                SenderId = Value("SMSSENDERID")
            };
        }

        public async Task<WhatsAppSendResult> SendRegistrationSmsAsync(WhatsAppRegistrationMessageRequest request, string? sentBy, CancellationToken cancellationToken = default)
        {
            // resend of a row that is already sent: nothing is sent again
            var alreadySent = await GetAlreadySentAsync(request, "S");
            if (alreadySent != null) return alreadySent;

            return await SendAndLogAsync(request, sentBy, "S", cancellationToken);
        }

        private async Task<WhatsAppSendResult> SendSmsToProviderAsync(WhatsAppRegistrationMessageRequest request, WhatsAppSendContext context, CancellationToken cancellationToken)
        {
            var groupCode = request.GroupCode.Trim();
            var branchCode = request.BranchCode.Trim();
            var result = new WhatsAppSendResult
            {
                RegistrationId = request.RegistrationId,
                Provider = "Pinnacle SMS",
                MessageType = "sms",
                // SMS has no template: only the message text is sent
                TemplateName = null
            };

            if (string.Equals(request.RecipientType, WhatsAppRecipientType.Employee, StringComparison.OrdinalIgnoreCase))
                return Fail(result, WhatsAppFailure.MobileNotAvailable, "SMS to employees is not available yet.");

            // 1. student + SMS mobile number (parent details)
            var contact = await GetRegistrationWhatsAppContactAsync(request.RegistrationId, groupCode, branchCode);
            context.Contact = contact;
            if (contact == null)
                return Fail(result, WhatsAppFailure.RegistrationNotFound, "Registration record was not found.");

            result.RegistrationNo = contact.RegistrationNo;
            result.StudentName = string.Join(" ", new[] { contact.ChildFirstName, contact.ChildMiddleName, contact.ChildLastName }
                .Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!.Trim()));

            // SMS goes to the SMS mobile no. of the parent details (ADM_ParentDetails.SMSMobileNo)
            var rawMobile = contact.SMSMobileNo?.Trim();
            if (string.IsNullOrWhiteSpace(rawMobile))
                return Fail(result, WhatsAppFailure.MobileNotAvailable, "SMS mobile number is not available for this registration.");

            var withCountryCode = ToWhatsAppNumber(rawMobile, _sms.CountryCode);
            result.MobileNo = PiiMask.Mobile(withCountryCode ?? rawMobile);
            if (withCountryCode == null)
                return Fail(result, WhatsAppFailure.InvalidMobile, "SMS mobile number is not a valid mobile number.");
            var cc = string.IsNullOrWhiteSpace(_sms.CountryCode) ? "91" : _sms.CountryCode.Trim();
            var to = _sms.MobileWithCountryCode ? withCountryCode : withCountryCode[cc.Length..];
            context.To = to;

            // 2. text: %SN% / %FN% / %ADMNO% / %DT% filled for this student
            var text = (request.Message ?? "").Trim()
                .Replace("%SN%", result.StudentName ?? "", StringComparison.OrdinalIgnoreCase)
                .Replace("%FN%", contact.FatherName?.Trim() ?? "", StringComparison.OrdinalIgnoreCase)
                .Replace("%ADMNO%", contact.RegistrationNo?.Trim() ?? "", StringComparison.OrdinalIgnoreCase)
                .Replace("%DT%", DateTime.Today.ToString("dd-MM-yyyy"), StringComparison.OrdinalIgnoreCase);
            context.Text = text;
            if (string.IsNullOrWhiteSpace(text))
                return Fail(result, WhatsAppFailure.ProviderRejected, "SMS text is empty.");

            // 3. credentials: the branch's DefaultSettings when complete there, otherwise appsettings SmsSettings
            var branch = await GetSmsSettingsAsync(groupCode, branchCode);
            var useBranch = !string.IsNullOrWhiteSpace(branch.UserName) && !string.IsNullOrWhiteSpace(branch.Password) && !string.IsNullOrWhiteSpace(branch.SenderId);
            var provider = useBranch ? FirstValue(branch.Provider, _sms.Provider) : _sms.Provider;
            var userName = useBranch ? branch.UserName : _sms.UserName;
            var password = useBranch ? branch.Password : _sms.Password;
            var senderId = useBranch ? branch.SenderId : _sms.SenderId;

            if (string.IsNullOrWhiteSpace(_sms.Url) || string.IsNullOrWhiteSpace(userName)
                || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(senderId))
                return Fail(result, WhatsAppFailure.SettingsMissing, "SMS settings are not configured for this branch.");

            if (!string.IsNullOrWhiteSpace(provider) && !provider.Contains("pinnacle", StringComparison.OrdinalIgnoreCase))
                return Fail(result, WhatsAppFailure.SettingsMissing, $"The SMS provider of this branch is '{provider}'. Only Pinnacle is supported.");

            // 4. send (GET url from SmsSettings:Url, every value URL-encoded)
            var url = _sms.Url
                .Replace("{UserName}", Uri.EscapeDataString(userName!.Trim()), StringComparison.OrdinalIgnoreCase)
                .Replace("{Password}", Uri.EscapeDataString(password!.Trim()), StringComparison.OrdinalIgnoreCase)
                .Replace("{SenderId}", Uri.EscapeDataString(senderId!.Trim()), StringComparison.OrdinalIgnoreCase)
                .Replace("{MobileNo}", Uri.EscapeDataString(to), StringComparison.OrdinalIgnoreCase)
                .Replace("{Message}", Uri.EscapeDataString(text), StringComparison.OrdinalIgnoreCase);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            var client = _httpClientFactory.CreateClient(SmsSettings.HttpClientName);
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(_sms.TimeoutSeconds > 0 ? _sms.TimeoutSeconds : 30));

            HttpResponseMessage response;
            try
            {
                response = await client.SendAsync(httpRequest, timeout.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return Fail(result, WhatsAppFailure.Timeout, "SMS provider did not respond in time. Please try again.");
            }
            catch (HttpRequestException)
            {
                return Fail(result, WhatsAppFailure.ProviderUnreachable, "Unable to connect to the SMS provider.");
            }

            using (response)
            {
                var body = (await response.Content.ReadAsStringAsync(cancellationToken)).Trim();
                result.ProviderStatusCode = (int)response.StatusCode;
                // never echo the credentials back, even if the provider repeats them
                body = body.Replace(password!.Trim(), "***", StringComparison.Ordinal);
                result.ProviderResponse = body.Length > 4000 ? body[..4000] : body;

                var (ok, messageId, error) = ReadSmsResponse(body);
                result.ProviderMessageId = messageId;

                if (response.IsSuccessStatusCode && ok)
                {
                    result.Success = true;
                    result.Message = "SMS accepted by the provider.";
                    return result;
                }

                if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                    return Fail(result, WhatsAppFailure.InvalidCredentials, "SMS provider rejected the credentials.");

                return Fail(result, WhatsAppFailure.ProviderRejected,
                    string.IsNullOrWhiteSpace(error) ? "SMS provider did not accept the message." : $"SMS provider did not accept the message: {error}");
            }
        }

        // JSON { "status":"success", "data":[{"uniqueid":"..."}] } / { "code":"EC1xxx", "msg":"..." }
        // or plain text (message id on success, an error text otherwise)
        private (bool Ok, string? MessageId, string? Error) ReadSmsResponse(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return (false, null, "Empty response.");

            if (body.StartsWith("{") || body.StartsWith("["))
            {
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0
                        ? doc.RootElement[0] : doc.RootElement;
                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        static string? Prop(JsonElement e, string name)
                        {
                            foreach (var p in e.EnumerateObject())
                            {
                                if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
                                    return p.Value.ValueKind is JsonValueKind.String or JsonValueKind.Number ? p.Value.ToString() : null;
                            }
                            return null;
                        }

                        var status = Prop(root, "status");
                        var code = Prop(root, "code");
                        string? id = Prop(root, "uniqueid") ?? Prop(root, "msgid") ?? Prop(root, "messageid");
                        if (id == null && root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array && data.GetArrayLength() > 0
                            && data[0].ValueKind == JsonValueKind.Object)
                            id = Prop(data[0], "uniqueid") ?? Prop(data[0], "msgid");
                        var error = Prop(root, "msg") ?? Prop(root, "message") ?? Prop(root, "description");

                        var ok = string.Equals(status, "success", StringComparison.OrdinalIgnoreCase)
                                 && (string.IsNullOrWhiteSpace(code) || code == "200");
                        return (ok, id, ok ? null : (error ?? code ?? status));
                    }
                }
                catch (JsonException)
                {
                    // not JSON after all: read as text
                }
            }

            var failed = (_sms.FailureKeywords ?? new List<string>())
                .Any(k => !string.IsNullOrWhiteSpace(k) && body.Contains(k, StringComparison.OrdinalIgnoreCase));
            return failed ? (false, null, body.Length > 200 ? body[..200] : body) : (true, body.Length > 100 ? body[..100] : body, null);
        }

        // SMS balance: GET SmsSettings:BalanceUrl ({UserName} / {Password} from appsettings SmsSettings)
        public async Task<SmsBalanceResult> GetSmsBalanceAsync(CancellationToken cancellationToken = default)
        {
            var userName = _sms.UserName?.Trim();
            var password = _sms.Password?.Trim();
            if (string.IsNullOrWhiteSpace(_sms.BalanceUrl) || string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                return new SmsBalanceResult { Success = false, Message = "SMS balance settings are not configured." };

            var url = _sms.BalanceUrl
                .Replace("{UserName}", Uri.EscapeDataString(userName), StringComparison.OrdinalIgnoreCase)
                .Replace("{Password}", Uri.EscapeDataString(password), StringComparison.OrdinalIgnoreCase);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            var client = _httpClientFactory.CreateClient(SmsSettings.HttpClientName);
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(_sms.TimeoutSeconds > 0 ? _sms.TimeoutSeconds : 30));

            try
            {
                using var response = await client.SendAsync(httpRequest, timeout.Token);
                var body = (await response.Content.ReadAsStringAsync(timeout.Token)).Trim();
                // never echo the credentials back
                body = body.Replace(password, "***", StringComparison.Ordinal);

                if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(body))
                    return new SmsBalanceResult { Success = false, Message = "SMS provider did not return the balance." };

                // plain text like "1520" / "Balance : 1520", or an error text
                var number = Regex.Match(body, @"-?\d+(\.\d+)?");
                var failed = (_sms.FailureKeywords ?? new List<string>())
                    .Any(k => !string.IsNullOrWhiteSpace(k) && body.Contains(k, StringComparison.OrdinalIgnoreCase));
                if (failed || !number.Success)
                    return new SmsBalanceResult { Success = false, Message = body.Length > 200 ? body[..200] : body };

                return new SmsBalanceResult { Success = true, Balance = number.Value, Message = "SMS balance loaded." };
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return new SmsBalanceResult { Success = false, Message = "SMS provider did not respond in time." };
            }
            catch (HttpRequestException)
            {
                return new SmsBalanceResult { Success = false, Message = "Unable to connect to the SMS provider." };
            }
        }

        public async Task<List<MessageHistoryRow>> GetMessageHistoryAsync(MessageHistoryRequest request)
        {
            using var db = new SqlConnection(_connectionString);
            var rows = await db.QueryAsync<MessageHistoryRow>("Alert_Usp_GetRecipientMessageHistory", new
            {
                GroupCode = request.GroupCode.Trim(),
                BranchCode = request.BranchCode.Trim(),
                RecipientType = string.Equals(request.RecipientType, WhatsAppRecipientType.Employee, StringComparison.OrdinalIgnoreCase)
                    ? WhatsAppRecipientType.Employee : WhatsAppRecipientType.Student,
                RecipientIds = string.Join(",", request.RecipientIds.Where(id => id > 0).Distinct())
            }, commandType: CommandType.StoredProcedure);

            var list = rows.ToList();
            // DPDP: the number leaves the API masked
            foreach (var row in list)
                row.MobileNo = PiiMask.Mobile(row.MobileNo);
            return list;
        }

        // 918080869713 / +91 80808 69713 / 08080869713 -> 8080869713 ; anything else is kept as given
        private string? ToLocalMobile(string? number)
        {
            if (string.IsNullOrWhiteSpace(number)) return null;
            var cc = string.IsNullOrWhiteSpace(_whatsApp.CountryCode) ? "91" : _whatsApp.CountryCode.Trim();
            var full = ToWhatsAppNumber(number, cc);
            return full != null && full.StartsWith(cc) ? full[cc.Length..] : number.Trim();
        }

        private static string? Cut(string? value, int length)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            value = value.Trim();
            return value.Length > length ? value[..length] : value;
        }

        private sealed class SentRow
        {
            public int SmsEmailId { get; set; }
            public byte? Status { get; set; }
            public int FailCount { get; set; }
            public string? MobileNo { get; set; }
        }

        // filled while sending, used to save the status row
        private sealed class WhatsAppSendContext
        {
            public WhatsAppRegistrationContact? Contact { get; set; }
            public string? To { get; set; }
            // SMS text after %SN% / %FN% ... were filled
            public string? Text { get; set; }
        }

        private async Task<WhatsAppSendResult> SendToProviderAsync(WhatsAppRegistrationMessageRequest request, WhatsAppSendContext context, CancellationToken cancellationToken)
        {
            var groupCode = request.GroupCode.Trim();
            var branchCode = request.BranchCode.Trim();
            var result = new WhatsAppSendResult { RegistrationId = request.RegistrationId, Provider = "Pinnacle" };

            // employee numbers are not read yet (only the registration / father's number is)
            if (string.Equals(request.RecipientType, WhatsAppRecipientType.Employee, StringComparison.OrdinalIgnoreCase))
                return Fail(result, WhatsAppFailure.MobileNotAvailable, "WhatsApp to employees is not available yet.");

            // 1. student + father's WhatsApp number
            var contact = await GetRegistrationWhatsAppContactAsync(request.RegistrationId, groupCode, branchCode);
            context.Contact = contact;
            if (contact == null)
                return Fail(result, WhatsAppFailure.RegistrationNotFound, "Registration record was not found.");

            result.RegistrationNo = contact.RegistrationNo;
            result.StudentName = string.Join(" ", new[] { contact.ChildFirstName, contact.ChildMiddleName, contact.ChildLastName }
                .Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!.Trim()));

            // 2. mobile number
            if (string.IsNullOrWhiteSpace(contact.FatherWhatsAppNo))
                return Fail(result, WhatsAppFailure.MobileNotAvailable, "Father's WhatsApp number is not available for this registration.");

            var to = ToWhatsAppNumber(contact.FatherWhatsAppNo, _whatsApp.CountryCode);
            context.To = to;
            result.MobileNo = PiiMask.Mobile(to ?? contact.FatherWhatsAppNo);
            if (to == null)
                return Fail(result, WhatsAppFailure.InvalidMobile, "Father's WhatsApp number is not a valid mobile number.");

            // 3. credentials: branch settings (DefaultSettings) first, appsettings WhatsAppSettings as fallback
            var branchSettings = await GetWhatsAppSettingsAsync(groupCode, branchCode);
            if (!string.IsNullOrWhiteSpace(branchSettings.Provider))
                result.Provider = branchSettings.Provider;
            var token = FirstValue(branchSettings.Token, _whatsApp.Token);
            var apiKey = FirstValue(branchSettings.ApiKey, _whatsApp.ApiKey);

            if (string.IsNullOrWhiteSpace(_whatsApp.BaseUrl) || !_whatsApp.BaseUrl.Contains("{Token}", StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(_whatsApp.ApiKeyHeaderName))
                return Fail(result, WhatsAppFailure.SettingsMissing, "WhatsApp settings are not configured for this branch.");

            if (!string.IsNullOrWhiteSpace(branchSettings.Provider)
                && !branchSettings.Provider.Contains("pinnacle", StringComparison.OrdinalIgnoreCase)
                && !branchSettings.Provider.Contains("pinbot", StringComparison.OrdinalIgnoreCase))
                return Fail(result, WhatsAppFailure.SettingsMissing, "The WhatsApp provider of this branch is not Pinnacle.");

            // 4. send (Pinnacle v3 messages endpoint, WhatsApp Cloud API message format)
            var url = _whatsApp.BaseUrl.Replace("{Token}", Uri.EscapeDataString(token!), StringComparison.Ordinal);
            object payload;
            if (!string.IsNullOrWhiteSpace(request.TemplateName))
            {
                // approved template: WhatsApp shows the template's own text, {{1}}, {{2}} ... are filled from TemplateParameters
                var templateName = request.TemplateName.Trim();
                var language = string.IsNullOrWhiteSpace(request.LanguageCode)
                    ? (string.IsNullOrWhiteSpace(_whatsApp.TemplateLanguageCode) ? "en" : _whatsApp.TemplateLanguageCode.Trim())
                    : request.LanguageCode.Trim();
                var parameters = (request.TemplateParameters ?? new List<string>())
                    .Select(p => new { type = "text", text = p?.Trim() ?? "" })
                    .ToList();

                result.MessageType = "template";
                result.TemplateName = templateName;
                payload = new
                {
                    messaging_product = "whatsapp",
                    recipient_type = "individual",
                    to,
                    type = "template",
                    template = new
                    {
                        name = templateName,
                        language = new { code = language },
                        components = parameters.Count == 0
                            ? Array.Empty<object>()
                            : new object[] { new { type = "body", parameters } }
                    }
                };
            }
            else
            {
                // free text: delivered only inside the 24-hour customer service window
                result.MessageType = "text";
                payload = new
                {
                    messaging_product = "whatsapp",
                    recipient_type = "individual",
                    to,
                    type = "text",
                    text = new { preview_url = false, body = request.Message!.Trim() }
                };
            }

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            httpRequest.Headers.TryAddWithoutValidation(_whatsApp.ApiKeyHeaderName, apiKey);
            httpRequest.Headers.Accept.ParseAdd("application/json");

            var client = _httpClientFactory.CreateClient(WhatsAppSettings.HttpClientName);
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(_whatsApp.TimeoutSeconds > 0 ? _whatsApp.TimeoutSeconds : 30));

            HttpResponseMessage response;
            try
            {
                response = await client.SendAsync(httpRequest, timeout.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return Fail(result, WhatsAppFailure.Timeout, "WhatsApp provider did not respond in time. Please try again.");
            }
            catch (HttpRequestException)
            {
                return Fail(result, WhatsAppFailure.ProviderUnreachable, "Unable to connect to the WhatsApp provider.");
            }

            using (response)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                result.ProviderStatusCode = (int)response.StatusCode;
                result.ProviderResponse = body.Length > 4000 ? body[..4000] : body;

                // 5. success only when the provider accepted the message (2xx + message id)
                var (messageId, messageStatus, providerError) = ReadProviderResponse(body);
                result.ProviderMessageId = messageId;
                result.ProviderMessageStatus = messageStatus;

                if (response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(messageId))
                {
                    result.Success = true;
                    result.Message = "WhatsApp message accepted by the provider.";
                    return result;
                }

                if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                    return Fail(result, WhatsAppFailure.InvalidCredentials, "WhatsApp provider rejected the credentials (token / API key).");

                return Fail(result, WhatsAppFailure.ProviderRejected,
                    string.IsNullOrWhiteSpace(providerError) ? "WhatsApp provider did not accept the message." : $"WhatsApp provider did not accept the message: {providerError}");
            }
        }

        // ---------------------------------------------------------------- helpers

        private static WhatsAppSendResult Fail(WhatsAppSendResult result, WhatsAppFailure failure, string message)
        {
            result.Success = false;
            result.Failure = failure;
            result.Message = message;
            return result;
        }

        private static string? FirstValue(params string?[] values) =>
            values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim();

        // 9129346284 / +91 91293 46284 / 919129346284 -> 919129346284 ; anything else -> null
        private static string? ToWhatsAppNumber(string? raw, string countryCode)
        {
            var digits = Regex.Replace(raw ?? "", @"\D", "");
            if (digits.StartsWith("00")) digits = digits[2..];
            if (digits.Length == 11 && digits.StartsWith("0")) digits = digits[1..];

            var cc = string.IsNullOrWhiteSpace(countryCode) ? "91" : countryCode.Trim();
            if (digits.Length == 10 && Regex.IsMatch(digits, ValidMobileNoAttribute.Pattern))
                return cc + digits;
            if (digits.Length == 10 + cc.Length && digits.StartsWith(cc) && Regex.IsMatch(digits[cc.Length..], ValidMobileNoAttribute.Pattern))
                return digits;
            return null;
        }

        // WhatsApp Cloud API style response: { "messages":[{"id":"...","message_status":"accepted"}] }
        // error: { "error": { "message": "...", "code": ... } }
        private static (string? MessageId, string? Status, string? Error) ReadProviderResponse(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return (null, null, null);
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                string? id = null, status = null, error = null;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("messages", out var messages) && messages.ValueKind == JsonValueKind.Array && messages.GetArrayLength() > 0)
                    {
                        var first = messages[0];
                        if (first.TryGetProperty("id", out var idProp)) id = idProp.ToString();
                        if (first.TryGetProperty("message_status", out var st)) status = st.ToString();
                    }
                    if (root.TryGetProperty("error", out var err))
                        error = err.ValueKind == JsonValueKind.Object && err.TryGetProperty("message", out var em) ? em.ToString() : err.ToString();
                    else if (root.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String)
                        error = msg.GetString();
                }
                return (id, status, error);
            }
            catch (JsonException)
            {
                return (null, null, null);
            }
        }
    }
    
}
