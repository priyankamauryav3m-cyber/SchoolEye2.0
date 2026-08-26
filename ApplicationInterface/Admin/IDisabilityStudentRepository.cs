using DomainModel.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationInterface.Admin
{
    public interface IDisabilityStudentRepository
    {
        public Task<IEnumerable<DisabilityStudentResponse>> GetDisabilityStudentData(DisabilityStudentRequest request);
        public Task<int> UpdateStudentDisabilityData(StudentDisabilityRequest request);
    }
}
