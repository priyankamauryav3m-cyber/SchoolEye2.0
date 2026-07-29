using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.FinanceMNGT.FeeMNGTMasters
{
    public interface IDistanceRepository
    {
        public Task<string> AddUpdateDistanceData(DistanceModel distance);
        public Task<int> DeleteDistanceData(int distanceId);
        public Task<IEnumerable<DistanceModel>> GetDistanceData();
      
    
    }
}
