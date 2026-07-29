using ApplicationInterface.SuperAdmin;
using Dapper;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using DomainModel.Admin;
using Infrastructure.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Transactions;
using static DomainModel.Admin.SuperAdminDomain;

namespace Infrastructure.SuperAdmin
{
    public class SuperAdminService : ISuperAdmin
    {
        private readonly string _connectionString;
        public SuperAdminService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
            ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }      
        // Add  Module  
        public async Task<int> AddModuleData(SuperAdminModule module)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                return await connection.ExecuteScalarAsync<int>(
                    "V3M_Security_Module_Insert",
                    new
                    {
                        module.MName,
                        module.DisplayName,
                        module.Description,
                        module.DisplayOrder,
                        module.Icon,
                        module.CreatedBy
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("AddModule Error: " + ex.Message);
                return 0;
            }
        }
        public async Task<IEnumerable<SuperAdminModule>> GetAddModuleData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT * FROM Mstmodule";
                return await con.QueryAsync<SuperAdminModule>(sql);
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while fetching module data.", ex);
            }
        }

        public async Task<int> AddModuleEditData(SuperAdminModule module)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                string query = @"UPDATE MstModule SET MName = @MName,DisplayName = @DisplayName,Description = @Description,DisplayOrder = @DisplayOrder,Icon = @Icon,
                IsValid = @IsValid WHERE ModuleId = @ModuleId";
                return await connection.ExecuteAsync(query, new
                {
                    module.ModuleId,
                    module.MName,
                    module.DisplayName,
                    module.Description,
                    module.DisplayOrder,
                    module.Icon,
                    module.IsValid
                });
            }
            catch
            {
                return 0; // update failed
            }
        }
        public async Task<int> AddModuleDeleteData(int moduleId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                string query = @" UPDATE MstModule SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END WHERE ModuleId = @ModuleId";
                var parameters = new { ModuleId = moduleId };
                return await connection.ExecuteAsync(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while deleting module: " + ex.Message, ex);
            }
           
        }
        public async Task<int> AddFeaturesData(SuperAdminFeatures features)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                return await connection.ExecuteScalarAsync<int>(
                    "V3M_Security_Feature_Insert",
                    new
                    {
                        features.ModuleId,
                        features.FeaturesName,
                        features.Description,
                        features.DisplayOrder,
                        features.CreatedBy
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("AddFeatures Error: " + ex.Message);
                return 0;
            }
        }
        public async Task<IEnumerable<SuperAdminFeatures>> GetAddFeaturesData(int moduleId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var result= await connection.QueryAsync<SuperAdminFeatures>(
                    "V3M_SecurityFeatureByModule",
                    new { ModuleId = moduleId },
                    commandType: CommandType.StoredProcedure
                );
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching features", ex);
            }
        }
        public async Task<int> AddFeaturesEditData(SuperAdminFeatures features)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                string query = @" UPDATE MstFeaturesList SET FeaturesName = @FeaturesName, Description = @Description, DisplayOrder = @DisplayOrder, IsValid = @IsValid
                WHERE FeatureId = @FeatureId";
                return await connection.ExecuteAsync(query, new
                {
                    features.FeatureId,
                    features.FeaturesName,
                    features.Description,
                    features.DisplayOrder,
                    features.IsValid
                });
            }
            catch
            {
                return 0; // update failed
            }
        }
        public async Task<int> AddFeaturesDeleteData(int featureId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                string query = @" UPDATE MstFeaturesList SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END WHERE FeatureId = @FeatureId";
                var parameters = new { FeatureId = featureId };
                return await connection.ExecuteAsync(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while deleting feature: " + ex.Message, ex);
            }
        }
        public async Task<int> AddActivityData(SuperAdminActivity activity)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                return await connection.ExecuteScalarAsync<int>(
                    "V3M_Security_Activity_Insert",
                    new
                    {
                        activity.ActivityName,
                        activity.DisplayName,
                        activity.DisplayOrder,
                        activity.FeatureId,
                        activity.IsAdd,
                        activity.IsModifiy,
                        activity.IsPrint,
                        activity.IsExportToExcel,
                        activity.IsPII,
                        activity.Action1,
                        activity.Action1Desc,
                        activity.Action2,
                        activity.Action2Desc,
                        activity.Action3,
                        activity.Action3Desc,
                        activity.CreatedBy,
                        activity.URL
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("AddActivity Error: " + ex.Message);
                return 0;
            }
        }
        public async Task<IEnumerable<SuperAdminActivity>> GetAddActivityData(int featureId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var result= await connection.QueryAsync<SuperAdminActivity>(
                    "V3M_Security_Activity_GetByFeature",
                    new { FeatureId = featureId },
                    commandType: CommandType.StoredProcedure
                );
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching activities: " + ex.Message, ex);
            }
        }

        public async Task<(int insertCount, int updateCount)> AccessControlMappingData(List<ControlAccess> controls)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var jsonData = JsonConvert.SerializeObject(controls);

                var param = new DynamicParameters();
                param.Add("@JsonData", jsonData);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "V3M_Insert_AccessControlMapping_JSON",
                    param,
                    commandType: CommandType.StoredProcedure
                );         
                int insertCount = result.InsertCount;
                int updateCount = result.UpdateCount;
                return (insertCount, updateCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ControlAccess>> GetControlAccessByRole(int roleId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                string sql = @"SELECT RoleId,AccessId,ModuleId, FeatureId, ActivityId, IsAdd, IsModifiy, IsPrint, IsExportToExcel, IsPII, Action1, Action2, Action3 
                            FROM MSt_ACMapping WHERE RoleId = @RoleId";
                var result = await connection.QueryAsync<ControlAccess>(
                    sql,
                    new { RoleId = roleId }
                );
                return result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<RoleaBase>> GetRoleBasedShowRecord(int roleId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@RoleId", roleId);
                return await con.QueryAsync<RoleaBase>(
                    "V3M_AccordingtoMapRestrictedData",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<RolebaseActivity>> GetRoleBasedActivity(int roleId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@RoleId", roleId);

                return await con.QueryAsync<RolebaseActivity>(
                    "RB_RoleBasedActivity",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteAccessMappings(List<int> accessIds)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                string sql = "DELETE FROM Mst_ACMapping WHERE AccessId IN @Ids";
                await connection.ExecuteAsync(sql, new { Ids = accessIds });
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while deleting access mappings.", ex);
            }
        }
        public async Task<IEnumerable<DashboardModel>> GetDashboardData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "select DashBoardId,DashBoard,DisplayOrder,IsValid,DisplayName from MstDashboard with(nolock)";
                return await con.QueryAsync<DashboardModel>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

    }
}






