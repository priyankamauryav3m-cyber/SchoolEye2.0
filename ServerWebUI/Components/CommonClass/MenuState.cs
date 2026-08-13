using DomainModel.Admin;

namespace ServerWebUI.Components.CommonClass
{
    public class MenuState
    {
        public List<SuperAdminModule> Features { get; private set; } = new();

        public string DashboardUrl { get; private set; } = "/";

        public event Action? OnChange;

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

            OnChange?.Invoke();
        }
    }
}