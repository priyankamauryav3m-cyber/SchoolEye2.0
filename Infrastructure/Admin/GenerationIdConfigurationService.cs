using ApplicationInterface.SchoolMaster;
using Dapper;
using DomainModel.Admin;
using DomainModel.SchoolMaster;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.SchoolMaster
{
    public class GenerationIdConfigurationService : IGenerationIdConfigurationRepository
    {
        private readonly string _connectionString;

        public GenerationIdConfigurationService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<GenerationIdConfigurationModel>> GetAllAsync(long sessionId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"SELECT Sid,GroupCode,BranchCode,BTCID,BranchCodeRequired,BTCRequired,SessionRequired,
                       PatternFor,KeyWord,PreFix,KeyValue,KeyValueLength,ResetFlag,ClassGroup,
                       IsValid,CreatedDate,CreatedBy,SessionId
                       FROM MstIdGeneration
                       WHERE (SessionId = @SessionId)
                       ORDER BY Sid ASC";
                return await con.QueryAsync<GenerationIdConfigurationModel>(sql, new { SessionId = sessionId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<int> DeleteGenerationIdConfigurationData(int sid)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE MstGenerationIdConfiguration SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE Sid = @Sid";
                return await con.ExecuteAsync(sql, new { Sid = sid });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        // Loops through every row submitted by the editable grid and upserts
        // each one via the stored procedure, collecting the per-row '0'/'1'/'2' result.
        //public async Task<List<string>> AddUpdateGenerationIdConfiguration(List<GenerationIdConfigurationModel> objList)
        //{
        //    var results = new List<string>();
        //    try
        //    {
        //        using (var connection = new SqlConnection(_connectionString))
        //        {
        //            await connection.OpenAsync();
        //            foreach (var objConfig in objList)
        //            {
        //                using (SqlCommand cmd = new SqlCommand("V3M_InsertUpdate_GenerationIdConfiguration", connection))
        //                {
        //                    cmd.CommandType = CommandType.StoredProcedure;
        //                    cmd.Parameters.AddWithValue("@Sid", objConfig.Sid);
        //                    cmd.Parameters.AddWithValue("@GroupCode", objConfig.GroupCode);
        //                    cmd.Parameters.AddWithValue("@BranchCode", objConfig.BranchCode);
        //                    cmd.Parameters.AddWithValue("@BTCID", (object)objConfig.BTCID ?? DBNull.Value);
        //                    cmd.Parameters.AddWithValue("@BranchCodeRequired", objConfig.BranchCodeRequired);
        //                    cmd.Parameters.AddWithValue("@BTCRequired", objConfig.BTCRequired);
        //                    cmd.Parameters.AddWithValue("@SessionRequired", objConfig.SessionRequired);
        //                    cmd.Parameters.AddWithValue("@PatternFor", (object)objConfig.PatternFor ?? DBNull.Value);
        //                    cmd.Parameters.AddWithValue("@KeyWord", objConfig.KeyWord);
        //                    cmd.Parameters.AddWithValue("@PreFix", objConfig.PreFix);
        //                    cmd.Parameters.AddWithValue("@KeyValue", objConfig.KeyValue);
        //                    cmd.Parameters.AddWithValue("@KeyValueLength", objConfig.KeyValueLength);
        //                    cmd.Parameters.AddWithValue("@ResetFlag", objConfig.ResetFlag);
        //                    cmd.Parameters.AddWithValue("@ClassGroup", (object)objConfig.ClassGroup ?? DBNull.Value);
        //                    cmd.Parameters.AddWithValue("@IsValid", objConfig.IsValid);
        //                    cmd.Parameters.AddWithValue("@CreatedBy", objConfig.CreatedBy);
        //                    cmd.Parameters.AddWithValue("@SessionId", (object)objConfig.SessionId ?? DBNull.Value);
        //                    SqlParameter returnValueParam = new SqlParameter
        //                    {
        //                        ParameterName = "@ReturnValue",
        //                        SqlDbType = SqlDbType.VarChar,
        //                        Size = 50,
        //                        Direction = ParameterDirection.Output
        //                    };
        //                    cmd.Parameters.Add(returnValueParam);
        //                    await cmd.ExecuteNonQueryAsync();
        //                    results.Add(returnValueParam.Value?.ToString() ?? "-1");
        //                }
        //            }
        //        }
        //        return results;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error while inserting/updating Generation Id Configuration", ex);
        //    }
        //}

        public async Task<int> AddUpdateGenerationIdConfiguration(List<GenerationIdConfigurationModel> objList)
        {
            try
            {
                DataTable dt = new DataTable();

                dt.Columns.Add("Sid", typeof(int));
                dt.Columns.Add("GroupCode", typeof(string));
                dt.Columns.Add("BranchCode", typeof(string));
                dt.Columns.Add("BTCID", typeof(int));
                dt.Columns.Add("BranchCodeRequired", typeof(bool));
                dt.Columns.Add("BTCRequired", typeof(bool));
                dt.Columns.Add("SessionRequired", typeof(bool));
                dt.Columns.Add("PatternFor", typeof(string));
                dt.Columns.Add("KeyWord", typeof(string));
                dt.Columns.Add("PreFix", typeof(string));
                dt.Columns.Add("KeyValue", typeof(int));
                dt.Columns.Add("KeyValueLength", typeof(int));
                dt.Columns.Add("ResetFlag", typeof(bool));
                dt.Columns.Add("ClassGroup", typeof(string));
                dt.Columns.Add("IsValid", typeof(bool));
                dt.Columns.Add("CreatedBy", typeof(string));
                dt.Columns.Add("SessionId", typeof(long));
                objList.ForEach(x =>
                {
                    dt.Rows.Add(
                        x.Sid,
                        x.GroupCode,
                        x.BranchCode,
                        x.BTCID.HasValue ? x.BTCID.Value : DBNull.Value,
                        x.BranchCodeRequired,
                        x.BTCRequired,
                        x.SessionRequired,
                        (object?)x.PatternFor ?? DBNull.Value,
                        x.KeyWord,
                        x.PreFix,
                        x.KeyValue,
                        x.KeyValueLength,
                        x.ResetFlag,
                        (object?)x.ClassGroup ?? DBNull.Value,
                        x.IsValid,
                        x.CreatedBy,
                        (object?)x.SessionId ?? DBNull.Value
                    );
                });

                using SqlConnection connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                using SqlCommand cmd = new SqlCommand("V3M_InsertUpdate_GenerationIdConfiguration", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlParameter tvp = cmd.Parameters.AddWithValue("@Data", dt);
                tvp.SqlDbType = SqlDbType.Structured;
                tvp.TypeName = "dbo.GenerationIdConfigurationType";
           
                SqlParameter output = new("@ReturnValue", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                cmd.Parameters.Add(output);

                await cmd.ExecuteNonQueryAsync();

                return Convert.ToInt32(output.Value);
         
            }
            catch (Exception ex)
            {
                throw new Exception("Error while inserting/updating Generation Id Configuration.", ex);
            }
        }

   
    }
}
