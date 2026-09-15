using DomainModel.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationInterface.Admin
{
    public interface IRegistrationDemoRepository
    {
        Task<int> AddRegistrationDemo(RegistrationDemoModel model);
        Task<RegistrationDemoModel?> GetRegistrationDemoByRegiNo(string groupCode, string branchCode, long sessionId, int regiNo);
        Task<IEnumerable<RegistrationDemoModel>> GetRegistrationDemoList(string groupCode, string branchCode, long sessionId);
    }
}
