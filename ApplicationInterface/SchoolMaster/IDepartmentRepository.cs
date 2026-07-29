using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface IDepartmentRepository
    {

        public Task<IEnumerable<DepartmentModel>> GetAllDepartmentAsync();

        public Task<string> AddUpdateDepartment(DepartmentModel objdepartment);

        public Task<int> DeleteDepartmentAsync(int deprtId);
    }
}
