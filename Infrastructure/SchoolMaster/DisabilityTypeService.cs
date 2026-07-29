using ApplicationInterface.SchoolMaster;
using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
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
    public class DisabilityTypeService : IDisabilityTypeRepository
    {
        private readonly string _connectionString;

        public DisabilityTypeService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddUpdateDisabilityType(DisabilityTypeModel objdisabilitytype)
        {
            try
            {
                string returnValue;
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand("V3M_InsertUpdateDisabilityType", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@DisabilityTypeId", objdisabilitytype.SeedId);
                        cmd.Parameters.AddWithValue("@GroupCode", objdisabilitytype.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", objdisabilitytype.BranchCode);
                        cmd.Parameters.AddWithValue("@DisplayOrder", objdisabilitytype.DisplayOrder);
                        cmd.Parameters.AddWithValue("@DisabilityTypeName", objdisabilitytype.DisabilityType);
                        cmd.Parameters.AddWithValue("@IsValid", objdisabilitytype.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", objdisabilitytype.CreatedBy);
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
                throw new Exception("Failed to insert/update DisabilityType", ex);
            }
        }
        public async Task<int> DeleteDisabilityType(int SeedId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE Mst_DisabilityType SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE SeedId = @SeedId";
                return await con.ExecuteAsync(sql, new { SeedId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<IEnumerable<DisabilityTypeModel>> GetDisabilityType()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT SeedId,DisabilityType,DisplayOrder,IsValid FROM Mst_DisabilityType";
                return await con.QueryAsync<DisabilityTypeModel>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
    }
}
