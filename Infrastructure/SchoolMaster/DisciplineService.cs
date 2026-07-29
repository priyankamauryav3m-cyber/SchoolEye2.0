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
    public class DisciplineService : IDisciplineRepository
    {
        private readonly string _connectionString;

        public DisciplineService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<DisciplineModel>> GetAllAsync()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT DisciplineId,LevelId,GroupCode,BranchCode,DisciplineName,Remarks,IsValid,CreatedDate,CreatedBy FROM V3M_DIS_Mst_Discipline ORDER BY DisciplineId ASC";
                return await con.QueryAsync<DisciplineModel>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<int> DeleteDisciplineData(int disciplineId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE V3M_DIS_Mst_Discipline SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE DisciplineId = @DisciplineId";
                return await con.ExecuteAsync(sql, new { DisciplineId = disciplineId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<string> AddUpdateDiscipline(DisciplineModel objDiscipline)
        {
            try
            {
                string returnValue;
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("V3M_InsertUpdate_Discipline", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@DisciplineId", objDiscipline.DisciplineId);
                        cmd.Parameters.AddWithValue("@LevelId", objDiscipline.LevelId);
                        cmd.Parameters.AddWithValue("@GroupCode", objDiscipline.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", objDiscipline.BranchCode);
                        cmd.Parameters.AddWithValue("@DisciplineName", objDiscipline.DisciplineName);
                        cmd.Parameters.AddWithValue("@Remarks", (object)objDiscipline.Remarks ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsValid", objDiscipline.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", objDiscipline.CreatedBy);
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
                throw new Exception("Error while inserting/updating Discipline", ex);
            }
        }
    }
}
