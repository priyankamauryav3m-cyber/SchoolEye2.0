using DomainModel.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationInterface.Admin
{
    public interface IBirthdayStudentRepository
    {
        public Task<IEnumerable<BirthdayStudentResponse>> GetBirthdayStudent(BirthdayStudentRequest request);
    }
}
