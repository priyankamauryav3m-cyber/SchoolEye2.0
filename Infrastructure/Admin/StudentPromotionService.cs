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
    public  class StudentPromotionService:IStudentPromotionRepository
    {

        private readonly string _connectionString;
        public StudentPromotionService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<ClassWiseStudentForPromotion>> GetClassWiseStudentPromotion(SearchAnyRequestModel searchAny)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                DynamicParameters param = new DynamicParameters();
                    param.Add("@GroupCode", searchAny.GroupCode);
                    param.Add("@BranchCode", searchAny.BranchCode);
                    param.Add("@SessionId", searchAny.SessionId);
                    param.Add("@ClassCode", searchAny.RequestName);
                    param.Add("@SectionId", searchAny.RequestId);
                    var result = await con.QueryAsync<ClassWiseStudentForPromotion>(
                        "USP_GetClassWiseStudentForPromotion",
                        param,
                        commandType: CommandType.StoredProcedure);
                    return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while Class Wise Student Promotion List", ex);
            }
        }

        public async Task<List<string>> PromoteStudentClass(List<PromoteClassModel> promoteList)
        {
            try
            {
                List<string> results = new();
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    foreach (var promote in promoteList)
                    {
                        using (SqlCommand cmd = new SqlCommand("STU_UspPromoteClass", connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@GroupCode", promote.GroupCode);
                            cmd.Parameters.AddWithValue("@BranchCode", promote.BranchCode);
                            cmd.Parameters.AddWithValue("@SessionId", promote.SessionId);
                            cmd.Parameters.AddWithValue("@StudentId", promote.StudentId);
                            cmd.Parameters.AddWithValue("@ClassCode", promote.ClassCode);
                            cmd.Parameters.AddWithValue("@SectionId", promote.SectionId);
                            cmd.Parameters.AddWithValue("@PromoteType", promote.PromoteType);
                            cmd.Parameters.AddWithValue("@PromoteComment", promote.PromoteComment);
                            cmd.Parameters.AddWithValue("@CreatedBy", promote.CreatedBy);
                            SqlParameter returnValueParam = new SqlParameter
                            {
                                ParameterName = "@Result",
                                SqlDbType = SqlDbType.VarChar,
                                Size = 50,
                                Direction = ParameterDirection.Output
                            };
                            cmd.Parameters.Add(returnValueParam);
                            await cmd.ExecuteNonQueryAsync();
                            string result = returnValueParam.Value?.ToString() ?? "";
                            results.Add(result);
                        }
                    }
                }
                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }

        }

        public async Task<IEnumerable<StudentNotPromotedModel>> GetAllNotPromotedStudent(SearchAnyRequestModel searchAny)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                DynamicParameters param = new DynamicParameters();
                param.Add("@GroupCode", searchAny.GroupCode);
                param.Add("@BranchCode", searchAny.BranchCode);
                param.Add("@SessionId", searchAny.SessionId);
                var result = await con.QueryAsync<StudentNotPromotedModel>(
                    "V3M_USP_StudentNotPromoted",
                    param,
                    commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while Class Wise Student Promotion List", ex);
            }
        }
    }
}
