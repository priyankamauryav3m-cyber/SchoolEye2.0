using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.FinanceMNGT.FeeMNGTMasters
{
    public interface IFeeHeadMappedRepository
    {
        public Task<string> AddUpdateFeeheadMapped(ClassFeeHeadMappedModel mapped);
        public  Task<int> DeletefeeheadMappedData(int classFeeId);
        public Task<IEnumerable<ClassFeeHeadMappedModel>> GetfeeheadMappedData();
    }
}
