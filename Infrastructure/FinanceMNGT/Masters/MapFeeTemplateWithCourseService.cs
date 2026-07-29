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
    public class MapFeeTemplateWithCourseService: IMapFeeTemplateWithCourseRepository
    {
        private readonly string _connectionString;
        public MapFeeTemplateWithCourseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<ClassWiseFeeTemplateModel>> GetClassWiseFeeTemplate(SearchAnyRequestModel request)
        {
            using var con = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@GroupCode", request.GroupCode);
            parameters.Add("@BranchCode", request.BranchCode);
            parameters.Add("@SessionId", request.SessionId);
            parameters.Add("@ClassCode",  request.RequestName);
            parameters.Add("@TemplateId", request.RequestId);
            var result = await con.QueryAsync<ClassWiseFeeTemplateModel>("V3M_FIN_GetDefaultFeeTemplateWithClass",
                parameters, commandType: CommandType.StoredProcedure );

            return result;
        }

        public async Task<string> SaveOrUpdateClasswiseFeeTemplateData(ClassWiseFeeTemplateModel request)
        {
            try
            {
                string returnValue;
                var sqlQry = "Fin_V3M_USPMapUnmapDefaultFeeTemplateWithCourse";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@SeedId", request.SeedId);
                        cmd.Parameters.AddWithValue("@GroupCode", request.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", request.BranchCode);
                        cmd.Parameters.AddWithValue("@SessionId", request.SessionId);
                        cmd.Parameters.AddWithValue("@ClassCode", request.ClassCode);
                        cmd.Parameters.AddWithValue("@FeeTemplateId", request.DefaultFeeTemplateId);
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
    }

}
