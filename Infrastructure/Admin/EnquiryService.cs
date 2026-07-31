using ApplicationInterface.Admin;
using Dapper;
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
    public class EnquiryService:IEnquiryRepository
    {
        private readonly string _connectionString;

        public EnquiryService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<List<EnquiryListResponse>> GetEnquiryListofData(EnquiryRequestDto request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", request.GroupCode);
                parameters.Add("@BranchCode", request.BranchCode);
                parameters.Add("@SessionId", request.SessionId);
                parameters.Add("@Course", request.Course);
                parameters.Add("@FromDate", request.FromDate);
                parameters.Add("@ToDate", request.ToDate);
                parameters.Add("@DateWorkAs", request.DateWorkAs);
                parameters.Add("@FollowStatus", request.FollowStatus);
                parameters.Add("@AppliedFrom", request.AppliedFrom);
                parameters.Add("@StudentName", request.StudentName);
                parameters.Add("@MobileNo", request.MobileNo);
                var result = await connection.QueryAsync<EnquiryListResponse>(
                    "USP_GetEnquiryListData",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching enquiry list data", ex);
            }
        }
        public async Task<string> SubmitEnquiryData(EnquiryListResponse model)
        {
            try
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    var param = new DynamicParameters();
                    param.Add("@EnqId", model.EnquiryId);
                    param.Add("@groupCode", model.GroupCode);
                    param.Add("@branchCode", model.BranchCode);
                    param.Add("@SessionId", model.SessionId);
                    param.Add("@EnquiryDate", model.EnquiryDate);
                    param.Add("@ChildFirstName", model.StudentFirstName);
                    param.Add("@ChildMiddleName", model.StudentMiddleName);
                    param.Add("@ChildLastName", model.StudentLastName);
                    param.Add("@ClassCode", model.ClassCode);
                    param.Add("@DOB", model.DateOfBirth);
                    param.Add("@Gender", model.Gender);
                    param.Add("@Email", model.Email);
                    param.Add("@MobileNo", model.MobileNo);
                    param.Add("@AlternateContactNo", model.ContactNo);
                    param.Add("@FatherFirstName", model.FatherName);
                    param.Add("@MotherFirstName", model.MotherName);
                    param.Add("@Address", model.Address);
                    param.Add("@SourceOfEnquiry", model.SourceOfEnquiry);
                    param.Add("@Remarks", model.Remarks);
                    param.Add("@createdBy", model.CreatedBy);
                    param.Add("@IsOnline", model.IsOnline);
                    param.Add("@Area", model.Area);
                    param.Add("@EnquiryType", model.EnquiryType);
                    param.Add("@LastSchool", model.LastSchool);
                    // OUTPUT PARAM
                    param.Add("@EnquiryNo", dbType: DbType.String, size: 20, direction: ParameterDirection.Output);

                    await con.ExecuteAsync(
                        "ENQ_UspSubmitEnquiry",
                        param,
                        commandType: CommandType.StoredProcedure
                    );

                    var res = param.Get<string>("@EnquiryNo");
                    return res;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error submitting enquiry data", ex);
            }
        }

        public async Task<List<FollowupDetailsResponse>> GetFollowupDetails(SearchAnyRequestModel searchAnyRequest)
        {
            try
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    await con.OpenAsync();
                    var param = new DynamicParameters();
                    param.Add("@GroupCode", searchAnyRequest.GroupCode);
                    param.Add("@BranchCode", searchAnyRequest.BranchCode);
                    param.Add("@EnquiryId", searchAnyRequest.RequestName);
                    var result = await con.QueryAsync<FollowupDetailsResponse>(
                        "GetFollowupDetails",
                        param,
                        commandType: CommandType.StoredProcedure
                    );
                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                // General error
                throw new Exception("Something went wrong in GetFollowupDetails", ex);
            }
        }

        public async Task<string> AddFollowupDetails(AddFollowupRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                await con.OpenAsync();
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@EnquiryId", request.EnquiryId);
                param.Add("@FollowupDate", request.FollowupDate);
                param.Add("@FollowupStatus", request.FollowupStatus);
                param.Add("@NextFollowupDate", request.NextFollowupDate);
                param.Add("@Remarks", request.Remarks);
                param.Add("@NeverCall", request.NeverCall);
                param.Add("@CreatedBy", request.CreatedBy);
                param.Add("@InteractionVia", request.InteractionVia);
                param.Add("@NextFollowupAction", request.NextFollowupAction);
                param.Add("@ReminderDateTime", request.ReminderDateTime);
                // OUTPUT PARAM
                param.Add("@Result", dbType: DbType.String, size: 20, direction: ParameterDirection.Output);
                await con.ExecuteAsync(
                    "ENQ_UspAddFollowupDetails",
                    param,
                    commandType: CommandType.StoredProcedure
                );
                return param.Get<string>("@Result");
            }
            catch (Exception ex)
            {
                throw new Exception("Error while adding follow-up", ex);
            }
        }
        public async Task<DashboardResponse> GetDashboardAsync(
    int currentSessionId,
    int previousSessionId)
        {
            using var con = new SqlConnection(_connectionString);

            var multi = await con.QueryMultipleAsync(
                "sp_GetDashboard",
                new
                {
                    CurrentSessionId = currentSessionId,
                    PreviousSessionId = previousSessionId
                },
                commandType: CommandType.StoredProcedure);

            var response = new DashboardResponse();
            response.TotalStudents = await multi.ReadFirstAsync<DashboardCardDto>();
            response.NewAdmissions = await multi.ReadFirstAsync<DashboardCardDto>();
            response.TodaysCollection = await multi.ReadFirstAsync<DashboardCardDto>();
            response.OutstandingFees = await multi.ReadFirstAsync<DashboardCardDto>();

            response.TotalStudents.GraphData =
                (await multi.ReadAsync<DashboardGraphDto>()).ToList();

            response.NewAdmissions.GraphData =
                (await multi.ReadAsync<DashboardGraphDto>()).ToList();

            response.TodaysCollection.GraphData =
                (await multi.ReadAsync<DashboardGraphDto>()).ToList();

            response.OutstandingFees.GraphData =
                (await multi.ReadAsync<DashboardGraphDto>()).ToList();

            return response;
        }
    }
}
