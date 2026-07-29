using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.FinanceMNGT.FeeMNGTMasters
{
    public interface ISetFeeTakingMethodRepository
    {
       public  Task<IEnumerable<dynamic>> GetFeeHeadsOfTemplateData(SearchAnyRequestModel request);

        public Task<string> SaveFeeCollectionConfig(FeeTakingMethod method);
    }
}