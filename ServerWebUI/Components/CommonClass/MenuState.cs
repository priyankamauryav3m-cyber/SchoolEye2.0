using DomainModel.Admin;

namespace ServerWebUI.Components.CommonClass
{
    public class MenuState
    {
        public List<SuperAdminModule> Features { get; private set; } = new();

        public string DashboardUrl { get; private set; } = "/";

        // module (MstModule.ModuleId) whose menu opened the current page; 0 = unknown
        public int ActiveModuleId { get; private set; }

        public event Action? OnChange;

        // no OnChange here: only pages read it while loading, the menu does not re-render for it
        public void SetActiveModule(int moduleId)
        {
            ActiveModuleId = moduleId;
        }

        public void SetFeatures(List<SuperAdminModule> features)
        {
            Features = features ?? new();
            OnChange?.Invoke();
        }

        public void SetDashboardUrl(string? url)
        {
            DashboardUrl = string.IsNullOrWhiteSpace(url)
                ? "/"
                : url.StartsWith("/") ? url : "/" + url;

            OnChange?.Invoke();
        }

        public void Clear()
        {
            Features = new();
            DashboardUrl = "/";
            ActiveModuleId = 0;

            OnChange?.Invoke();
        }
    }
}