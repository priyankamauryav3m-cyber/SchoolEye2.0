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
    public class UsersRightsService : IUserRightRepository
    {
        private readonly string _connectionString;
        public UsersRightsService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddUpdateUserRights(UserRightModal objright)
        {
            try
            {
                string returnValue;
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand("V3M_InsertUpdate_UsersRights", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@URSID", objright.URSID);
                        cmd.Parameters.AddWithValue("@UserName", objright.UserName);
                        cmd.Parameters.AddWithValue("@RoleId", objright.RoleId);
                        cmd.Parameters.AddWithValue("@CreatedDate", objright.CreatedDate);
                        cmd.Parameters.AddWithValue("@CreatedBy", objright.CreatedBy);
                        SqlParameter returnValueParam = new SqlParameter("@ReturnValue", SqlDbType.VarChar, 50)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(returnValueParam);
                        await cmd.ExecuteNonQueryAsync();
                        returnValue = returnValueParam.Value?.ToString() ?? string.Empty;
                    }
                }
                return returnValue;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to insert/update Subject", ex);
            }
        }

        public async Task<int> DeleteUserRight(int ursId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"DELETE FROM MstUsersRights WHERE URSID = @URSID";
                return await con.ExecuteAsync(sql, new { URSID = ursId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }


        public async Task<IEnumerable<UserRightModal>> GetUserRight()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT URSID,UserName,RoleId,IsValid FROM MstUsersRights with(nolock)";
                return await con.QueryAsync<UserRightModal>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

    }
}
