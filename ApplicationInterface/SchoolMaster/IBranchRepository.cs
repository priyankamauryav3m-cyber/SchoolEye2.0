using DomainModel.SchoolMaster;

namespace ApplicationInterface.SchoolMaster
{
    public interface IBranchRepository
    {

        public Task<IEnumerable<BranchModel>> GetAllAsync();

        // public Task<int> AddUpdateBranchMaster(BranchModel objBranch);
        public Task<string> AddUpdateBranchMaster(BranchModel objBranch);

        public Task<int> DeleteBranchMasterData(int branchId);
    }
}
