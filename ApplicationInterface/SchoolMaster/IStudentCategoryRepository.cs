using DomainModel.SchoolMaster;

namespace ApplicationInterface.SchoolMaster
{
    public interface IStudentCategoryRepository
    {

        public Task<IEnumerable<StudentCategoryModel>> GetAllAsync();

        // public Task<int> AddUpdateStudentCategory(StudentCategoryModel objCategory);
        public Task<string> AddUpdateStudentCategory(StudentCategoryModel objCategory);

        public Task<int> DeleteStudentCategoryData(int categoryId);
    }
}
