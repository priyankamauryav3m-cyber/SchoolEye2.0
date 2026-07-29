using DomainModel.Admin;
using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface IUserRightRepository
    {
        public Task<string> AddUpdateUserRights(UserRightModal objright);
        public Task<int> DeleteUserRight(int uRSID);
        public Task<IEnumerable<UserRightModal>> GetUserRight();
    }
}
