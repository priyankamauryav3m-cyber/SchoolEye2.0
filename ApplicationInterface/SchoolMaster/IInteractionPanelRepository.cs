using DomainModel.SchoolMaster;

namespace ApplicationInterface.SchoolMaster
{
    public interface IInteractionPanelRepository
    {

        public Task<IEnumerable<InteractionPanelModel>> GetAllAsync();

        // public Task<int> AddUpdateInteractionPanel(InteractionPanelModel objPanel);
        public Task<string> AddUpdateInteractionPanel(InteractionPanelModel objPanel);

        public Task<int> DeleteInteractionPanelData(int pid);
    }
}
