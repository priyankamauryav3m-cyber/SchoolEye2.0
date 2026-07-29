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
    public class GroupService:IGroupMasterRepository
    {
        private readonly string _connectionString;
        public GroupService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async Task<string> AddUpdateGroup(GroupMaster objgroup,string logopath)
        {
            try
            {
                string returnValue;

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand("V3M_InsertUpdate_GroupMaster", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@GroupId", objgroup.GroupId);
                        cmd.Parameters.AddWithValue("@GroupCode", objgroup.GroupCode);
                        cmd.Parameters.AddWithValue("@GroupName", objgroup.GroupName);
                        cmd.Parameters.AddWithValue("@LogoPath", (object?)logopath ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ContactPerson", (object?)objgroup.ContactPerson ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ContactNo", (object?)objgroup.ContactNo ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ContactEmailId", (object?)objgroup.ContactEmailId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ContactPersonImagePath", (object?)objgroup.ContactPersonImagePath ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@WebSite", (object?)objgroup.WebSite ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@GroupEmailId", (object?)objgroup.EmailId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@AddressLine1", (object?)objgroup.AddressLine1 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@AddressLine2", (object?)objgroup.AddressLine2 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DistrictId", objgroup.DistrictId);
                        cmd.Parameters.AddWithValue("@StateId", objgroup.StateId);
                        cmd.Parameters.AddWithValue("@CountryId", objgroup.CountryId);
                        cmd.Parameters.AddWithValue("@PinCode", (object?)objgroup.PinCode ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsValid", objgroup.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", objgroup.CreatedBy);
                        cmd.Parameters.AddWithValue("@ResellerId", objgroup.ResellerId);
                        cmd.Parameters.AddWithValue("@LoginName", objgroup.LoginName);
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
                throw new Exception("Failed to insert/update Group", ex);
            }
        }
        public async Task<int> DeleteGroup(int groupId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE MstGroupMaster SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END 
                       WHERE GroupId = @GroupId";
                return await con.ExecuteAsync(sql, new { GroupId = groupId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<IEnumerable<GroupMaster>> GetGroupMaster()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT GroupId,GroupCode,GroupName,LogoPath,ContactPerson,ContactNo,ContactEmailId,ContactPersonImagePath,ContactPersonImagePath,WebSite,GroupEmailId,AddressLine1,AddressLine2,DistrictId,StateId,CountryId,PinCode,ResellerId,LoginName FROM MstGroupMaster";
                return await con.QueryAsync<GroupMaster>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
    }
}
