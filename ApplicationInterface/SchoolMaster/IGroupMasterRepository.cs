using DomainModel.Admin;
using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface IGroupMasterRepository
    {
        public Task<string> AddUpdateGroup(GroupMaster objgroup,string logopath);
        public Task<int> DeleteGroup(int GroupId);
        public Task<IEnumerable<GroupMaster>> GetGroupMaster();
    }
}
