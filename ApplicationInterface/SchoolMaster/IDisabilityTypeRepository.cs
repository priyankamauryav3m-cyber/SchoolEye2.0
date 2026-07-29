using DomainModel.Admin;
using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface IDisabilityTypeRepository
    {
        public Task<string> AddUpdateDisabilityType(DisabilityTypeModel objdisabilitytype);
        public Task<int> DeleteDisabilityType(int DisabilityTypeId);
        public Task<IEnumerable<DisabilityTypeModel>> GetDisabilityType();
    }
}
