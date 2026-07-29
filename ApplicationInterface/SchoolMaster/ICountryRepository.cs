using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface ICountryRepository
    {
       
        public Task<IEnumerable<CountryModel>> GetAllAsync();
    
       // public Task<int> AddUpdateCountry(CountryModel objCountry);
        public Task<string> AddUpdateCountry(CountryModel objCountry);

        public  Task<int> DeleteCountryData(int countryId);
    }
}
