using ApplicationInterface.Admin;
using Dapper;
using DomainModel.Admin;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Infrastructure.Admin
{
    public class StudentDisciplineService : IStudentDisciplineRepository
    {
        private readonly string _connectionString;
        public StudentDisciplineService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddStudentDiscipline(AddStudentDisciplineRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@CategoryId", request.CategoryId);
                param.Add("@StudentId", request.StudentId);
                param.Add("@IndisciplineComment", request.IndisciplineComment);
                param.Add("@Fine", request.Fine);
                param.Add("@IndisciplineDate", request.IndisciplineDate);
                param.Add("@CreatedBy", request.CreatedBy);
                param.Add("@ReturnValue", dbType: DbType.String, size: -1, direction: ParameterDirection.Output);
                await con.ExecuteAsync(
                    "STU_UspAddStudentDiscipline",
                    param,
                    commandType: CommandType.StoredProcedure);
                var returnValue = param.Get<string?>("@ReturnValue");
                if (returnValue != "1")
                {
                    Console.WriteLine($"STU_UspAddStudentDiscipline failed: {returnValue}");
                }
                return returnValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<IndisciplineCommentResponse>> GetIndisciplineComment(IndisciplineCommentRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@CategoryId", request.CategoryId);
                param.Add("@ClassCode", request.ClassCode);
                param.Add("@SectionId", request.SectionId); 
                param.Add("@FromDate", request.FromDate);
                param.Add("@ToDate", request.ToDate);
                param.Add("@StudentNo", request.StudentNo);
                param.Add("@StudentName", request.StudentName);
                param.Add("@Mode", request.Mode);
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                return await con.QueryAsync<IndisciplineCommentResponse>(
                    "STU_UspGetIndisciplineComment",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<SiblingStudentResponse>> GetSearchedSiblingStudent(SiblingStudentRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@ClassCode", request.ClassCode);
                param.Add("@SectionCode", request.SectionCode);
                param.Add("@Gender", request.Gender);
                param.Add("@ControlNo", request.ControlNo);
                param.Add("@StudentName", request.StudentName);
                param.Add("@JoinType", request.JoinType);
                return await con.QueryAsync<SiblingStudentResponse>(
                    "STU_UspGetSearchedSiblingStudent",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<SiblingStudentResponse>> GetSearchedStudent(SiblingStudentRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@ClassCode", request.ClassCode);
                param.Add("@SectionCode", request.SectionCode);
                param.Add("@Gender", request.Gender);
                param.Add("@ControlNo", request.ControlNo);
                param.Add("@StudentName", request.StudentName);
                param.Add("@IsEWS", request.IsEWS);
                param.Add("@JoinType", request.JoinType);
                return await con.QueryAsync<SiblingStudentResponse>(
                    "STU_UspGetSearchedStudent",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<int> EnableStudentDiscipline(IndisciplineCommentResponse request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@DisId", request.DisId);
                param.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.Output);
                await con.ExecuteAsync(
                    "V3M_DIS_UspEnableStudentDiscipline",
                    param,
                    commandType: CommandType.StoredProcedure);
                return param.Get<int>("@ReturnValue");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<int> DisbleStudentDiscipline(IndisciplineCommentResponse request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@DisId", request.DisId);
                param.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.Output);
                await con.ExecuteAsync(
                    "DIS_UspDisbleStudentDiscipline",
                    param,
                    commandType: CommandType.StoredProcedure);
                return param.Get<int>("@ReturnValue");
            }
            catch (Exception ex)    
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

    }
}
