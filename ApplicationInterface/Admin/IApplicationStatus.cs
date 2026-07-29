using DomainModel.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SuperAdmin
{
    public interface IApplicationStatus
    {
        public Task<IEnumerable<RegistrationStatusModel>> GetRegistrationStatus(string groupCode, string branchCode, string registrationNo, string sessionName);
    }
}
