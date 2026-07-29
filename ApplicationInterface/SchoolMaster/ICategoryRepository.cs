using DomainModel.SchoolMaster;

namespace ApplicationInterface.SchoolMaster
{
    public interface ICategoryRepository
    {

        public Task<IEnumerable<RTECategoryModel>> GetAllAsync();

        // public Task<int> AddUpdateCategory(CategoryModel objCategory);
        public Task<string> AddUpdateCategory(RTECategoryModel objCategory);

        public Task<int> DeleteCategoryData(int categoryId);
    }
}
