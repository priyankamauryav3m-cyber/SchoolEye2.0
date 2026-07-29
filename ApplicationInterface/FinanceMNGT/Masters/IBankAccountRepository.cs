using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Text;
namespace ApplicationInterface.FinanceMNGT
{
    public interface IBankAccountRepository
    {
       public  Task<string> AddUpdateBankAccount(BankAccountModel account);
        public Task<int> DeleteBankAccountData(int detBankAcId);
        public Task<IEnumerable<BankAccountModel>> GetBankAccountData();
    }
}
