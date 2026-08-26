using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using System.Threading.Tasks;

namespace ApplicationInterface.Admin
{
    public interface IAdmissionDateRepository
    {
      public Task<string> UpdateStudentAdmissionDate(UpdateStudentAdmissionDateRequest request);

        public Task<IEnumerable<StuSearchedStudentResponse>> GetSearchedStudent(StuSearchedStudentRequest request);

        public Task<IEnumerable<ClassRegistrationDocumentsResponse>> GetClassRegistrationDocumentsAsync(ClassRegistrationDocumentsRequest request);
        Task<ClassSectionDetailResponse> GetClassSectionDetail(StuSearchedStudentRequest request);
    }
}
