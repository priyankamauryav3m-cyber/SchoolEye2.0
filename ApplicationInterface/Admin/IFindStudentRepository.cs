using DomainModel.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationInterface.Admin
{
    public interface IFindStudentRepository
    {


        public Task<IEnumerable<FindStudentResponse>> GetSearchedStudentByDetails(FIndStudentRequest request);
    }
}
