using ApplicationInterface.FinanceMNGT;
using Dapper;
using DomainModel.FinanceMNGT;
using DomainModel.SchoolMaster;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Infrastructure.FinanceMNGT
{
    public  class FINBankService: IFINBankRepository
    {

        private readonly string _connectionString;

        public FINBankService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddUpdateFINBank(BankModel bank)
        {
            try
            {
                string returnValue;
                var sqlQry = "MNGT_InsertUpdate_Bank";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@BankId", bank.BankId);
                        cmd.Parameters.AddWithValue("@BankName", bank.BankName);
                        cmd.Parameters.AddWithValue("@BranchName", bank.BranchName);
                        cmd.Parameters.AddWithValue("@BankAddress", bank.BankAddress);
                        cmd.Parameters.AddWithValue("@GroupCode", bank.GroupCode);
                        cmd.Parameters.AddWithValue("@IsValid", bank.IsValid);
                        //cmd.Parameters.AddWithValue("@CreatedDate", bank.CreatedDate);
                        cmd.Parameters.AddWithValue("@CreatedBy", bank.CreatedBy);
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

        public async Task<IEnumerable<BankModel>> GetBankData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "select * from V3M_FIN_MstBank";
                return await con.QueryAsync<BankModel>(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> DeleteFINBankData(int bankId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE V3M_FIN_MstBank
                       SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE BankId = @bankId";

                return await con.ExecuteAsync(sql, new { BankId = bankId });
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
