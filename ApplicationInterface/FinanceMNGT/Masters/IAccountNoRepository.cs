using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationInterface.FinanceMNGT
{
    public interface IAccountNoRepository
    {
        public Task<string> AddUpdateAccountNo(AccountNoModel number);
        public Task<int> DeleteAccountNoData(int accountId);
        public Task<IEnumerable<AccountNoModel>> GetAccountNoData();
    }
}
