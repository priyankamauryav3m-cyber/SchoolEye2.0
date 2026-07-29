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
    public class PaymentModeRepository : IPaymentModeRepository
    {
      
        private readonly string _connectionString;
        public PaymentModeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddUpdatePaymentMode(PaymentModel paymentMode)
        {
            try
            {
                string returnValue;
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("MNGT_InsertUpdate_PaymentMode", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PMId", paymentMode.PMId);
                        cmd.Parameters.AddWithValue("@GroupCode", paymentMode.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", paymentMode.BranchCode);
                        cmd.Parameters.AddWithValue("@DisplayOrder", paymentMode.DisplayOrder);
                        cmd.Parameters.AddWithValue("@ModeName", paymentMode.ModeName);
                        cmd.Parameters.AddWithValue("@ModeAbbr", paymentMode.ModeAbbr);
                        cmd.Parameters.AddWithValue("@CreatedBy", paymentMode.CreatedBy);
                        cmd.Parameters.AddWithValue("@IsValid", paymentMode.IsValid);
                      
                        SqlParameter outputParam = new SqlParameter
                        {
                            ParameterName = "@ReturnValue",
                            SqlDbType = SqlDbType.VarChar,
                            Size = 50,
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outputParam);

                        await cmd.ExecuteNonQueryAsync();
                        returnValue = outputParam.Value?.ToString();
                    }
                }
                return returnValue;
            }
            catch (Exception ex)
            {

                throw;
            }
        }



        public async Task<IEnumerable<PaymentModel>> GetPaymentModeData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "select * from V3M_Fin_MstPaymentMode with(nolock)";
                return await con.QueryAsync<PaymentModel>(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeletePaymentModeData(int pid)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE V3M_Fin_MstPaymentMode
                       SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE PMId = @pid";
                return await con.ExecuteAsync(sql, new { pid = pid });
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

