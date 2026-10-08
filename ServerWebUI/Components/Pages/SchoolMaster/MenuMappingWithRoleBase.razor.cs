using DomainModel.Admin;
using Microsoft.AspNetCore.Components;
using MyApp.Common;

namespace ServerWebUI.Components.Pages.SchoolMaster
{
    public partial class MenuMappingWithRoleBase
    {
        // One Module / Feature / Activity in the order tree
        public class MenuNode
        {
            public string LevelType { get; set; } = "";   // M / F / A
            public int Id { get; set; }
            public string? Name { get; set; }
            public bool IsCustom { get; set; }
            public bool IsExpanded { get; set; }
            public int Version { get; set; }   // bumped to re-render the position box
            public List<MenuNode> Children { get; set; } = new();
        }

        private List<SuperAdminDomain> Roles = new();
        private List<MenuNode> Modules = new();
        private int SelectedRoleId;
        private bool IsBusy = true;
        private bool IsDirty;
        private string SearchText = string.Empty;

        private IEnumerable<MenuNode> VisibleModules => string.IsNullOrWhiteSpace(SearchText)
            ? Modules
            : Modules.Where(m => !string.IsNullOrEmpty(m.Name) && m.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        // called by V3MAdminGuard after the page is rendered, only for V3M Admin
        private async Task LoadRoles()
        {
            try
            {
                var roles = await httpService.Get<List<SuperAdminDomain>>("RoleAdmin/GetAdd_Role") ?? new();
                Roles = roles.Where(r => r.IsValid).OrderBy(r => r.RoleName).ToList();
            }
            catch (Exception ex)
            {
                await Alert.ShowError($"{Localizer["Error"]}: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                StateHasChanged();
            }
        }

        private async Task OnRoleChanged(ChangeEventArgs e)
        {
            int.TryParse(e.Value?.ToString(), out var roleId);
            if (roleId == SelectedRoleId) return;

            if (IsDirty && !await Alert.ShowConfirm("You have unsaved changes. Discard them?"))
            {
                // force the select back to the current role
                var current = SelectedRoleId;
                SelectedRoleId = -1;
                StateHasChanged();
                await Task.Yield();
                SelectedRoleId = current;
                return;
            }

            SelectedRoleId = roleId;
            await LoadMenuOrder();
        }

        private async Task LoadMenuOrder()
        {
            Modules = new();
            IsDirty = false;
            SearchText = string.Empty;
            if (SelectedRoleId <= 0) return;

            try
            {
                IsBusy = true;
                var result = await httpService.Get<ApiResponse<List<RoleMenuOrderRow>>>($"SuperAdminModule/RoleMenuOrder/{SelectedRoleId}");
                Modules = BuildTree(result?.Data ?? new());
            }
            catch (Exception ex)
            {
                await Alert.ShowError($"{Localizer["Error"]}: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                StateHasChanged();
            }
        }

        // Rows already come sorted (module, feature, activity); GroupBy keeps that order
        private static List<MenuNode> BuildTree(List<RoleMenuOrderRow> rows) =>
            rows.GroupBy(r => r.ModuleId)
                .Select(m => new MenuNode
                {
                    LevelType = "M",
                    Id = m.Key,
                    Name = m.First().MName,
                    IsCustom = m.First().IsModuleCustom,
                    IsExpanded = true,
                    Children = m.GroupBy(r => r.FeatureId)
                        .Select(f => new MenuNode
                        {
                            LevelType = "F",
                            Id = f.Key,
                            Name = f.First().FeaturesName,
                            IsCustom = f.First().IsFeatureCustom,
                            IsExpanded = true,
                            Children = f.GroupBy(r => r.ActivityId)
                                .Select(a => new MenuNode
                                {
                                    LevelType = "A",
                                    Id = a.Key,
                                    Name = string.IsNullOrWhiteSpace(a.First().DisplayName) ? a.First().ActivityName : a.First().DisplayName,
                                    IsCustom = a.First().IsActivityCustom
                                }).ToList()
                        }).ToList()
                }).ToList();

        private void OnPositionChanged(List<MenuNode> list, MenuNode node, object? value)
        {
            int from = list.IndexOf(node);
            if (!int.TryParse(value?.ToString(), out var position)
                || Math.Clamp(position, 1, list.Count) - 1 == from)
            {
                node.Version++;   // invalid / same position: recreate the box with the real number
                return;
            }
            MoveTo(list, node, position);
        }

        // Moves node to a 1-based position inside its sibling list
        private void MoveTo(List<MenuNode> list, MenuNode node, int position)
        {
            int from = list.IndexOf(node);
            if (from < 0) return;

            int to = Math.Clamp(position, 1, list.Count) - 1;
            if (to == from)
            {
                StateHasChanged();
                return;
            }

            list.RemoveAt(from);
            list.Insert(to, node);
            IsDirty = true;
        }

        private async Task SaveOrder()
        {
            if (SelectedRoleId <= 0 || !Modules.Any()) return;

            // Current position of every node = its display order
            var items = new List<RoleMenuOrderItem>();
            for (int m = 0; m < Modules.Count; m++)
            {
                var module = Modules[m];
                items.Add(new RoleMenuOrderItem { LevelType = "M", RefId = module.Id, DisplayOrder = m + 1 });
                for (int f = 0; f < module.Children.Count; f++)
                {
                    var feature = module.Children[f];
                    items.Add(new RoleMenuOrderItem { LevelType = "F", RefId = feature.Id, DisplayOrder = f + 1 });
                    for (int a = 0; a < feature.Children.Count; a++)
                    {
                        items.Add(new RoleMenuOrderItem { LevelType = "A", RefId = feature.Children[a].Id, DisplayOrder = a + 1 });
                    }
                }
            }

            try
            {
                IsBusy = true;
                var response = await httpService.Post<ApiResponse<int>>("SuperAdminModule/RoleMenuOrder",
                    new RoleMenuOrderSaveRequest { RoleId = SelectedRoleId, Items = items });

                if (response == null || !response.Success)
                {
                    await Alert.ShowWarning(response?.Message ?? "Menu order could not be saved.");
                    return;
                }
                await Alert.ShowSuccess("Menu order " + Localizer["Saved"]);
            }
            catch (Exception ex)
            {
                await Alert.ShowError($"{Localizer["Error"]}: {ex.Message}");
                return;
            }
            finally
            {
                IsBusy = false;
            }
            await LoadMenuOrder();
        }

        private async Task ResetOrder()
        {
            if (SelectedRoleId <= 0) return;
            if (!await Alert.ShowConfirm("Reset this role's menu to the default order?")) return;

            try
            {
                IsBusy = true;
                var response = await httpService.Post<ApiResponse<int>>($"SuperAdminModule/RoleMenuOrder/Reset/{SelectedRoleId}", new { });
                if (response == null || !response.Success)
                {
                    await Alert.ShowWarning(response?.Message ?? "Menu order could not be reset.");
                    return;
                }
                await Alert.ShowSuccess(response.Message);
            }
            catch (Exception ex)
            {
                await Alert.ShowError($"{Localizer["Error"]}: {ex.Message}");
                return;
            }
            finally
            {
                IsBusy = false;
            }
            await LoadMenuOrder();
        }
    }
}
