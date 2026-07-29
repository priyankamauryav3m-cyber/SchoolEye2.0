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
    public class TransportFeeConfigRepository : ITransportFeeConfigRepository
    {
        private readonly string _connectionString;
        public TransportFeeConfigRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddOrUpdateTransportData(TransportFeeConfig transport)
        {
            try
            {
                string returnValue;
                var sqlQry = "MNGT_InsertUpdate_TransportFee";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;  
                        cmd.Parameters.AddWithValue("@Tid", transport.Tid);
                        cmd.Parameters.AddWithValue("@SessionId", transport.SessionId);
                        cmd.Parameters.AddWithValue("@Amount", transport.Amount);
                        cmd.Parameters.AddWithValue("@DistanceId", transport.DistanceId);
                        cmd.Parameters.AddWithValue("@IsValid", transport.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", transport.CreatedBy);

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


        public async Task<IEnumerable<TransportFeeConfig>> GetTransporData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT d.DistanceId,f.IsValid, f.Tid, d.DistanceName,  f.Amount,f.SessionId FROM TPT_MstDistance d WITH(NOLOCK) INNER JOIN V3M_FIN_MstTransportFeeConfig f WITH(NOLOCK) ON d.DistanceId = f.DistanceId";
                return await con.QueryAsync<TransportFeeConfig>(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IEnumerable<TransportFeeConfig>> GetDistanceMapAmount(long SessionId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                string sql = @"SELECT  d.DistanceId,f.IsValid, d.DistanceName,f.Amount FROM TPT_MstDistance d WITH(NOLOCK) INNER JOIN V3M_FIN_MstTransportFeeConfig f WITH(NOLOCK) ON d.DistanceId = f.DistanceId WHERE f.SessionId= @SessionId";

                return await con.QueryAsync<TransportFeeConfig>(sql, new { SessionId = SessionId });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> DeleteTransportData(int tid)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE V3M_FIN_MstTransportFeeConfig
                       SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE Tid = @tid";
                return await con.ExecuteAsync(sql, new { Tid = tid });
            }
            catch (Exception)
            {
                throw;
            }
        }
       

    }
}
