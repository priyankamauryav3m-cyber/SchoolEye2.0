using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface IClassCodeRepository
    {    
        public Task<IEnumerable<ClassCodeModel>> GetAllAsync();
        public Task<int> DeleteAsync(int classId);
        public Task<string> AddUpdateClasscode(ClassCodeModel objClasscode);
    }
}
