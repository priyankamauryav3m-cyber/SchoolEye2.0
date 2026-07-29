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
using static System.Net.WebRequestMethods;

namespace Infrastructure.SchoolMaster
{
    public class SendSMSService : ISendSMS
    {

        private readonly string _connectionString;
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        public SendSMSService(IConfiguration configuration, HttpClient http = null, IConfiguration config = null)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
            ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
            _http = http;
            _config = config;
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
    }
    
}
