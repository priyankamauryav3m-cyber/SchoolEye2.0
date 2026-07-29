using ApplicationInterface.SchoolMaster;
using Dapper;
using DomainModel.SchoolMaster;
using DomainModel.Admin;
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
    public class RoleAdminService : IRoleAdminMaster
    {
        private readonly string _connectionString;
        public RoleAdminService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
            ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<int> AddRoleData(SuperAdminDomain role)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var result = await connection.ExecuteScalarAsync<int>(
                    "V3M_Security_Role_Insert",
                    new
                    {
                        role.RoleName,
                        role.RoleDescripation,
                        role.DisplayOrder,
                        role.DashBoardId,
                        RoleIcon = role.Icon
                    },
                    commandType: CommandType.StoredProcedure
                );
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("CreateRole Error: " + ex.Message);
                return 0;
            }
        }
        public async Task<int> Add_RoleEditData(SuperAdminDomain role)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                string query = @" UPDATE MStRoles SET RoleName = @RoleName,RoleDescripation = @RoleDescripation,DisplayOrder = @DisplayOrder,DashBoardId=@DashBoardId
                WHERE RoleId = @RoleId";
                var parameters = new
                {
                    role.RoleId,
                    role.RoleName,
                    role.RoleDescripation,
                    role.DisplayOrder,
                    role.DashBoardId
                };
                return await connection.ExecuteAsync(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while updating Role: " + ex.Message, ex);
            }
        }
        public async Task<int> Add_RoleDeleteData(int RoleId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                string query = @"UPDATE MStRoles
                         SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                         WHERE RoleId = @RoleId";
                var parameters = new { RoleId = RoleId };
                return await connection.ExecuteAsync(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while deleting Role: " + ex.Message, ex);
            }
        }

        public async Task<List<SuperAdminDomain>> GetAddRole()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var result = await connection.QueryAsync<SuperAdminDomain>(
                    "V3M_Security_Role_GetAll",
                    commandType: CommandType.StoredProcedure
                );
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching roles: " + ex.Message, ex);
            }
        }
    }
}
