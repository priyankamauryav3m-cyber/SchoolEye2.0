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
    public class FindStudentService : IFindStudentRepository
    {
        private readonly string _connectionString;

        public FindStudentService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }



        public async Task<IEnumerable<FindStudentResponse>> GetSearchedStudentByDetails(FIndStudentRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@AdmissionNo", request.AdmissionNo);
                param.Add("@StudentName", request.StudentName);
                param.Add("@FatherName", request.FatherName);
                param.Add("@MotherName", request.MotherName);
                param.Add("@SMSMobileNo", request.SMSMobileNo);
                return await con.QueryAsync<FindStudentResponse>(
                    "STU_UspGetSearchedStudentByDetails",
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
