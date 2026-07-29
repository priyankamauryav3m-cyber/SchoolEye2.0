using DomainModel.Admin;
using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface IReligionRepository
    {
        public Task<string> AddUpdateReligion(ReligionMaster objreligion);
        public Task<int> DeleteReligion(int ReligionId);
        public Task<IEnumerable<ReligionMaster>> GetReligion();
    }
}
