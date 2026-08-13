using DomainModel.Admin;
using DomainModel.SchoolMaster;

namespace ApplicationInterface.SchoolMaster
{
    public interface IGenerationIdConfigurationRepository
    {

        public Task<IEnumerable<GenerationIdConfigurationModel>> GetAllAsync(long sessionId);
        public Task<IEnumerable<KeyWordModel>> GetAllKeyword();

        // Bulk save: the editable grid submits the whole list at once.
        public Task<int> AddUpdateGenerationIdConfiguration(GenerationIdConfigurationModel objList);

        public Task<int> DeleteGenerationIdConfigurationData(int sid);
    }
}
