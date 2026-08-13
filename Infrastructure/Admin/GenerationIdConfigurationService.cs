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
                string sql = @"UPDATE MstIdGeneration  SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
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
        public async Task<int> AddUpdateGenerationIdConfiguration(GenerationIdConfigurationModel x)
        {
            try
            {
                using SqlConnection connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                using SqlCommand cmd = new SqlCommand("V3M_InsertUpdate_GenerationIdConfiguration", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Sid", x.Sid);
                cmd.Parameters.AddWithValue("@GroupCode", (object?)x.GroupCode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@BranchCode", (object?)x.BranchCode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@BTCID", x.BTCID.HasValue ? x.BTCID.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@BranchCodeRequired", x.BranchCodeRequired);
                cmd.Parameters.AddWithValue("@BTCRequired", x.BTCRequired);
                cmd.Parameters.AddWithValue("@SessionRequired", x.SessionRequired);
                cmd.Parameters.AddWithValue("@PatternFor", (object?)x.PatternFor ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@KeyWord", (object?)x.KeyWord ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PreFix", (object?)x.PreFix ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@KeyValue", x.KeyValue);
                cmd.Parameters.AddWithValue("@KeyValueLength", x.KeyValueLength);
                cmd.Parameters.AddWithValue("@ResetFlag", x.ResetFlag);
                cmd.Parameters.AddWithValue("@ClassGroup", (object?)x.ClassGroup ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsValid", x.IsValid);
                cmd.Parameters.AddWithValue("@CreatedBy", (object?)x.CreatedBy ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SessionId", (object?)x.SessionId ?? DBNull.Value);

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

        public async Task<IEnumerable<KeyWordModel>> GetAllKeyword()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"Select Id,KeyWord,IsValid,CreatedDate,CreatedBy From V3M_IdGenerationKeyword";
                var data = await con.QueryAsync<KeyWordModel>(sql);
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
    }
}
