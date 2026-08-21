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
    public class StudentRollNoService:IStudentRollNoRepository
    {
        private readonly string _connectionString;

        public StudentRollNoService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<int> ViewStudentRollNoPreference(MapStudentRollNoRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@ClassCode", request.ClassCode);
                param.Add("@SectionId", request.SectionId);
                param.Add("@OrderBy", request.OrderBy);
                param.Add("@CreatedBy", request.CreatedBy);
                param.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.Output);
                await con.ExecuteAsync(
                    "STU_UspMapStudentRollNoByPreference",
                    param,
                    commandType: CommandType.StoredProcedure);
                return param.Get<int>("@Result");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<IEnumerable<AdmSearchedStudentResponse>> GetSearchedStudentRollNo(AdmSearchedStudentRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@ClassCode", request.ClassCode);
                param.Add("@SectionCode", request.SectionId);
                param.Add("@Gender", request.Gender);
                param.Add("@ControlNo", request.ControlNo);
                param.Add("@StudentName", request.StudentName);
                param.Add("@IsEWS", request.IsEWS);
                param.Add("@JoinType", request.JoinType);

                return await con.QueryAsync<AdmSearchedStudentResponse>(
                    "V3M_ADM_UspGetSearchedStudent",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<string?> AllocateSection(AllocateSectionRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@StudentId", request.StudentId);
                param.Add("@SessionId", request.SessionId);
                param.Add("@ClassCode", request.ClassCode);
                param.Add("@SectionId", request.SectionId);
                param.Add("@CreatedBy", request.CreatedBy);
                param.Add("@ReturnValue", dbType: DbType.String, size: -1, direction: ParameterDirection.Output);
                await con.ExecuteAsync(
                    "STU_UspAllocateSection",
                    param,
                    commandType: CommandType.StoredProcedure);
                return param.Get<string?>("@ReturnValue");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

    }
}
