using DomainModel.Admin;
using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface IGenderRepository
    {

        public Task<IEnumerable<GenderModal>> GetAllAsync();
        public Task<int> DeleteAsync(int GenderId);
        public Task<string> AddUpdateGender(GenderModal objgender);
    }
}

