using ApplicationInterface.Admin;
using Dapper;
using DomainModel.Admin;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Admin
{
    public class AdmissionDateService : IAdmissionDateRepository
    {
        private readonly string _connectionString;

        public AdmissionDateService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> UpdateStudentAdmissionDate(UpdateStudentAdmissionDateRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                var param = new DynamicParameters();

                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@StudentDetails", request.StudentDetails);
                param.Add("@UserName", request.UserName);
                param.Add("@ReturnValue", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);
                await con.ExecuteAsync("STU_UspUpdateStudentAdmissionDate", param, commandType: CommandType.StoredProcedure);

                return param.Get<string>("@ReturnValue") ?? string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error in UpdateStudentAdmissionDate: {ex.Message}");

                throw;
            }
        }

        public async Task<IEnumerable<StuSearchedStudentResponse>> GetSearchedStudent(StuSearchedStudentRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@ClassCode", string.IsNullOrWhiteSpace(request.ClassCode) ? null : request.ClassCode);
                param.Add("@SectionCode", string.IsNullOrWhiteSpace(request.SectionCode) ? null : request.SectionCode);
                param.Add("@Gender", string.IsNullOrWhiteSpace(request.Gender) ? null : request.Gender);
                param.Add("@ControlNo", string.IsNullOrWhiteSpace(request.ControlNo) ? null : request.ControlNo);
                param.Add("@StudentName", string.IsNullOrWhiteSpace(request.StudentName) ? null : request.StudentName);
                param.Add("@IsEWS", string.IsNullOrWhiteSpace(request.IsEWS) ? null : request.IsEWS);
                param.Add("@JoinType", string.IsNullOrWhiteSpace(request.JoinType) ? null : request.JoinType);
                var data = await con.QueryAsync<StuSearchedStudentResponse>(
                    "STU_UspGetSearchedStudent",
                    param,
                    commandType: CommandType.StoredProcedure);
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<IEnumerable<ClassRegistrationDocumentsResponse>> GetClassRegistrationDocumentsAsync(ClassRegistrationDocumentsRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@ClassCode", request.ClassCode);
                param.Add("@DocumentType", request.DocumentType);
                param.Add("@RegistrationId", request.RegistrationId);
                return await con.QueryAsync<ClassRegistrationDocumentsResponse>(
                    "USP_GetClassRegistrationDocuments",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"GetClassRegistrationDocuments Error: {ex.Message}");

                throw;
            }
        }
    }
}
