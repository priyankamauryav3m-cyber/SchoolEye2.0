using DomainModel.FinanceMNGT;
using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationInterface.FinanceMNGT
{   
    public interface IFINBankRepository
    {
        public Task<string> AddUpdateFINBank(BankModel bank);
        public Task<int> DeleteFINBankData(int bankId);
        public Task<IEnumerable<BankModel>> GetBankData();
      
    }
}
