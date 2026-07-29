using DomainModel.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DomainModel.Admin.SuperAdminDomain;

namespace ApplicationInterface.SuperAdmin
{
    public interface ISuperAdmin
    {
        public Task<List<ControlAccess>> GetControlAccessByRole(int roleId);
        public Task<(int insertCount, int updateCount)> AccessControlMappingData(List<ControlAccess> control);
        public Task<int> AddActivityData(SuperAdminActivity activity);
        public Task<int> AddFeaturesData(SuperAdminFeatures features);
        public Task<int> AddFeaturesDeleteData(int features);
        public Task<int> AddFeaturesEditData(SuperAdminFeatures features);
        public Task<int> AddModuleData(SuperAdminModule module);
        public Task<int> AddModuleDeleteData(int moduleId);
        public Task<int> AddModuleEditData(SuperAdminModule module);
        public Task<IEnumerable<RoleaBase>> GetRoleBasedShowRecord(int roleId);
        public Task<IEnumerable<SuperAdminActivity>> GetAddActivityData(int featureId);
        public Task<IEnumerable<SuperAdminFeatures>> GetAddFeaturesData(int moduleId);
        public Task<IEnumerable<SuperAdminModule>> GetAddModuleData();
        public Task<IEnumerable<RolebaseActivity>> GetRoleBasedActivity(int roleId);
        public Task DeleteAccessMappings(List<int> accessIds);
        public Task<IEnumerable<DashboardModel>> GetDashboardData();
    }
}
