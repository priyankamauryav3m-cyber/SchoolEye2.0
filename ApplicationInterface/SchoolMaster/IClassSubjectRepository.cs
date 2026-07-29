using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface IClassSubjectRepository
    {

        public Task<IEnumerable<ClassSubjectModel>> GetAllAsync();
        public Task<int> DeleteAsync(int mapId);
        public Task<string> AddUpdateClassSubject(ClassSubjectModel model);
    }
}
