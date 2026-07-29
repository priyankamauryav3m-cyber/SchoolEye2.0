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

namespace Infrastructure.SuperAdmin
{
    public class ApplicationStatusService : IApplicationStatus
    {
        private readonly string _connectionString;
        public ApplicationStatusService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString") ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<RegistrationStatusModel>> GetRegistrationStatus(string groupCode, string branchCode, string registrationNo, string sessionName)
        {
            try
            {
                using var db = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", groupCode);
                parameters.Add("@BranchCode", branchCode);
                parameters.Add("@SessionName", sessionName);
                parameters.Add("@RegistrationNo", registrationNo);
                var data = await db.QueryAsync<RegistrationStatusModel>(
                    "ADM_UspGetRegistrationStatus",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
                return data.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching registration status", ex);
            }
        }
    }
}
