using ApplicationInterface.Admin;
using Dapper;
using DomainModel.Admin;
using DomainModel.FinanceMNGT;
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
    public class AdminDashboardService : IAdminDashboard
    {
        private readonly string _connectionString;
        public AdminDashboardService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString") ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async Task<AdminDashboardModal> GetAdminDashboardData(SearchAnyRequestModel model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", model.GroupCode);
                parameters.Add("@BranchCode", model.BranchCode);
                parameters.Add("@SessionId", model.SessionId);


                var data = await con.QueryFirstOrDefaultAsync<AdminDashboardModal>("USP_AdminDashboard", parameters, commandType: CommandType.StoredProcedure);
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<List<FeeHeadCollectionDto>> GetFeeHeadCollectionSummary(SearchAnyRequestModel model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();

                param.Add("@GroupCode", model.GroupCode);
                param.Add("@BranchCode", model.BranchCode);
                param.Add("@SessionId", model.SessionId);
                param.Add("@Filter", model.RequestName);

                var result = await con.QueryAsync<FeeHeadCollectionDto>(
                    "USP_FeeCollectionDashboard",
                    param,
                    commandType: CommandType.StoredProcedure);

                return result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        public async Task<AdmissionDashboardModel> GetAdmissionData(SearchAnyRequestModel model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", model.GroupCode);
                parameters.Add("@BranchCode", model.BranchCode);
                parameters.Add("@SessionId", model.SessionId);
                
                    var result=await con.QueryFirstOrDefaultAsync<AdmissionDashboardModel>(
                    "USP_GetAdmissionDashboard",
                    parameters,
                    commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

    }
}
