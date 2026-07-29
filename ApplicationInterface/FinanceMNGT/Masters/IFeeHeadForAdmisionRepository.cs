using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationInterface.FinanceMNGT
{
    public interface IFeeHeadForAdmisionRepository
    {
        public Task<string> AddUpdateFeeHeadForAdmision(FeeHeadForAdmision feeHeadForAdmision);
        public Task<int> DeleteFeeHeadForAdmisionData(int cid);
        public Task<List<FeeHeadForAdmision>> GetFeeHeadForAdmisionData();
    }
}
