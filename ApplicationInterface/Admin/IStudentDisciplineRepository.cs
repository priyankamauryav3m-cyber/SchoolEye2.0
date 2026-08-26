using DomainModel.Admin;
using System.Threading.Tasks;

namespace ApplicationInterface.Admin
{
    public interface IStudentDisciplineRepository
    {
        public Task<string> AddStudentDiscipline(AddStudentDisciplineRequest request);
        public Task<IEnumerable<IndisciplineCommentResponse>> GetIndisciplineComment(IndisciplineCommentRequest request);
        public Task<IEnumerable<SiblingStudentResponse>> GetSearchedSiblingStudent(SiblingStudentRequest request);
        public Task<IEnumerable<SiblingStudentResponse>> GetSearchedStudent(SiblingStudentRequest request);
        public Task<int> EnableStudentDiscipline(IndisciplineCommentResponse request);
        public Task<int> DisbleStudentDiscipline(IndisciplineCommentResponse request);


    }
}
