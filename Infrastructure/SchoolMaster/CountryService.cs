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
    public class CountryService : ICountryRepository
    {
        private readonly string _connectionString;

        public CountryService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<CountryModel>> GetAllAsync()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT CountryId,CountryCode,CountryName,Remarks,Language,Nationality,IsValid,CreatedDate,CreatedBy FROM MstCountry ORDER BY CountryId ASC";
                return await con.QueryAsync<CountryModel>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<int> DeleteCountryData(int countryId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE MstCountry SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE CountryId = @CountryId";
                return await con.ExecuteAsync(sql, new { CountryId = countryId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<string> AddUpdateCountry(CountryModel objCountry)
        {
            try
            {
                string returnValue;
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("V3M_InsertUpdate_Country", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CountryId", objCountry.CountryId);
                        cmd.Parameters.AddWithValue("@CountryCode", objCountry.CountryCode);
                        cmd.Parameters.AddWithValue("@CountryName", objCountry.CountryName);
                        cmd.Parameters.AddWithValue("@IsValid", objCountry.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", objCountry.CreatedBy);
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
                throw new Exception("Error while inserting/updating Country", ex);
            }
        }
    }
}
