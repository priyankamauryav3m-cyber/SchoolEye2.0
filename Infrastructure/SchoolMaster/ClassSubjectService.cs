using ApplicationInterface.SchoolMaster;
using Dapper;
using DomainModel.SchoolMaster;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.SchoolMaster
{
    public class ClassSubjectService : IClassSubjectRepository
    {
        private readonly string _connectionString;

        public ClassSubjectService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<ClassSubjectModel>> GetAllAsync()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var sql = "SELECT MapId,ClassCode,SubjectCode,GroupCode,BranchCode,IsValid,IsPracticalSubject,IsOptionalSubject,IsScholasticSubject," +
                    "IsReportCardSubject,IsCalculatedSubject,DisplayOrder,IsLanguage,SemesterId FROM MstClassSubject with(nolock)";
                return await con.QueryAsync<ClassSubjectModel>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<string> AddUpdateClassSubject(ClassSubjectModel model)
        {
            try
            {
                string returnValue;
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("V3M_Insert_MstClassSubject", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MapId", model.MapId);
                        cmd.Parameters.AddWithValue("@ClassCode", model.ClassCode);
                        cmd.Parameters.AddWithValue("@SubjectCode", model.SubjectCode);
                        cmd.Parameters.AddWithValue("@GroupCode", model.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", model.BranchCode);
                        cmd.Parameters.AddWithValue("@IsValid", model.IsValid);
                        cmd.Parameters.AddWithValue("@IsOptionalSubject", model.IsOptionalSubject);
                        cmd.Parameters.AddWithValue("@IsScholasticSubject", model.IsScholasticSubject);
                        cmd.Parameters.AddWithValue("@IsReportCardSubject", model.IsReportCardSubject);
                        cmd.Parameters.AddWithValue("@IsCalculatedSubject", model.IsCalculatedSubject);
                        cmd.Parameters.AddWithValue("@DisplayOrder", model.DisplayOrder);
                        cmd.Parameters.AddWithValue("@IsLanguage", model.IsLanguage);
                        cmd.Parameters.AddWithValue("@SemesterId", model.SemesterId);
                        SqlParameter returnValueParam = new SqlParameter
                        {
                            ParameterName = "@ReturnValue",
                            SqlDbType = SqlDbType.VarChar,
                            Size = 50,
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(returnValueParam);

                        await cmd.ExecuteNonQueryAsync();

                        returnValue = returnValueParam.Value?.ToString();
                    }
                }
                return returnValue;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while inserting/updating Class Subject", ex);
            }
        }
        public async Task<int> DeleteAsync(int mapId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var sql = "UPDATE MstClassSubject SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END WHERE MapId = @MapId";
                return await con.ExecuteAsync(sql, new { MapId = mapId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

    }
}
