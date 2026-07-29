using DomainModel.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface IBranchClassRepository
    {
        public Task<IEnumerable<BranchClassModel>> GetBranchesAsync();
    }
}
