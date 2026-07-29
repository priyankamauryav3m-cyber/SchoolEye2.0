using DomainModel.Admin;
using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface IHolidayRepository
    {
       public  Task<IEnumerable<HolidayModal>> GetAllHoliday();
       public  Task<string> AddUpdateHoliday(HolidayModal objholidayModal);
        public Task<int> DeleteHoliday(int Id);
    }
}
