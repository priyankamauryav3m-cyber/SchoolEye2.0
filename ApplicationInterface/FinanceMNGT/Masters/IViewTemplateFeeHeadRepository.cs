using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.FinanceMNGT.FeeMNGTMasters
{
    public interface IViewTemplateFeeHeadRepository
    {
      public Task<IEnumerable<ClassFeeHeadsModel>> GetFeeHeadsMappedWithTemplateList(FeeHeadTemplateRequest request);
        public Task<int> DeleteFeeMapTemplateData(int feeHeadId);
        public Task<string> SaveFeeTemplateFeeHeads(ClassFeeHeadsModel request);
        public Task<IEnumerable<FeeHeadTemplatesListModel>> GetFeeHeadTemplatesList(FeeHeadTemplateRequest request);
    }
}
