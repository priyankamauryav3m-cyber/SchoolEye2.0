using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface IClassRepository
    {
       
        public Task<IEnumerable<ClassModel>> GetClassData();
        public Task<int> DeleteClassData(int classId);
        public Task<string> AddUpdateClass(ClassModel objClass);
    }
}
