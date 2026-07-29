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
    public class FeeTemplateService: IFeeTemplateRepository
    {
        private readonly string _connectionString;

        public FeeTemplateService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddUpdateFeeTemplateData(FeeTemplateModel feeTem)
        {
            try
            {
                string returnValue;
                var sqlQry = "MNGT_InsertUpdate_FeeTemplate";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@FeeTemplateId", feeTem.FeeTemplateId);
                        cmd.Parameters.AddWithValue("@TemplateName", feeTem.TemplateName);
                        cmd.Parameters.AddWithValue("@Description", feeTem.Description);
                        cmd.Parameters.AddWithValue("@DisplayOrder", feeTem.DisplayOrder);
                        cmd.Parameters.AddWithValue("@IsValid", feeTem.IsValid);                     
                        cmd.Parameters.AddWithValue("@GroupCode", feeTem.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", feeTem.BranchCode);
                        cmd.Parameters.AddWithValue("@CreatedDate", feeTem.CreatedDate);
                        cmd.Parameters.AddWithValue("@CreatedBy", feeTem.CreatedBy);
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

        public async Task<IEnumerable<FeeTemplateModel>> GetFeeTemplateData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "select * from V3M_FIN_MstFeeTemplate with(NoLock)";
                return await con.QueryAsync<FeeTemplateModel>(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<int> DeleteFeeTemplateData(int feeTemplateId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE V3M_FIN_MstFeeTemplate 
                SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END 
                WHERE FeeTemplateId = @feeTemplateId";
                return await con.ExecuteAsync(sql, new { FeeTemplateId = feeTemplateId });
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
