using ApplicationInterface.FinanceMNGT;
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
    public class SessionFeeHeadService : ISessionFeeHeadsRepository
    {
        private readonly string _connectionString;
        public SessionFeeHeadService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddUpdateSession(DetSessionModel session)
        {
            try
            {
                string returnValue;
                var sqlQry = "FNGT_InsertUpdate_session";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Sid", session.Sid);
                        cmd.Parameters.AddWithValue("@GroupCode", session.GroupCode);
                        cmd.Parameters.AddWithValue("@BrachCode", session.BranchCode);
                        cmd.Parameters.AddWithValue("@SessionId", session.SessionId);
                        cmd.Parameters.AddWithValue("@FeeHeadId", session.FeeHeadId);
                        cmd.Parameters.AddWithValue("@IsValid", session.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", session.CreatedBy);
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


        public async Task<IEnumerable<DetSessionModel>> GetSessionFeeHead( SearchAnyRequestModel searchAny)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                var param = new DynamicParameters();
                param.Add("@GroupCode", searchAny.GroupCode);
                param.Add("@BranchCode", searchAny.BranchCode);
                param.Add("@SessionId", searchAny.SessionId); 
                var result = await con.QueryAsync<DetSessionModel>(
                    "Get_FeeHeadSession",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> DeleteSessionData(int Sid)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                string sql = @"UPDATE DetSessionFeeHeads
                       SET IsValid = CASE WHEN IsValid = 1 THEN 0  ELSE 1  END WHERE Sid = @Sid";
                return await con.ExecuteAsync(sql, new { Sid = Sid });
            }
            catch (Exception ex)
            {
                // Optional: log the error here
                throw;
            }
        }
    }
}
