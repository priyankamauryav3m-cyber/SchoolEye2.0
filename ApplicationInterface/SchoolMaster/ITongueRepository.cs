using DomainModel.SchoolMaster;

namespace ApplicationInterface.SchoolMaster
{
    public interface ITongueRepository
    {

        public Task<IEnumerable<TongueModel>> GetAllAsync();

        // public Task<int> AddUpdateTongue(TongueModel objTongue);
        public Task<string> AddUpdateTongue(TongueModel objTongue);

        public Task<int> DeleteTongueData(int tongueId);
    }
}
