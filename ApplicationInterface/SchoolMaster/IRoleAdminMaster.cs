using DomainModel.SchoolMaster;
using DomainModel.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface IRoleAdminMaster
    {
       public  Task<int> AddRoleData(SuperAdminDomain role);
        public Task<int> Add_RoleDeleteData(int RoleId);
        public Task<int> Add_RoleEditData(SuperAdminDomain role);
        public Task<List<SuperAdminDomain>> GetAddRole();
    }
}
