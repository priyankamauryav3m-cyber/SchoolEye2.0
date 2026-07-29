using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface IStudentNameRepository
    {  
        public Task<List<ShareDomain>> GetCompulsorySubjects(string groupCode, string branchCode, string streamCode);
       public Task<List<ShareDomain>> ElectiveSubjectsData(string groupCode,string branchCode,string streamCode,string groupId,string firstElement);
    }
}
