using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface IPAddressRepository
    {
        public Task<string> AddUpdateIPAddress(AllowedIPModel objip);
    }
}
