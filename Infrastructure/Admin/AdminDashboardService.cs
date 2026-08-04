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
        public async Task<EnquiryDashboardResponse> GetDashboardAsync(EnquiryDashboardSearchRequest model)
        {   
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", model.GroupCode);
                param.Add("@BranchCode", model.BranchCode);
                param.Add("@SessionId", model.SessionId);
                param.Add("@ClassCode", model.ClassCode);
                param.Add("@Source", model.Source);
                param.Add("@Month", model.Month);
                param.Add("@FromDate", model.FromDate);
                param.Add("@ToDate", model.ToDate);

                using var multi = await con.QueryMultipleAsync(
                    "V3M_Get_EnquiryDashboard",
                    param,
                    commandType: CommandType.StoredProcedure);

                var response = new EnquiryDashboardResponse();
                response.Dashboard = await multi.ReadFirstOrDefaultAsync<EnquiryDashboardModel>();
                response.SessionComparison = (await multi.ReadAsync<SessionComparisonModel>()).ToList();
                response.FollowupDashboard = await multi.ReadFirstOrDefaultAsync<FollowupDashboardModel>();
                response.SourceReport = (await multi.ReadAsync<SourceReportModel>()).ToList();
                response.MonthWise = (await multi.ReadAsync<MonthWiseModel>()).ToList();
                response.MonthWiseAdmission = (await multi.ReadAsync<MonthWiseAdmissionModel>()).ToList();
                response.ClassWise =(await multi.ReadAsync<ClassWiseModel>()).ToList();
                response.ClassWiseAdmission =(await multi.ReadAsync<ClassWiseAdmissionModel>()).ToList();
                response.EnquirySummary =await multi.ReadFirstOrDefaultAsync<EnquirySummaryModel>();
                response.Pipeline =(await multi.ReadAsync<PipelineModel>()).ToList();
                response.RecentEnquiries = (await multi.ReadAsync<RecentEnquiryModel>()).ToList();
                response.RecentAdmissions =(await multi.ReadAsync<RecentAdmissionModel>()).ToList();
                response.FollowupStatus =(await multi.ReadAsync<FollowupStatusModel>()).ToList();
                await multi.ReadAsync<dynamic>();
                response.TodayFollowups = (await multi.ReadAsync<TodayFollowupModel>()).ToList();
                response.TodayAdmissions = (await multi.ReadAsync<TodayAdmissionModel>()).ToList();
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
    }
}
