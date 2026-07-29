using ApplicationInterface.FinanceMNGT;
using Dapper;
using DomainModel.FinanceMNGT;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Infrastructure.FinanceMNGT
{
    public class MonthService: IMonthRepository
    {
        private readonly string _connectionString;
        public MonthService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddUpdateMonth(MonthModel month)
        {
            try
            {
                string returnValue;
                var sqlQry = "MNGT_InsertUpdate_Month";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Sid", month.Sid);
                        cmd.Parameters.AddWithValue("@MonthNo", month.MonthNo);
                        cmd.Parameters.AddWithValue("@MonthName", month.MonthName);
                        cmd.Parameters.AddWithValue("@DisplayOrder", month.DisplayOrder);
                        cmd.Parameters.AddWithValue("@GroupCode", month.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", month.BranchCode);
                        cmd.Parameters.AddWithValue("@IsValid", month.IsValid);
                        
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

        public async Task<IEnumerable<MonthModel>> GetMonthData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT Sid,MonthNo,DisplayOrder,ShortMonthName,IsValid, MonthName, 0 AS IsSelected, 0 AS IsDisabled FROM V3M_FIN_Mst_MonthDisplayOrder\r\n";
                return await con.QueryAsync<MonthModel>(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> DeleteMonthData(int Sid)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                string sql = @"UPDATE V3M_FIN_Mst_MonthDisplayOrder
                       SET IsValid = CASE WHEN IsValid = 1 THEN 0  ELSE 1  END WHERE Sid = @Sid";
                return await con.ExecuteAsync(sql, new { Sid = Sid });
            }
            catch (Exception ex)
            {
                // Optional: log the error here
                throw;
            }
        }

    }
}
