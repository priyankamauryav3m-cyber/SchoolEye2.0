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
    public class DisabilityStudentService : IDisabilityStudentRepository
    {
        private readonly string _connectionString;

        public DisabilityStudentService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        


        public async Task<IEnumerable<DisabilityStudentResponse>> GetDisabilityStudentData(DisabilityStudentRequest request)
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
                param.Add("@IsDisability", request.IsDisability);
                param.Add("@Status", request.Status);
                return await con.QueryAsync<DisabilityStudentResponse>(
                    "STU_UspGetDisabilityStudentData",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<int> UpdateStudentDisabilityData(StudentDisabilityRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var studentTable = new DataTable();
                studentTable.Columns.Add("StudentId", typeof(long));
                studentTable.Columns.Add("IsDisability", typeof(bool));
                studentTable.Columns.Add("NatureOfDisability", typeof(string));
                foreach (var item in request.StudentDetails)
                {
                    studentTable.Rows.Add(item.StudentId, item.IsDisability, (object?)item.NatureOfDisability ?? DBNull.Value);
                }
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@StudentDetails", studentTable.AsTableValuedParameter("dbo.StudentDisabilityTVP"));
                param.Add("@CreatedBy", request.CreatedBy);
                param.Add("@Status", dbType: DbType.Int32, direction: ParameterDirection.Output);
                await con.ExecuteAsync(
                    "Usp_UpdateStudentDisabilityData",
                    param,
                    commandType: CommandType.StoredProcedure);
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
