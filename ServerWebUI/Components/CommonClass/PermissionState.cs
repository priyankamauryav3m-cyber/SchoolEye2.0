using DomainModel.Admin;

namespace ServerWebUI.Components.Common
{

    public class PermissionState
    {

        public List<SuperAdminModule> Features { get; set; } = new();
        public event Action? OnChange;
        public string RoleId { get; set; }
        public bool isAdd { get; set; }
        public bool isModifiy { get; set; }
        public bool isPrint { get; set; }
        public bool isPII { get; set; }
        public bool isExportToExcel { get; set; }
        public bool Action1 { get; set; }
        public bool Action2 { get; set; }
        public bool Action3 { get; set; }
        public List<RolebaseActivity> Activities { get; set; } = new();
        public string DashboardUrl { get; set; } = "";
        public bool HasPermission(string activityName, Func<RolebaseActivity, bool> selector)
        {
            var activity = Activities.FirstOrDefault(x => x.URL != null && activityName.Contains(x.URL.Trim(), StringComparison.OrdinalIgnoreCase));
            return activity != null && selector(activity);
        }
       
        public void SetFromRoleActivities(string roleId, List<RolebaseActivity> activities)
        {
            RoleId = roleId;
            Activities = activities;
            NotifyStateChanged();
          
        }
      
        private void NotifyStateChanged()=> OnChange?.Invoke();

        public void SetMenus(string roleId, List<RolebaseActivity> activities, List<SuperAdminModule> features)
        {
            
            RoleId = roleId;
            Activities = activities;
            Features = features;
            OnChange?.Invoke();
        }
        public List<BreadcrumbItem> GetBreadcrumb(string currentUrl)
        {
            var result = new List<BreadcrumbItem>();

            result.Add(new BreadcrumbItem
            {
                Text = "Home",
                Url = DashboardUrl
            });

            foreach (var module in Features)
            {
                foreach (var feat in module.Features)
                {
                    var activity = feat.Activites
                        .FirstOrDefault(x =>
                            !string.IsNullOrEmpty(x.URL) &&
                            currentUrl.Contains(x.URL,
                            StringComparison.OrdinalIgnoreCase));

                    if (activity != null)
                    {
                        result.Add(new BreadcrumbItem
                        {
                            Text = module.MName
                        });

                        result.Add(new BreadcrumbItem
                        {
                            Text = feat.FeaturesName
                        });

                        result.Add(new BreadcrumbItem
                        {
                            Text = activity.DisplayName,
                            Url = activity.URL,
                            IsCurrent = true
                        });

                        return result;
                    }
                }
            }

            return result;
        }
    }


}
