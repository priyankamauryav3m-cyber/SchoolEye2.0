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
    public class FeeHeadMappedService: IFeeHeadMappedRepository
    {
        private readonly string _connectionString;
        public FeeHeadMappedService(IConfiguration configuration)

        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddUpdateFeeheadMapped(ClassFeeHeadMappedModel mapped)
        {
            try
            {
                string returnValue;
                var sqlQry = "MNGT_InsertUpdate_FeeHeadMappedToClass";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ClassFeeId", mapped.ClassFeeId);
                        cmd.Parameters.AddWithValue("@SessionName", mapped.SessionName);
                        cmd.Parameters.AddWithValue("@ClassCode", mapped.ClassCode);
                        cmd.Parameters.AddWithValue("@FeeHeadId", mapped.FeeHeadId);
                        cmd.Parameters.AddWithValue("@IsStudentSpecific", mapped.IsStudentSpecific);
                        cmd.Parameters.AddWithValue("@IsEditable", mapped.IsEditable);
                        cmd.Parameters.AddWithValue("@Amount", mapped.Amount);
                        cmd.Parameters.AddWithValue("@IsValid", mapped.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", mapped.CreatedBy);
                        cmd.Parameters.AddWithValue("@AmountForOld", mapped.AmountForOld);
                        cmd.Parameters.AddWithValue("@FeeTemplateId", mapped.FeeTemplateId);
                        cmd.Parameters.AddWithValue("@CreatedDate", mapped.CreatedDate);
                        cmd.Parameters.AddWithValue("@IsClubForTutionCertificate", mapped.IsClubForTutionCertificate);
                        
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


        public async Task<IEnumerable<ClassFeeHeadMappedModel>> GetfeeheadMappedData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "select * from V3M_FIN_DetFeeHeadMappedToClass with(NoLock)";
                return await con.QueryAsync<ClassFeeHeadMappedModel>(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeletefeeheadMappedData(int classFeeId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE V3M_FIN_DetFeeHeadMappedToClass
                       SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE ClassFeeId = @classFeeId";
                return await con.ExecuteAsync(sql, new { ClassFeeId = classFeeId });
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
