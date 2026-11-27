using ApplicationInterface.SchoolAdmin;
using Dapper;
using DomainModel.SchoolAdmin;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Infrastructure.SchoolAdmin
{
    public class SummerActivityRegistrationViewService : ISummerActivityRegistrationView
    {
        private readonly string _connectionString;

        public SummerActivityRegistrationViewService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async Task<IEnumerable<SummerActivityRegistrationListModel>> GetRegistrationsAsync(SummerActivityRegistrationSearchModel search)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryAsync<SummerActivityRegistrationListModel>("Usp_GetSummerActivityRegistrationList",
                new
                {
                    search.GroupCode,
                    search.BranchCode,
                    search.SessionId,
                    search.ActivityId,
                    search.Valid,
                    PaymentMode = search.PaymentMode?.Trim() ?? ""
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<SummerActivityRegistrationFieldValue>> GetRegistrationDetailAsync(SummerActivityRegistrationKeyModel model)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryAsync<SummerActivityRegistrationFieldValue>("Usp_GetSummerActivityRegistrationDetail",
                new { model.GroupCode, model.BranchCode, OAId = model.RegistrationId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<string>> GetPaymentModesAsync(string groupCode, string branchCode)
        {
            using var con = new SqlConnection(_connectionString);
            return await con.QueryAsync<string>("Usp_GetSummerActivityPaymentModes",
                new { GroupCode = groupCode, BranchCode = branchCode },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> ActiveDeactiveAsync(SummerActivityRegistrationStatusModel model)
        {
            using var con = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@GroupCode", model.GroupCode);
            param.Add("@BranchCode", model.BranchCode);
            param.Add("@OAId", model.RegistrationId);
            param.Add("@IsValid", model.IsValid);
            param.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.Output);
            await con.ExecuteAsync("Usp_ActiveDeactiveSummerActivityRegistration", param, commandType: CommandType.StoredProcedure);
            return param.Get<int?>("@Result") ?? -1;
        }
    }
}
