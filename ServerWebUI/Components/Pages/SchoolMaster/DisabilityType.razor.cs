using ApplicationInterface.SchoolMaster;
using ApplicationInterface.SuperAdmin;
using DomainModel.Admin;
using DomainModel.Enum;
using DomainModel.SchoolMaster;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.JSInterop;
using MyApp.Common;

namespace ServerWebUI.Components.Pages.SchoolMaster
{
    public partial class DisabilityType
    {
        private bool IsLoading = true;
        private DisabilityTypeModel disabilitytype = new();
        private string oldcdataModel = "";
        private List<DisabilityTypeModel> disabilitytypelist = new();
        private bool ShowPopup = false;
        PaginationState pagination = new()
        {
            ItemsPerPage = 15
        };
        string nameFilter = string.Empty;
        private string statusFilter = "1";
        private string UserName;
        private string? branchcode;
        private string? groupcode;
        private string? currentUrl;
        protected override void OnInitialized()
        {

            PermissionState.OnChange += StateHasChanged;
            currentUrl = Nav.Uri;
            StateHasChanged();

        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var username = await sessionStorage.GetAsync<string>("UserName");
                UserName = username.Value;
                var branchResult = await sessionStorage.GetAsync<string>("BranchCode");
                branchcode = branchResult.Value;
                var groupResult = await sessionStorage.GetAsync<string>("GroupCode");
                groupcode = groupResult.Value;
                await LoadData();
             
                StateHasChanged();
            }
        }
        private async Task LoadData()
        {
            IsLoading = true;
            string ApiUri = "DisabilityType/GetAllDisabilityType";
            disabilitytypelist = await httpService.Get<List<DisabilityTypeModel>>(ApiUri) ?? new();
            IsLoading = false;


        }
        protected IQueryable<DisabilityTypeModel> FilteredItems
        {
            get
            {
                var result = disabilitytypelist.AsQueryable();
                if (!string.IsNullOrEmpty(nameFilter))
                {
                    result = result.Where(x => (x.DisabilityType != null && x.DisabilityType.Contains(nameFilter, StringComparison.CurrentCultureIgnoreCase)));

                }
                if (statusFilter == "1")
                {
                    result = result.Where(s => s.IsValid);
                }
                else if (statusFilter == "0")
                {
                    result = result.Where(s => !s.IsValid);
                }
                return result;
            }
        }
        private string GetStatusClass(bool isActive)
        {
            return isActive ? "activeClr" : "inactiveClr";
        }
        private void FilterChangedAsync(int filterValue)
        {
            statusFilter = filterValue.ToString();
            StateHasChanged();
        }
        private void AddDisabilityType()
        {
            ShowPopup = true;
            disabilitytype = new DisabilityTypeModel();
        }
        private async Task OnSave()
        {
            try
            {
                if (disabilitytype.SeedId > 0)
                {
                    ShowPopup = false;
                    var isConfirmed = await JS.InvokeAsync<bool>("showUpdateConfirm");
                    if (!isConfirmed)
                    {
                        ShowPopup = true;
                        return;
                    }
                    bool isSame = CommonMethod.IsEdited(disabilitytype, oldcdataModel);
                    if (!isSame)
                    {
                        await Alert.ShowWarning("No changes detected.");
                        ShowPopup = true;
                        return;
                    }
                }
                ShowPopup = false;
                disabilitytype.IsValid = true;
                disabilitytype.CreatedBy = UserName;
                disabilitytype.GroupCode = groupcode;
                disabilitytype.BranchCode = branchcode;
                string apiUrl = "DisabilityType/AddOrUpdateDisabilityType";
                var apiResponse = await httpService.Post<ApiResponse<object>>(apiUrl, disabilitytype);
                var responsecode = apiResponse.Code;
                if (responsecode == 0)
                {
                    await Alert.ShowWarning("Disability Type" + @Localizer["Already"]);
                }
                else if (responsecode == 1)
                {
                    await Alert.ShowSuccess("Disability Type" + @Localizer["Inserted"]);
                }
                else if (responsecode == 2)
                {
                    await Alert.ShowSuccess("Disability Type" + @Localizer["Updated"]);
                }
                else
                {
                    await Alert.ShowCustomAlert(@Localizer["Admin"]);
                }

                await LoadData();
                disabilitytype = new DisabilityTypeModel();

            }
            catch (Exception)
            {
                return;
            }
            finally
            {

                StateHasChanged();
            }
        }

        private void EditDisabilityType(DisabilityTypeModel item)
        {
            disabilitytype = new DisabilityTypeModel
            {
                SeedId = item.SeedId,
                DisabilityType = item.DisabilityType,
                DisplayOrder = item.DisplayOrder,
            };
            oldcdataModel = CommonMethod.CreateSnapshot(disabilitytype);
            ShowPopup = true;
        }

        private void ClosePopup()
        {
            ShowPopup = false;
            disabilitytype = new DisabilityTypeModel();
            StateHasChanged();
        }
        private async Task ChangeDisabilityTypeStatus(int id, bool isValid)
        {
            try
            {
                bool isActivate = !isValid;
                bool isConfirmed = await JS.InvokeAsync<bool>(isActivate ? "showActiveConfirm" : "showDeleteConfirm");
                if (!isConfirmed)
                    return;

                string apiUrl = "DisabilityType/DeleteOrDisabilityType";

                var apiResponse = await httpService.Post<ApiResponse<object>>(apiUrl, id);

                if (apiResponse.Success)
                {
                    if (isActivate)
                        await Alert.ShowActive(Localizer["Activated"]);
                    else
                        await Alert.ShowDeactive(Localizer["Deleted"]);

                    await LoadData();
                }
                else
                {
                    await Alert.ShowCustomAlert(@Localizer["Admin"]);
                }
            }
            catch (Exception ex)
            {
                await Alert.ShowError($"{Localizer["Error"]}: {ex.Message}");
            }
            finally
            {
                StateHasChanged();
                await LoadData();
            }
        }
    }
}
