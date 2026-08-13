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
    public class PrepareService:IPrepareRepository
    {
        private readonly string _connectionString;
        public PrepareService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString") ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<RegistrationInfoListResponse>> GetRegistrationInfoList(RegistrationInfoListRequest model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", model.GroupCode);
                param.Add("@BranchCode", model.BranchCode);
                param.Add("@SessionId", model.SessionId);
                param.Add("@ClassCode", model.ClassCode);
                param.Add("@StreamCode", model.StreamCode);
                param.Add("@RegistrationFrom", model.RegistrationFrom);
                param.Add("@RegistrationTo", model.RegistrationTo);
                param.Add("@Gender", model.Gender);
                param.Add("@DistanceFromSchool", model.DistanceFromSchool);
                param.Add("@PointsFrom", model.PointsFrom);
                param.Add("@PointsTo", model.PointsTo);
                param.Add("@ListPrepared", model.ListPrepared);
                param.Add("@ApplicationStatus", model.ApplicationStatus);
                return await con.QueryAsync<RegistrationInfoListResponse>(
                    "GetRegistrationInfoList",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<int> AddPublishList(PublishListModel model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();

                parameters.Add("@GroupCode", model.GroupCode);
                parameters.Add("@BranchCode", model.BranchCode);
                parameters.Add("@SessionId", model.SessionId);
                parameters.Add("@ListName", model.ListName);
                parameters.Add("@CreatedBy", model.CreatedBy);
                return await con.ExecuteScalarAsync<int>(
                    "USP_ADM_AddPublishList",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<RegistrationInfoListRequest>> GetListStatusData(PublishListModel request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@ListNo", request.ListNo);
                var result = await con.QueryAsync<RegistrationInfoListRequest>(
                    "USP_GetListStatus",
                    param,
                    commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<PublishListModel>> GetAllAsync(SearchAnyRequestModel request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                string sql = @"
            SELECT
                LID,
                GroupCode,
                BranchCode,
                ListNo,
                ListName,
                PublishStatus,
                PublishDate,
                CreatedDate,
                CreatedBy,
                IsActive,
                SessionId
            FROM ADM_PublishList WITH (NOLOCK)
            WHERE GroupCode = @GroupCode
              AND BranchCode = @BranchCode
              AND SessionId = @SessionId
            ORDER BY LID DESC";

                return await con.QueryAsync<PublishListModel>(
                    sql,
                    new
                    {
                        request.GroupCode,
                        request.BranchCode,
                        request.SessionId
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<int> AddStudentInListAsync(AddStudentInListRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();

                parameters.Add("@GroupCode", request.GroupCode, DbType.String);
                parameters.Add("@BranchCode", request.BranchCode, DbType.String);
                parameters.Add("@SessionId", request.SessionId);
                parameters.Add("@RegistrationId", request.RegistrationId);
                parameters.Add("@PublishListNo", request.PublishListNo);
                parameters.Add("@CreatedBy", request.CreatedBy, DbType.String);

                return await con.ExecuteScalarAsync<int>("USP_AddStudentInList",parameters,commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<int> DeleteStudentInListAsync(AddStudentInListRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();

                parameters.Add("@GroupCode", request.GroupCode, DbType.String);
                parameters.Add("@BranchCode", request.BranchCode, DbType.String);
                parameters.Add("@SessionId", request.SessionId);
                parameters.Add("@RegistrationId", request.RegistrationId);
                parameters.Add("@PublishListNo", request.PublishListNo);
                parameters.Add("@CreatedBy", request.CreatedBy, DbType.String);

                return await con.ExecuteScalarAsync<int>("USP_DeleteStudentFromList", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<int> PublishStudentInListAsync(AddStudentInListRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();

                parameters.Add("@GroupCode", request.GroupCode, DbType.String);
                parameters.Add("@BranchCode", request.BranchCode, DbType.String);
                parameters.Add("@SessionId", request.SessionId);
                parameters.Add("@ListNo", request.PublishListNo);

                return await con.ExecuteScalarAsync<int>("USP_PrepareList", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<IEnumerable<RegistrationInfoListResponse>> GetPublishingListDetails(RegistrationInfoListRequest model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", model.GroupCode);
                param.Add("@BranchCode", model.BranchCode);
                param.Add("@SessionId", model.SessionId);
                param.Add("@ListNo", model.ListNo);
                param.Add("@ClassCode", model.ClassCode);
                param.Add("@AppStatus", model.ApplicationStatus);
                return await con.QueryAsync<RegistrationInfoListResponse>(
                    "USP_GetPublishingListDetails",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

    }
}
