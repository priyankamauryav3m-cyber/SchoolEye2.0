using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface IClassScheduleRepository
    {
        //Task<bool> DuplicateExistsAsync(string classCode);

      public   Task<IEnumerable<ClassSchedule>> GetAllAsync();

      public   Task<int> InsertAsync(ClassSchedule model);

     public    Task<int> UpdateAsync(ClassSchedule model);

      public   Task<int> DeleteAsync(int sid);
    }
}
