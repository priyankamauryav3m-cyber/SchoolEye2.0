using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.FinanceMNGT.FeeMNGTMasters
{
    public interface IFeeTemplateRepository
    {
     public Task<string> AddUpdateFeeTemplateData(FeeTemplateModel feeTem);
     public Task<int> DeleteFeeTemplateData(int feeTemplateId);
      public  Task<IEnumerable<FeeTemplateModel>> GetFeeTemplateData();
    }
}
