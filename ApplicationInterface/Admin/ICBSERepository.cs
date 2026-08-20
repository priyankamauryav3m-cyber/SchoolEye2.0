using DomainModel.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationInterface.Admin
{
    public interface IUpdateStudentCBSERegNoRepository
    {
        public Task<IEnumerable<AdmSearchedStudentResponse>> GetStudentBoardRollNo(AdmSearchedStudentRequest request);
        //public Task<List<StudentCBSERegNoResult>> AddUpdateStudentCBSERegNo(UpdateStudentCBSERegNoRequest request);
        Task<string> AddUpdateStudentBoardRollNo(AddUpdateStudentBoardRollNoRequest request);
    }
}
