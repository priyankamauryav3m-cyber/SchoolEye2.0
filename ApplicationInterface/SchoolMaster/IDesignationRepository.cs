using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface IDesignationRepository
    {

      public   Task<IEnumerable<DesignationModel>> GetAllDesignationAsync();
        public Task<int> DeleteDesignationAsync(int desigId);    
        public Task<string> AddUpdateDesignation(DesignationModel objDesignation);

    }
}
