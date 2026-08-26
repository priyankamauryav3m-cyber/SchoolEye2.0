using DomainModel.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationInterface.Admin
{
    public interface IStudentRTERepository
    {
        public Task<IEnumerable<StudentRTEResponse>> GetSearchedStudentRTE(StudentRTERequest request);
        

        public Task<int> AddUpdateRTEStudentData(RTEStudentDataRequest request);
    }
}
