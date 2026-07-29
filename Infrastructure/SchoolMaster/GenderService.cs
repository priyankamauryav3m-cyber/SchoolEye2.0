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
    public class GenderService : IGenderRepository
    {
        private readonly string _connectionString;

        public GenderService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddUpdateGender(GenderModal objgender)
        {
            try
            {
                string returnValue;
                var sqlQry = "V3M_InsertUpdate_Gender";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Sid", objgender.Sid);
                        cmd.Parameters.AddWithValue("@GenderName", objgender.GenderName);
                        cmd.Parameters.AddWithValue("@GroupCode", objgender.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", objgender.BranchCode);
                        cmd.Parameters.AddWithValue("@IsValid", objgender.IsValid);
                        cmd.Parameters.AddWithValue("@DisplayOrder", objgender.DisplayOrder);
                        cmd.Parameters.AddWithValue("@CreatedDate", objgender.CreatedDate);
                        cmd.Parameters.AddWithValue("@CreatedBy", objgender.CreatedBy);
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

        public async Task<int> DeleteAsync(int GenderId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE MstGender
                       SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE Sid = @Sid";
                return await con.ExecuteAsync(sql, new { Sid = GenderId });
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IEnumerable<GenderModal>> GetAllAsync()
        {
            try
            {
                using var db = new SqlConnection(_connectionString);
                string sql = "SELECT Sid,GroupCode,BranchCode,GenderName,IsValid,DisplayOrder FROM MstGender";
                return await db.QueryAsync<GenderModal>(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
