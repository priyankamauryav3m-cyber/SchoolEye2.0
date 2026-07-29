using DomainModel.Admin;
using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface ISessionRepository
    {
        public Task<string> AddUpdateSession(SessionModel session);
    
        public Task<int> DeleteSessionData(int sessionId);
        public Task<IEnumerable<SessionModel>> GetSessionData();
    }
}
