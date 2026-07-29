using ApplicationInterface.SchoolMaster;
using Dapper;
using DomainModel.Admin;
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
    public class ReligionService:IReligionRepository
    {
        private readonly string _connectionString;

        public ReligionService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddUpdateReligion(ReligionMaster objreligion)
        {
            try
            {
                string returnValue;
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand("V3M_InsertUpdate_religion", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ReligionId", objreligion.ReligionId);
                        cmd.Parameters.AddWithValue("@ReligionName", objreligion.ReligionName);
                        cmd.Parameters.AddWithValue("@Remarks", objreligion.Remarks);
                        cmd.Parameters.AddWithValue("@IsValid", objreligion.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedDate", objreligion.CreatedDate);
                        cmd.Parameters.AddWithValue("@CreatedBy", objreligion.CreatedDate);
                        SqlParameter returnValueParam = new SqlParameter("@ReturnValue", SqlDbType.VarChar, 50)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(returnValueParam);

                        await cmd.ExecuteNonQueryAsync();
                        returnValue = returnValueParam.Value?.ToString() ?? string.Empty;
                    }
                }
                return returnValue;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to insert/update Group", ex);
            }
        }
        public async Task<int> DeleteReligion(int ReligionId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE MstReligion SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END 
                       WHERE ReligionId = @ReligionId";
                return await con.ExecuteAsync(sql, new { ReligionId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<IEnumerable<ReligionMaster>> GetReligion()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT ReligionId,ReligionName,Remarks,IsValid FROM MstReligion";
                return await con.QueryAsync<ReligionMaster>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
    }
}

