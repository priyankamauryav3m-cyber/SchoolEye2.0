using ApplicationInterface.Admin;
using Dapper;
using DomainModel.Admin;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Infrastructure.Admin
{
    public class AbscondedStudentService : IAbscondedStudentRepository
    {
        private readonly string _connectionString;

        public AbscondedStudentService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString") ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async Task<IEnumerable<GetAbscondedStudentResponse>> GetAbscondedStudent(GetAbscondedStudentRequest request)
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
                param.Add("@ControlNo", request.ControlNo);
                param.Add("@StudentName", request.StudentName);
                param.Add("@IsEWS", request.IsEWS);
                param.Add("@JoinType", request.JoinType);
                param.Add("@StudentStatus", request.StudentStatus);

                return await con.QueryAsync<GetAbscondedStudentResponse>(
                    "STU_UspGetAbscondedStudent",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<string> AbscondStudent(GetAbscondedStudentRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@StudentId", request.StudentId);
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@ReturnValue", dbType: DbType.String, size: -1, direction: ParameterDirection.Output);
                await con.ExecuteAsync(
                    "STU_UspAbscondStudent",
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

        public async Task<string> UnAbscondStudent(GetAbscondedStudentRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@StudentId", request.StudentId);
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@ReturnValue", dbType: DbType.String, size: -1, direction: ParameterDirection.Output);
                await con.ExecuteAsync(
                    "STU_UspUnAbscondStudent",
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
