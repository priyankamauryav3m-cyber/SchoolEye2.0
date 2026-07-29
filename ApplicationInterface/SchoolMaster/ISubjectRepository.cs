using DomainModel.Admin;
using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface ISubjectRepository
    {
        public Task<string> AddUpdateSubject(SubjectModel objsubject);
        public Task<int> DeleteSubject(int SubjectId);
        public Task<IEnumerable<SubjectModel>> GetSubject();
    }
}
