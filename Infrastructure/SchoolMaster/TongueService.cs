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
    public class TongueService : ITongueRepository
    {
        private readonly string _connectionString;

        public TongueService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<TongueModel>> GetAllAsync()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT TongueId,TongueName,Remarks,DisplayOrder,IsValid,CreatedDate,CreatedBy FROM MstMotherTongue ORDER BY TongueId ASC";
                return await con.QueryAsync<TongueModel>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<int> DeleteTongueData(int tongueId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE MstMotherTongue SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE TongueId = @TongueId";
                return await con.ExecuteAsync(sql, new { TongueId = tongueId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<string> AddUpdateTongue(TongueModel objTongue)
        {
            try
            {
                string returnValue;
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("V3M_InsertUpdate_MotherTongue", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@TongueId", objTongue.TongueId);
                        cmd.Parameters.AddWithValue("@TongueName", objTongue.TongueName);
                        cmd.Parameters.AddWithValue("@Remarks", (object)objTongue.Remarks ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DisplayOrder", objTongue.DisplayOrder);
                        cmd.Parameters.AddWithValue("@IsValid", objTongue.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", objTongue.CreatedBy);
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
                throw new Exception("Error while inserting/updating Mother Tongue", ex);
            }
        }
    }
}
