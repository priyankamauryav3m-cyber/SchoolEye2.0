using ApplicationInterface.SchoolMaster;
using Dapper;
using DomainModel.SchoolMaster;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ServerWebAPI.Controllers.SchoolMaster
{
    public class MessageTypeService : IMessageTypeRepository
    {
        private readonly string _connectionString;
        public MessageTypeService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString") ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<MessageTypeModal>> GetMessageType()
        {
            try
            {
                using var db = new SqlConnection(_connectionString);
                string sql = "SELECT MessageTypeId,GroupCode,BranchCode,MessageType,IsValid FROM MstMessageType with(nolock)";
                return await db.QueryAsync<MessageTypeModal>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<IEnumerable<SmsEmailTextModel>> GetMessageType(int messageTypeId)
        {
            try
            {
                using var db = new SqlConnection(_connectionString);
                const string sql = @"SELECT * FROM DetSMSEmailText WHERE MessageTypeId = @MessageTypeId";
                return await db.QueryAsync<SmsEmailTextModel>(
                    sql,
                    new { MessageTypeId = messageTypeId }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }


    }
}
