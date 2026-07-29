using DomainModel.Admin;
using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface IOccupationRepository
    {
        public Task<string> AddUpdateOccupation(OccupationModal objoccuption);
        public Task<int> DeleteOccupation(int occupationId);
        public Task<IEnumerable<OccupationModal>> GetOccupation();
    }
}
