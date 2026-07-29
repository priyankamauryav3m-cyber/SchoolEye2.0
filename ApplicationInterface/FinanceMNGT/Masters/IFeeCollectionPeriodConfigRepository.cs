using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationInterface.FinanceMNGT
{
    public interface IFeeCollectionPeriodConfigRepository
    {
        public Task<string> AddUpdateFeeCollectionPeriodConfig(FeeCollectionPeriodConfig feeCollectionPeriodConfig);
        public Task<int> DeleteFeeCollectionPeriodConfigData(int fid);
        public Task<IEnumerable<FeeCollectionPeriodConfig>> GetFeeCollectionPeriodConfigData(SearchAnyRequestModel requestModel);
        public Task<int> InsertLateFeeConfigration(LateFeeConfigration request);
        public Task<IEnumerable<LateFeeConfigData>> GetLateFeeConfigListData(LateFeeConfigData requestModel);
        public Task<IEnumerable<LateFeeConfigData>> GetClassesListData(LateFeeConfigration requestModel);
        public Task<int> UpdateLateFeeDataData(LateFeeConfigData lateFee);
        public Task<int> ActivateDeactivateLateFeeConfig(ActivateModal request);
           public Task<IEnumerable<PeriodMaster>> GetPeriodType();
        public Task<IEnumerable<FeeCollectionMonthMappingModel>> GetQuarterlyMonthMapping(SearchAnyRequestModel request);





    }
}
