using DomainModel.Admin;
using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface ISubjectCodeRepository
    {
        public Task<string> AddUpdateSubjectCode(SubjectCodeMaster objSubject);

        public Task<int> DeleteSubjectCode(int Sid);
        public Task<IEnumerable<SubjectCodeMaster>> GetSubjectCode();
    }
}
