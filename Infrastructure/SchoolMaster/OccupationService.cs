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
    public class OccupationService : IOccupationRepository
    {
        private readonly string _connectionString;

        public OccupationService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddUpdateOccupation(OccupationModal objoccuption)
        {
            try
            {
                string returnValue;
                var sqlQry = "V3M_InsertUpdate_Occupation";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@OccupationId", objoccuption.OccupationId);
                        cmd.Parameters.AddWithValue("@OccupationName", objoccuption.OccupationName);
                        cmd.Parameters.AddWithValue("@Remarks", (object?)objoccuption.Remarks ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsValid", objoccuption.IsValid);
                        cmd.Parameters.AddWithValue("@Type", objoccuption.Type);
                        cmd.Parameters.AddWithValue("@CreatedDate", objoccuption.CreatedDate);
                        cmd.Parameters.AddWithValue("@CreatedBy", objoccuption.CreatedBy);
                        var returnValueParam = new SqlParameter
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
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<int> DeleteOccupation(int occupationId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE MstOccupation 
                       SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END WHERE OccupationId = @OccupationId";
                return await con.ExecuteAsync(sql, new { OccupationId = occupationId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<IEnumerable<OccupationModal>> GetOccupation()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT * FROM MstOccupation with(nolock)";
                return await con.QueryAsync<OccupationModal>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

    }
}
