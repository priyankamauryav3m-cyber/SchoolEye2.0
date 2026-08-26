using DomainModel.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationInterface.Admin
{
    public interface IAbscondedStudentRepository
    {
        public Task<IEnumerable<GetAbscondedStudentResponse>> GetAbscondedStudent(GetAbscondedStudentRequest request);
        public Task<string> AbscondStudent(GetAbscondedStudentRequest request);

        public Task<string> UnAbscondStudent(GetAbscondedStudentRequest request);



    }
}
