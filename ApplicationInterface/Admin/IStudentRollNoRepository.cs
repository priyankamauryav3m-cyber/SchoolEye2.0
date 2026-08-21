using DomainModel.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.Admin
{
    public interface IStudentRollNoRepository
    {
        public Task<IEnumerable<AdmSearchedStudentResponse>> GetSearchedStudentRollNo(AdmSearchedStudentRequest request);
        public Task<int> ViewStudentRollNoPreference(MapStudentRollNoRequest request);
        public Task<string?> AllocateSection(AllocateSectionRequest request);

    }
}
