using ApplicationInterface.FinanceMNGT.FeeMNGTMasters;
using Dapper;
using DomainModel.Admin;
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
    public class ViewTemplateFeeHeadService: IViewTemplateFeeHeadRepository
    {
        private readonly string _connectionString;
        public ViewTemplateFeeHeadService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<ClassFeeHeadsModel>> GetFeeHeadsMappedWithTemplateList(FeeHeadTemplateRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", request.GroupCode);
                parameters.Add("@BranchCode", request.BranchCode);
                parameters.Add("@SessionId", request.SessionId);
                parameters.Add("@FeeHeadType", request.FeeHeadType);
                parameters.Add("@FeeHeadId", request.FeeHeadId);
                parameters.Add("@TemplateId", request.TemplateId);
                parameters.Add("@IsValid", request.Status);

                var result = await con.QueryAsync<ClassFeeHeadsModel>(
                    "V3M_FIN_GetFeeHeadsMappedWithTemplateList",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );  
                return result;

            }
            catch
            {
                return Enumerable.Empty<ClassFeeHeadsModel>();
            }
           
        }
        public async Task<string> SaveFeeTemplateFeeHeads(ClassFeeHeadsModel request)
        {
            try
            {
                string returnValue;
                var sqlQry = "V3M_UspAddEditFeeTemplateFeeHead";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ClassFeeId", request.TemplateFeeId);
                        cmd.Parameters.AddWithValue("@SessionId", request.SessionId);
                        cmd.Parameters.AddWithValue("@TemplateId", request.FeeTemplateID);
                        cmd.Parameters.AddWithValue("@FeeHeadId", request.FeeHeadId);
                        cmd.Parameters.AddWithValue("@Amount", request.Amount);
                        cmd.Parameters.AddWithValue("@CreatedBy", request.CreatedBy);
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
        public async Task<int> DeleteFeeMapTemplateData(int feeHeadId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE V3M_FIN_DetFeeHeadMappedWithTemplate
                SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                WHERE TemplateFeeId = @FeeHeadId";
                return await con.ExecuteAsync(sql, new { FeeHeadId = feeHeadId });
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IEnumerable<FeeHeadTemplatesListModel>> GetFeeHeadTemplatesList(FeeHeadTemplateRequest request)
        {
            using var con = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@GroupCode", request.GroupCode);
            parameters.Add("@BranchCode", request.BranchCode);
            parameters.Add("@SessionId", request.SessionId);
            parameters.Add("@FeeHeadId", request.FeeHeadId);
            var result = await con.QueryAsync<FeeHeadTemplatesListModel>(
                "V3M_FIN_GetFeeHeadTemplatesList",
                parameters,
                commandType: CommandType.StoredProcedure
            );
            return result;
        }

    }
}
