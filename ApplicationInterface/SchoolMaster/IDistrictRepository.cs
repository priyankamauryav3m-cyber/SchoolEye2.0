using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface IDistrictRepository
    {
   
        public Task<IEnumerable<DistrictModel>> GetAllAsync();  
        public Task<int> DeleteAsync(int districtId);
        public Task<string> AddUpdateDistrict(DistrictModel objDistrict);
    }
}
