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
    public class StudentImageService : IStudentImageRepository
    {
        private readonly string _connectionString;

        public StudentImageService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async Task<IEnumerable<StudentImageResponse>> GetStudentImageData(StudentImageRequest request)
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

                return await con.QueryAsync<StudentImageResponse>(
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
    }
}
