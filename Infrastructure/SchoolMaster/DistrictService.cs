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
    public class DistrictService: IDistrictRepository
    {
        private readonly string _connectionString;

        public DistrictService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException( "DatabaseSettings1:ConnectionString");
        }

        public async Task<IEnumerable<DistrictModel>> GetAllAsync()
        {
            try
            {
                using var db = new SqlConnection(_connectionString);
                string sql = "SELECT DistrictId,DistrictName,Remarks,StateId,IsValid,CreatedDate,CreatedBy FROM MstDistrict with(nolock)";
                return await db.QueryAsync<DistrictModel>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<int> DeleteAsync(int districtId)
        {
            try
            {
                using var db = new SqlConnection(_connectionString);
                string sql = @"UPDATE MstDistrict SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END 
                       WHERE DistrictId = @DistrictId";

                return await db.ExecuteAsync(sql, new { DistrictId = districtId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }


        public async Task<string> AddUpdateDistrict(DistrictModel objDistrict)
        {
            try
            {
                string returnValue;
                var sqlQry = "V3M_InsertUpdate_District";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@DistrictId", objDistrict.DistrictId);
                        cmd.Parameters.AddWithValue("@StateId", objDistrict.StateId);
                        cmd.Parameters.AddWithValue("@DistrictName", objDistrict.DistrictName);
                        cmd.Parameters.AddWithValue("@IsValid", objDistrict.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", objDistrict.CreatedBy);
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
