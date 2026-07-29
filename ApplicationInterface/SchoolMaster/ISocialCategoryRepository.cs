using DomainModel.SchoolMaster;

namespace ApplicationInterface.SchoolMaster
{
    public interface ISocialCategoryRepository
    {

        public Task<IEnumerable<SocialCategoryModel>> GetAllAsync();

        // public Task<int> AddUpdateSocialCategory(SocialCategoryModel objCategory);
        public Task<string> AddUpdateSocialCategory(SocialCategoryModel objCategory);

        public Task<int> DeleteSocialCategoryData(int categoryId);
    }
}
