using DomainModel;
using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationInterface.FinanceMNGT
{
    public interface IChequeBookRepository
    {
        public Task<string> AddUpdateCheckBook(ChequeBookModel check);
        public Task<int> DeleteFINBankData(int cheqBookId);
        public Task<IEnumerable<ChequeBookModel>> GetCheckBookData();
  
    }
}
