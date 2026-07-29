using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationInterface.FinanceMNGT
{
    public interface ISocietyRepository
    {
        public Task<string> AddUpdateSociety(SocietyModel society);
        public Task<int> DeleteSociety(int sid);
        public Task<IEnumerable<SocietyModel>> GetSociety();
    }
}
