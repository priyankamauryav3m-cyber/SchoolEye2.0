using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using DomainModel.SchoolMaster;

namespace ApplicationInterface.SchoolMaster
{
    public interface IInteractionPanelRepository
    {

        public Task<IEnumerable<InteractionPanelModel>> GetAllAsync();

        public Task<string> AddUpdateInteractionPanel(InteractionPanelModel objPanel);

        public Task<int> DeleteInteractionPanelData(int pid);
        public Task<string> AddUpdateInteractionComments(InteractionCommentsModel model);

        public Task<EmployeeModel> GetEmployeeList(EmployeeModel emp);
        Task<InteractionCommentsModel?> GetInteractionComments(SearchAnyRequestModel request);

    }
}
