using ApplicationInterface.Admin;
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

namespace Infrastructure.Admin
{
    public class RegistrationReceiptRepository : IRegistrationReceiptRepository
    {

        private readonly string _connectionString;
        public RegistrationReceiptRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString") ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<RegistrationReceiptResponse>> GetRegistrationReceiptAsync(RegistrationReceiptRequest request)
        {
            try
            {
                using IDbConnection db = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", request.GroupCode);
                parameters.Add("@BranchCode", request.BranchCode);
                parameters.Add("@SessionId", request.SessionId);
                parameters.Add("@RegistrationNo", request.RegistrationNo);
                parameters.Add("@FromDate", request.FromDate);
                parameters.Add("@ToDate", request.ToDate);
                var result = await db.QueryAsync<RegistrationReceiptResponse>("USP_GetRegistrationReceipt", parameters, commandType: CommandType.StoredProcedure);
                return result.ToList();
            }
            catch
            {
                return Enumerable.Empty<RegistrationReceiptResponse>();
            }
        }
    }
}
