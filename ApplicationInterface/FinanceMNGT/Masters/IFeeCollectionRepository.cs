using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.FinanceMNGT.FeeMNGTMasters
{
    public interface IFeeCollectionRepository 
    {
        public Task<string> AddUpdateFeeCollection(FeeCollectionModel fee);
        public  Task<int> DeleteFeeCollectionData(int sid);
        public Task<IEnumerable<FeeCollectionModel>> GetFeeCollectionData();
    }
}
