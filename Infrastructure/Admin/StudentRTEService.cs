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
    public class StudentRTEService : IStudentRTERepository
    {
        private readonly string _connectionString;

        public StudentRTEService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async Task<IEnumerable<StudentRTEResponse>> GetSearchedStudentRTE(StudentRTERequest request)
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
                param.Add("@AdmissionNo", request.AdmissionNo);
                param.Add("@StudentName", request.StudentName);
                param.Add("@IsRTE", request.IsRTE);
                param.Add("@RTECategory", request.RTECategory);
                param.Add("@Status", request.Status);
                return await con.QueryAsync<StudentRTEResponse>(
                    "STU_UspGetSearchedStudentRTE",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<int> AddUpdateRTEStudentData(RTEStudentDataRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@StudentDetails", request.StudentDetails);
                param.Add("@CreatedBy", request.CreatedBy);
                param.Add("@Status", dbType: DbType.Int32, direction: ParameterDirection.Output);
                await con.ExecuteAsync("Usp_RTEStudentData",param, commandType: CommandType.StoredProcedure);
                return param.Get<int>("@Status");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
    }
}
