using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationInterface.FinanceMNGT
{
    public interface IChequeTypeRepository
    {
       public   Task<string> AddUpdateChecktype(ChequeTypeModel checktype);
        public Task<int> DeleteChecktypeData(int sid);
        public Task<IEnumerable<ChequeTypeModel>> GetChecktypeData();

    }
}
