using DomainModel.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationInterface.Admin
{
    public interface IStudentImageRepository
    {
        public Task<IEnumerable<StudentImageResponse>> GetStudentImageData(StudentImageRequest request);
    }
}
