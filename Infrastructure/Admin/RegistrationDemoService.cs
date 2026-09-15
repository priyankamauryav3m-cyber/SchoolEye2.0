using ApplicationInterface.Admin;
using Dapper;
using DomainModel.Admin;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Admin
{
    public class RegistrationDemoService : IRegistrationDemoRepository
    {
        private readonly string _connectionString;

        public RegistrationDemoService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async Task<int> AddRegistrationDemo(RegistrationDemoModel model)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", model.GroupCode);
                parameters.Add("@BranchCode", model.BranchCode);
                parameters.Add("@SessionId", model.SessionId);
                parameters.Add("@StudentName", model.StudentName);
                parameters.Add("@CreatedBy", model.CreatedBy);
                parameters.Add("@RegiNo", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(
                    "ADM_UspAddRegistrationDemo",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return parameters.Get<int>("@RegiNo");
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Database error while adding registration demo.", ex);
            }
        }

        public async Task<RegistrationDemoModel?> GetRegistrationDemoByRegiNo(string groupCode, string branchCode, long sessionId, int regiNo)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", groupCode);
                parameters.Add("@BranchCode", branchCode);
                parameters.Add("@SessionId", sessionId);
                parameters.Add("@RegiNo", regiNo);

                return await connection.QueryFirstOrDefaultAsync<RegistrationDemoModel>(
                    "ADM_UspGetRegistrationDemo",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Database error while fetching registration demo.", ex);
            }
        }

        public async Task<IEnumerable<RegistrationDemoModel>> GetRegistrationDemoList(string groupCode, string branchCode, long sessionId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", groupCode);
                parameters.Add("@BranchCode", branchCode);
                parameters.Add("@SessionId", sessionId);

                var result = await connection.QueryAsync<RegistrationDemoModel>(
                    "ADM_UspGetRegistrationDemoList",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Database error while fetching registration demo list.", ex);
            }
        }
    }
}
