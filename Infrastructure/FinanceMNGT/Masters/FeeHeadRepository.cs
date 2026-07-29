using Dapper;
using DomainModel.FinanceMNGT;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ApplicationInterface.FinanceMNGT
{
    public class FeeHeadRepository : IFeeHeadRepository
    {
        private readonly string _connectionString;
        public FeeHeadRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddUpdateFeeHead(FeeHeadModel feehead)
        {
            try
            {
                string returnValue;
                var sqlQry = "MNGT_InsertUpdate_FeeHead";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@FeeHeadName", feehead.FeeHeadName);
                        cmd.Parameters.AddWithValue("@FeeHeadId", feehead.FeeHeadId);
                        cmd.Parameters.AddWithValue("@FeeHeadAbbr", feehead.FeeHeadAbbr);
                        cmd.Parameters.AddWithValue("@GroupCode", feehead.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", feehead.BranchCode);
                        cmd.Parameters.AddWithValue("@FeeHeadType", feehead.FeeHeadType);
                        cmd.Parameters.AddWithValue("@FeeApplicableType", feehead.FeeApplicableType);
                        cmd.Parameters.AddWithValue("@DisplayOrder", feehead.DisplayOrder);
                        cmd.Parameters.AddWithValue("@CreatedBy", feehead.CreatedBy);
                        cmd.Parameters.AddWithValue("@IsValid", feehead.IsValid);
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

        public async Task<int> DeleteFeeHeadData(int feeHeadId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE V3M_FIN_MstFeeHead
                       SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE FeeHeadId = @FeeHeadId";
                return await con.ExecuteAsync(sql, new { FeeHeadId = feeHeadId });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<FeeHeadModel>> GetFeeHeadData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "select * from V3M_FIN_MstFeeHead with(nolock)";
                var result = await con.QueryAsync<FeeHeadModel>(sql);
                return result.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
       
    }
}
