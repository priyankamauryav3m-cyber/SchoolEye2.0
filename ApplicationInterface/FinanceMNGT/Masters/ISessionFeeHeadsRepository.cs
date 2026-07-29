using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.FinanceMNGT.FeeMNGTMasters
{
    public interface ISessionFeeHeadsRepository
    {
       public  Task<string> AddUpdateSession(DetSessionModel session);
       public Task<int> DeleteSessionData(int sid);

        public Task<IEnumerable<DetSessionModel>> GetSessionFeeHead( SearchAnyRequestModel searchAny);
    }
}
