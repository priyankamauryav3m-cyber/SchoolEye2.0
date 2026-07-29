using DomainModel.FinanceMNGT;
using System.Collections.Generic;
using System.Text;

namespace ApplicationInterface.FinanceMNGT
{
    public interface IFeeHeadRepository
    {
        public Task<string> AddUpdateFeeHead(FeeHeadModel feehead);
        public Task<int> DeleteFeeHeadData(int feeHeadId);
        public Task<List<FeeHeadModel>> GetFeeHeadData();
     
    }
}
