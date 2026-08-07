using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface IStudentService
    {
        Task<string> AddUpdateStudent(StudentModel objStudent);
        Task<List<StudentModel>> GetStudent(int studentId = 0);
        Task<bool> DeleteStudent(int studentId, bool isActive);
    }
}
