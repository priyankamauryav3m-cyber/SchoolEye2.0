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
    public class SessionService: ISessionRepository
    {
        private readonly string _connectionString;
        public SessionService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddUpdateSession(SessionModel session)
        {
            try
            {
                string returnValue;
                var sqlQry = "V3M_InsertUpdate_Session";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@SessionId", session.SessionId);
                        cmd.Parameters.AddWithValue("@SessionName", session.Session);
                        cmd.Parameters.AddWithValue("@Remarks",(object?) session.Remarks ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@GroupCode", session.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", session.BranchCode);
                        cmd.Parameters.AddWithValue("@CurrentSession", session.CurrentSession);
                        cmd.Parameters.AddWithValue("@AdmissionSession", session.AdmissionSession);
                        cmd.Parameters.AddWithValue("@IsValid", session.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", session.CreatedBy);
                        cmd.Parameters.AddWithValue("@CreatedDate", session.CreatedDate);
                        cmd.Parameters.AddWithValue("@StartDate", session.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", session.EndDate);
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
        public async Task<IEnumerable<SessionModel>> GetSessionData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                return await con.QueryAsync<SessionModel>(
                    "Usp_GetSessionData",
                    commandType: CommandType.StoredProcedure
                );
            }
            catch(Exception e)
            {
                return Enumerable.Empty<SessionModel>();
            }
        }

        public async Task<int> DeleteSessionData(int sessionId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE MstBranchSession
                       SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE SessionId = @SessionId";
                return await con.ExecuteAsync(sql, new { SessionId = sessionId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
    }
}
