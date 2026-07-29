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
    public class DesignationService : IDesignationRepository
    {

        private readonly string _connectionString;

        public DesignationService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<int> DeleteDesignationAsync(int desigId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE MstDesignation
                       SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE DesignationId = @DesignationId";
                return await con.ExecuteAsync(sql, new { DesignationId = desigId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<IEnumerable<DesignationModel>> GetAllDesignationAsync()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT DesignationId,DesignationName,Remarks,GroupCode,IsValid,CreatedDate,CreatedBy FROM MstDesignation with(nolock) ORDER BY DesignationId ASC";
                return await con.QueryAsync<DesignationModel>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<string> AddUpdateDesignation(DesignationModel objDesignation)
        {
            try
            {
                string returnValue;
                var sqlQry = "V3M_InsertUpdate_Designation";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@DesignationId", objDesignation.DesignationId);
                        cmd.Parameters.AddWithValue("@DesignationName", objDesignation.DesignationName);
                        cmd.Parameters.AddWithValue("@GroupCode", 01);                   
                        cmd.Parameters.AddWithValue("@IsValid", objDesignation.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", objDesignation.CreatedBy);
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
    }
}
