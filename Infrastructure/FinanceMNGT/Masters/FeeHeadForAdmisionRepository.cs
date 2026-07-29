using ApplicationInterface.FinanceMNGT;
using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.FinanceMNGT
{
    public class FeeHeadForAdmisionRepository : IFeeHeadForAdmisionRepository
    {
        public Task<string> AddUpdateFeeHeadForAdmision(FeeHeadForAdmision feeHeadForAdmision)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteFeeHeadForAdmisionData(int cid)
        {
            throw new NotImplementedException();
        }

        public Task<List<FeeHeadForAdmision>> GetFeeHeadForAdmisionData()
        {
            throw new NotImplementedException();
        }
    }
}
