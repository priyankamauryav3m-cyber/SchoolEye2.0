using ApplicationInterface.FinanceMNGT;
using ClosedXML.Excel;
using Dapper;
using DomainModel;
using DomainModel.FinanceMNGT;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Infrastructure.FinanceMNGT
{
    public class ChequeBookService: IChequeBookRepository
    {
        private readonly string _connectionString;

        public ChequeBookService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddUpdateCheckBook(ChequeBookModel check)
        {
            try
            {
                string returnValue;
                var sqlQry = "MNGT_InsertUpdate_ChequeBook";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CheqBookId", check.CheqBookId);
                        cmd.Parameters.AddWithValue("@CheqTitle", check.CheqTitle);
                        cmd.Parameters.AddWithValue("@BankId", check.BankId);
                        cmd.Parameters.AddWithValue("@AccountId", check.AccountId);
                        cmd.Parameters.AddWithValue("@FirstLeafNo", check.FirstLeafNo);
                        cmd.Parameters.AddWithValue("@TotalLeaf", check.TotalLeaf);
                        cmd.Parameters.AddWithValue("@BookStatus", check.BookStatus);
                        cmd.Parameters.AddWithValue("@GroupCode", check.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", check.BranchCode);
                        cmd.Parameters.AddWithValue("@IsValid", check.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", check.CreatedBy);
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

        public async Task<IEnumerable<ChequeBookModel>> GetCheckBookData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "select * from V3M_FIN_MstChequeBook with(NoLock)";
                return await con.QueryAsync<ChequeBookModel>(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteFINBankData(int cheqBookId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE V3M_FIN_MstChequeBook
                       SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE CheqBookId = @cheqBookId";
                return await con.ExecuteAsync(sql, new { CheqBookId = cheqBookId });
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
