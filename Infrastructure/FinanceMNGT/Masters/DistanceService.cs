using ApplicationInterface.FinanceMNGT.FeeMNGTMasters;
using Dapper;
using DomainModel.FinanceMNGT;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.FinanceMNGT.FeeMNGTMasters
{
    public  class DistanceService: IDistanceRepository
    {
        private readonly string _connectionString;
        public DistanceService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddUpdateDistanceData(DistanceModel distance)
        {
            try
            {
                string returnValue;
                var sqlQry = "MNGT_InsertUpdate_Distance";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@DistanceId", distance.DistanceId);
                        cmd.Parameters.AddWithValue("@DistanceName", distance.DistanceName);
                        cmd.Parameters.AddWithValue("@DistanceOrder", distance.DistanceOrder);              
                        cmd.Parameters.AddWithValue("@GroupCode", distance.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", distance.BranchCode);
                        cmd.Parameters.AddWithValue("@IsValid", distance.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", distance.CreatedBy);
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
        public async Task<IEnumerable<DistanceModel>> GetDistanceData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "select * from TPT_MstDistance with(NoLock)";
                return await con.QueryAsync<DistanceModel>(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteDistanceData(int distanceId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE TPT_MstDistance
                       SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE DistanceId = @distanceId";
                return await con.ExecuteAsync(sql, new { DistanceId = distanceId });
            }
            catch (Exception)
            {
                throw;
            }
        }

       
    }
}
