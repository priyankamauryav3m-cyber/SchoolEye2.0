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
    public class FeeCollectionService: IFeeCollectionRepository
    {
        private readonly string _connectionString;

        public FeeCollectionService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddUpdateFeeCollection(FeeCollectionModel fee)
        {
            try
            {
                string returnValue;
                var sqlQry = "MNGT_InsertUpdate_FeeCollection";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Sid", fee.Sid);
                        cmd.Parameters.AddWithValue("@SessionId", fee.SessionId);
                        cmd.Parameters.AddWithValue("@MonthNo", fee.MonthNo);
                        cmd.Parameters.AddWithValue("@ClassCode", fee.ClassCode);
                        cmd.Parameters.AddWithValue("@FeeHeadId", fee.FeeHeadId);
                        cmd.Parameters.AddWithValue("@Amount", fee.Amount);
                        cmd.Parameters.AddWithValue("@FeeTemplateId", fee.FeeTemplateId);
                        cmd.Parameters.AddWithValue("@GroupCode", fee.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", fee.BranchCode);
                        cmd.Parameters.AddWithValue("@IsValid", fee.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", fee.CreatedBy);
                        cmd.Parameters.AddWithValue("@CreatedDate", fee.CreatedDate);
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

        public async Task<IEnumerable<FeeCollectionModel>> GetFeeCollectionData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "select * from V3M_FIN_Det_FeeCollectionConfig with(NoLock)";
                return await con.QueryAsync<FeeCollectionModel>(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteFeeCollectionData(int sid)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE V3M_FIN_Det_FeeCollectionConfig
                       SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE Sid = @sid";
                return await con.ExecuteAsync(sql, new { Sid = sid });
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
