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
    public class AccountNoService : IAccountNoRepository
    {
        private readonly string _connectionString;
        public AccountNoService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddUpdateAccountNo(AccountNoModel number)
        {
            try
            {
                string returnValue;
                var sqlQry = "MNGT_InsertUpdate_AccountNo";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@AccountId", number.AccountId);
                        cmd.Parameters.AddWithValue("@AccountNo", number.AccountNo);
                        cmd.Parameters.AddWithValue("@AccountDescription", number.AccountDescription);
                        cmd.Parameters.AddWithValue("@CreatedBy", number.CreatedBy);
                        cmd.Parameters.AddWithValue("@UpdatedBy", number.UpdatedBy);
                        cmd.Parameters.AddWithValue("@GroupCode", number.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", number.BranchCode);
                        cmd.Parameters.AddWithValue("@IsValid", number.IsValid);
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

        public async Task<IEnumerable<AccountNoModel>> GetAccountNoData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "select * from V3M_FIN_PT_MstAccount with(nolock)";
                return await con.QueryAsync<AccountNoModel>(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteAccountNoData(int accountId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE V3M_FIN_PT_MstAccount
                       SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE AccountId = @accountId";
                return await con.ExecuteAsync(sql, new { AccountId = accountId });
            }
            catch (Exception)
            {
                throw;
            }
        }

       
    }
  }


