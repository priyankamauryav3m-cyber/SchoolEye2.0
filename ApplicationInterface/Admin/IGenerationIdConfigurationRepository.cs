using DomainModel.Admin;
using DomainModel.SchoolMaster;

namespace ApplicationInterface.SchoolMaster
{
    public interface IGenerationIdConfigurationRepository
    {

        public Task<IEnumerable<GenerationIdConfigurationModel>> GetAllAsync(long sessionId);

        // Bulk save: the editable grid submits the whole list at once.
        public Task<int> AddUpdateGenerationIdConfiguration(List<GenerationIdConfigurationModel> objList);

        public Task<int> DeleteGenerationIdConfigurationData(int sid);
    }
}
