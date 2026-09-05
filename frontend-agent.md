# Blazor Project — Conventions & Handoff Reference

> Paste this whole document at the start of a new chat. It captures every UI/code
> convention established across a long working session so pages continue to be
> built exactly the same way — same classes, same structure, same patterns.

---

## 1. Tech stack / context

- **Blazor Server**, `@rendermode InteractiveServer`.
- Grids use **QuickGrid** (`Microsoft.AspNetCore.Components.QuickGrid`).
- Alerts via injected `SweetAlertService Alert` (`Alert.ShowSuccess`, `Alert.ShowWarning`,
  `Alert.ShowError`, `Alert.ShowConfirm`).
- A JS helper `showSelectRecordAlert` (SweetAlert2-based) is called via
  `JS.InvokeVoidAsync("showSelectRecordAlert")` whenever "please select a record" is
  needed. **Critical bug learned:** the JS function must `return Swal.fire(...)` —
  without `return`, `InvokeVoidAsync` resolves instantly instead of waiting for the
  user to dismiss the alert.
- HTTP calls go through injected `Shared.IHttpService httpService` with
  `httpService.Get<T>(url)` and `httpService.Post<T>(url, body)`, both wrapped in
  `ApiResponse<T>` (`Success`, `Message`, `Code`, `Data`).
- Session storage accessed via `sessionStorage.GetAsync<string>("BranchCode")`,
  `"GroupCode"`, `"UserName"`, etc. (no explicit `@inject` line needed — available
  globally, likely via a base component).
- `NavigationManager Nav`, `ICommonMethod _commonmethod` (or `CommonMethod`) also
  globally available; `_commonmethod.SetCurrentSessionData()` returns the current
  session id (`long`) on page load.
- `PermissionState` is a DI singleton with `.OnChange` event, subscribed in
  `OnInitialized()` for permission-driven re-renders.

## 2. Page skeleton (always this shape)

```razor
@page "/RouteName"
@using DomainModel.Admin
@using DomainModel.SchoolMaster
@using Microsoft.AspNetCore.Components.QuickGrid
@using MyApp.Common
@using Microsoft.AspNetCore.Authorization
@using ServerWebUI.Components.Pages.CommonPages
@inject Shared.IHttpService httpService
@inject IJSRuntime JS
@inject HttpClient Http
@inject IStringLocalizer<Resource> Localizer
@inject SweetAlertService Alert
@inject ICommonMethod _commonmethod
@rendermode InteractiveServer

<div class="main-container">
    <div class="row m-0 main-row">
        <div class="col-12 p-1" id="main-header">
            <PageHeading Tittle="Page Title Here">
                <!-- header controls here -->
            </PageHeading>
        </div>

        <div class="col-12 p-1">
            <div class="card">
                <div class="card-body table-card p-0">
                    <div class="quick-main">
                        <div class="grid-box-with-btn">
                            <QuickGrid Items="@moduleQuery" Class="table" Pagination="@pagination">
                                <!-- columns -->
                            </QuickGrid>
                        </div>
                        <div class="footer-pagination">
                            <div class="d-flex align-items-center">
                                <div class="welcome-text w-100">
                                    <Paginator State="@pagination" />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
```

**IMPORTANT quirk:** the `PageHeading` component's title parameter is spelled
**`Tittle`** (double-t typo), not `Title`. Always use `Tittle="..."`.

Sometimes `Tittle` is omitted entirely if the user says "no title, just PageHeading" —
then use `<PageHeading>` bare with no title attribute.

## 3. Loader overlay (always at the very end of markup, outside main-container)

```razor
@if (IsLoading)
{
    <div class="grid-loader-overlay">
        <img src="images/loading.gif" alt="Loading..." />
    </div>
}
else
{

}
```

## 4. Grid pagination footer — EXACT pattern, never add "Total Records" text unless explicitly asked

```razor
<div class="footer-pagination">
    <div class="d-flex align-items-center">
        <div class="welcome-text w-100">
            <Paginator State="@pagination" />
        </div>
    </div>
</div>
```

Only if the user explicitly asks for a total count does the `justify-content-between`
+ "Total Records : @list.Count" + `<Paginator>` variant get used instead. Default is
always the plain version above.

`PaginationState pagination = new() { ItemsPerPage = 15 };` (or a smaller number like
6 for nested/secondary grids).

## 5. Grid container class variants

- **`grid-box`** — default/master grid, no toolbar row above it.
- **`grid-box-with-btn`** — used whenever there's a filter/toolbar row baked into the
  same card as the grid (most search pages use this).
- Never use inline `style="height: ..."` on the grid wrapper — just the class, no
  inline styles, e.g. `<div class="grid-box-with-btn">` alone, nothing else.

## 6. Column patterns

**Search box in a column header (`ColumnOptions`):**
```razor
<PropertyColumn Property="@(x => string.IsNullOrWhiteSpace(x.Field) ? " " : x.Field)" Title="Label" Sortable="true" Align="Align.Left">
    <ColumnOptions>
        <div class="search-box">
            <input type="search" autofocus @bind="nameFilter" @bind:event="oninput" placeholder="Search..." />
        </div>
    </ColumnOptions>
</PropertyColumn>
```
Multiple columns can bind to the SAME `nameFilter` if the user wants one shared search
box across two columns (they will usually specify this — "esko ek hi rakho name
filter").

**Every string PropertyColumn must guard nulls with a space fallback:**
`x => string.IsNullOrWhiteSpace(x.Field) ? " " : x.Field` — never show raw null/empty.

**Date columns:**
`x => x.SomeDate.HasValue ? x.SomeDate.Value.ToString("dd-MM-yyyy") : " "`

**`moduleQuery` filtering pattern (this exact property-getter shape, not a one-liner
`.AsQueryable()`):**
```csharp
protected IQueryable<TModel> moduleQuery
{
    get
    {
        var result = list.AsQueryable();
        if (!string.IsNullOrEmpty(nameFilter))
        {
            result = result.Where(x => (x.StudentName != null && x.StudentName.Contains(nameFilter, StringComparison.CurrentCultureIgnoreCase)) ||
           (x.ControlNo != null && x.ControlNo.Contains(nameFilter, StringComparison.CurrentCultureIgnoreCase)));
        }
        return result;
    }
}
```

**Checkbox select-all column:**
```razor
<TemplateColumn Title="" Align="Align.Center">
    <HeaderTemplate>
        <div class="d-flex align-items-center justify-content-center">
            <input type="checkbox" @bind="IsAllSelected" @bind:after="ToggleAll" @onclick:stopPropagation="true" />
        </div>
    </HeaderTemplate>
    <ChildContent>
        <div class="d-flex align-items-center justify-content-center">
            <input type="checkbox" @bind="context.IsSelected" @onclick:stopPropagation="true" />
        </div>
    </ChildContent>
</TemplateColumn>
```
```csharp
private void ToggleAll()
{
    foreach (var row in list)
        row.IsSelected = IsAllSelected;
}
```

**Single-select radio in a grid (rare, when explicitly asked for single selection):**
enforce manually by clearing all others in the handler, OR prefer the simpler
`@bind="context.IsSelected"` checkbox pattern with a `.Count == 0 || .Count > 1` guard
in the action method (this is actually the pattern the user settled on most — plain
checkboxes everywhere, with the action validating "exactly one" where needed).

**Editable inline grid cells (row-level edit, e.g. Roll No, Bank details):**
```razor
<TemplateColumn Title="Field" Class="text-center" Align="Align.Center">
    <ChildContent>
        <input class="form-input" @bind="context.SomeField" @bind:event="oninput" placeholder=" " />
    </ChildContent>
</TemplateColumn>
```
Editing `context.SomeField` directly mutates the underlying list item (same
reference), which is what makes bulk-submit-selected-rows work.

**Conditionally disabled inline input based on a sibling field in the same row (e.g.
disable "Description" until "Is Disability" = Yes):**
```razor
<input class="form-input" @bind="context.NatureOfDisability" @bind:event="oninput"
       disabled="@(context.IsDisability != "Yes")" placeholder=" " />
```

**Status icon column (green check / red cross):**
```razor
@if (context.Verified == 1)
{
    <i class="bi bi-check-circle-fill" style="color:#28a745; font-size:18px;"></i>
}
else
{
    <i class="bi bi-x-circle-fill" style="color:#dc3545; font-size:18px;"></i>
}
```

**Image thumbnail in grid (fixed square, clickable to preview popup):**
```css
.grid-thumb-img {
    width: 70px;
    height: 70px;
    object-fit: cover;
    border: 1px solid #d9d9d9;
    cursor: pointer;
}
```
```razor
<img src="@GetImageUrl(context.ImagePath)" class="grid-thumb-img" @onclick="() => ViewImage(context.ImagePath)" alt="..." />
```
```csharp
private const string NoPhotoUrl = "https://demo.schooleye.in/Images/NoPhoto.gif";
private string GetImageUrl(string? imagePath) => string.IsNullOrWhiteSpace(imagePath) ? NoPhotoUrl : imagePath;
```

## 7. Popups / Modals

**Search popup (the sliders-icon popup) — fixed size, NEVER vertically centered:**
```razor
<div class="custom-modal row m-0">
    <div class="col-12 col-sm-7 col-md-5 col-lg-4 col-xl-9">
        <div class="modal-content clearfix main-popup">
            <div class="head-btn">
                <h5 class="modal-title">Search</h5>
                <button data-access="Close" type="button" @onclick="ClosePopup" class="close"><span aria-hidden="true">×</span></button>
            </div>
            <div class="modal-body">
                <div class="row m-0 form-row">
                    <!-- filter fields, col-lg-3 or col-lg-4 each -->
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="ui fade op-1 animated button form-btn" @onclick="SearchMethod" tabindex="0">
                    <div class="visible content">Search</div>
                    <div class="hidden content"><i class="bi bi-search"></i></div>
                </button>
            </div>
        </div>
    </div>
</div>
```
**CRITICAL RULE:** `<div class="custom-modal row m-0">` — **NEVER** add
`align-items-center` to this wrapper (it was explicitly removed and must stay
removed on every popup, search or otherwise). This applies globally to all popups
going forward.

**Close button — always this exact markup**, not an icon-only `close-btn` class:
```razor
<button data-access="Close" type="button" @onclick="ClosePopup" class="close"><span aria-hidden="true">×</span></button>
```

**Sliders icon that opens the search popup (in PageHeading):**
```razor
<button class="icon-btn icon-btn-white open-popup" @onclick="OpenSearch" data-id="popup" tabindex="0">
    <i class="bi bi-sliders"></i>
</button>
```

**Main-container-wrapped popups (bigger, col-xl-12 or col-xl-9) for Add/Edit forms**
use the same `custom-modal row m-0` wrapper (no `align-items-center`), with
`EditForm`/`DataAnnotationsValidator` inside when validation is needed, or a plain
`<div class="modal-body">` when not.

**Popup footer Save/Cancel button component:**
```razor
<SaveUpdateButton TextSave="Submit" OnClickCancel="ClosePopup" />
```
or, when only a Cancel/Close button is wanted (no save button rendered by the
component itself, e.g. Save is a separate custom button):
```razor
<SaveUpdateButton ShowSaveButton="false" OnClickCancel="@ClosePopup" />
```

**Divider/section header inside a popup or card body** (used for "Father's Details",
"Mother's Details", "Discipline Details" etc. — NOT a colored badge/button look):
```razor
<div class="col-12 col-sm-12 col-md-12 col-lg-12 px-2 mb-3">
    <div class="divider-box d-flex justify-content-between align-items-center gap-2">
        <p class="total-text-with-bg">
            Section Title
        </p>
        <hr class="m-0 w-webkit text-light-gray op-1">
        <!-- optional action button can sit here too, e.g. Change/Select Student -->
    </div>
</div>
```
Note: **no `bg-light-gray` class** on the `<p>` in the corrected version — that class
made it render as a solid blue pill/button, which was wrong. Just `total-text-with-bg`.

**`hr` divider line rendering as broken/dashed instead of solid:** this is the
browser's default two-tone `border: inset` style. Fix by forcing a solid border:
```css
hr { border: none; border-top: 1px solid #d9d9d9; opacity: 1; }
```

## 8. Form field patterns

**Standard floating-label input:**
```razor
<div class="form-box-group">
    <InputText class="floating-input" placeholder=" " @bind-Value="model.Field" />
    <label class="floating-label">Field Name</label>
    <ValidationMessage For="@(() => model.Field)" />
</div>
```

**Required field label:**
```razor
<label class="floating-label">
    Field Name <span class="required">*</span>
</label>
```

**Dropdown (static-label variant used when the label needs to sit above the select,
e.g. for radios/dropdowns with an option list):**
```razor
<div class="form-box-group static-label">
    <InputSelect class="floating-input" placeholder=" " @bind-Value="model.Field">
        <option value="">--Select--</option>
        @foreach (var item in list)
        {
            <option value="@item.Id">@item.Name</option>
        }
    </InputSelect>
    <label class="floating-label">Field Name</label>
</div>
```

**Readonly/disabled display field (used to show selected-record details):**
```razor
<div class="form-box-group">
    <input class="floating-input" placeholder=" " value="@model.Field" readonly disabled />
    <label class="floating-label">Field Name</label>
</div>
```

**Radio group (Yes/No, All/Active/Inactive, etc.):**
```razor
<div class="form-box-group static-label">
    <label class="floating-label">Field Name</label>
    <div class="border-box d-flex justify-content-center align-items-center gap-3">
        <InputRadioGroup @bind-Value="model.Field">
            <div class="d-flex align-items-center gap-1">
                <InputRadio Value="@("1")" />
                <label class="form-label">Yes</label>
            </div>
            <div class="d-flex align-items-center gap-1">
                <InputRadio Value="@("0")" />
                <label class="form-label">No</label>
            </div>
        </InputRadioGroup>
    </div>
</div>
```
- For `bool`-typed fields, `InputRadio Value="true"` / `Value="false"` (no quotes —
  literal bools).
- For `string`-typed fields, `Value="@("1")"` etc.

**Border-box label+value pair (used for counter displays, Max Strength, etc.):**
```razor
<div class="form-box">
    <div class="border-box d-flex justify-content-between align-items-center gap-2 px-2">
        <label class="form-label">Field Name</label>
        <input class="form-input" placeholder=" " value="@value" disabled />
    </div>
</div>
```
Or with label above input (not inline) when the user wants a stacked look:
```razor
<div class="form-box">
    <label class="form-label">Field Name</label>
    <input class="form-input" value="@value" readonly />
</div>
```

**File upload row (Photo/Sign panels with upload+delete icon buttons):**
```razor
<div class="form-box-group static-label">
    <label for="" class="floating-label">Sign</label>
    <div class="border-box h-auto">
        <a href="@ImagePreviewVar" target="_blank">
            <img src="@ImagePreviewVar" class="w-100" alt="photo" />
            <InputFile id="uniqueId" OnChange="@(e => OnFileSelected(e, "uniqueId"))" class="w-100" accept="image/*" style="display:none" />
        </a>
        <div class="action-btn-group justify-content-around p-1">
            <button type="button" class="btn-icon edit-btn" @onclick='() => OpenFilePicker("uniqueId")'>
                <i class="bi bi-upload"></i>
            </button>
            <button type="button" class="btn-icon inactive-btn" @onclick='() => DeleteImage("uniqueId")'>
                <i class="bi bi-trash"></i>
            </button>
        </div>
    </div>
</div>
```
Default "no photo" placeholder: `"https://demo.schooleye.in/Images/NoPhoto.gif"`.
File save pattern: save the physical file to `wwwroot/uploads/<Module>/<mm>/<dd>/<guid>_<filename>`
using `Path.Combine(Environment.CurrentDirectory, "wwwroot", "uploads", ...)` for the
disk write, but store only the **web-relative path** (`"/uploads/<Module>/.../file.png"`)
on the model — never the absolute disk path — since that's what `<img src>` needs to
resolve in the browser.

## 9. Buttons

**Header/toolbar action button (plain):**
```razor
<div class="form-box">
    <button class="table-btn" @onclick="SomeMethod">Button Label</button>
</div>
```

**Animated Search button (used both in header and popup footers):**
```razor
<button type="button" class="ui fade op-1 animated button form-btn" @onclick="SearchMethod" tabindex="0">
    <div class="visible content">Search</div>
    <div class="hidden content"><i class="bi bi-search"></i></div>
</button>
```
Swap the label text for "Go" or other verbs as needed, same structure.

**Icon-only button (e.g. move left/right between two grids):**
```razor
<button class="icon-btn" @onclick="MoveSelectedToRight" title="Move selected to allocated section">
    <i class="bi bi-play-fill"></i>
</button>
```
Back/left variant uses `<i class="bi bi-caret-left-fill"></i>`.

**ActionButtonGroup for grid rows (Edit + Status toggle):**
```razor
<ActionButtonGroup CurrentUrl="@currentUrl" OnEditClick="() => Edit(context)" OnStatusClick="() => ChangeStatus(context.Id, context.IsValid)" />
```
For a header "Add" button:
```razor
<ActionButtonGroup CurrentUrl="@currentUrl" Add="OpenNewPopup" />
```

**AdmitChildButton for Export/Print actions:**
```razor
<AdmitChildButton CurrentUrl="@currentUrl" ButtonText="Export To Excel" Export="ExportMethod" />
<AdmitChildButton CurrentUrl="@currentUrl" ButtonText="Print Summary" Print="PrintMethod" />
```

**Active/selected toggle button state (e.g. period filter buttons — Today / This Week
/ This Month / This Year):**
```razor
<button type="button" class="table-btn @(SelectedPeriod == "Today" ? "active" : "")" @onclick='() => ApplyPeriodFilter("Today")'>
    Today
</button>
```
```css
.table-btn.active {
    background-color: #1e8fd3;
    color: #ffffff;
    border: 1.5px solid #1e8fd3;
    transform: translateY(-1px);
}
```
The `.active` class mirrors the existing `:hover` state exactly, so the currently
selected button stays visually highlighted without needing to hover. Never edit the
base `:hover` rule to achieve this — always add a separate `.active` class.

## 10. Confirmation dialog pattern (reusable template)

Whenever a destructive/important action needs a confirm-then-fixed-message flow:
```csharp
private async Task SomeAction()
{
    var submit = await Alert.ShowConfirm("Are you sure, you want to <action> the selected record?");
    if (!submit)
    {
        StateHasChanged();
        return;
    }

    IsLoading = true;
    StateHasChanged();

    try
    {
        var apiResponse = await httpService.Post<ApiResponse<object>>("Controller/Endpoint", model);
        if (apiResponse != null && apiResponse.Success)
        {
            await Alert.ShowSuccess("Selected record <action>d successfully."); // fixed message, NOT server's message
            await ReloadListMethod();
        }
        else
        {
            await Alert.ShowWarning(apiResponse?.Message ?? "Operation failed.");
        }
    }
    catch (Exception ex)
    {
        await Alert.ShowError($"{@Localizer["Error"]}: {ex.Message}");
    }
    finally
    {
        IsLoading = false;
        StateHasChanged();
    }
}
```
Note: on success the message shown is often a **fixed custom string** specified by
the user, not `apiResponse.Message` — always check whether the user gave an exact
success string to use verbatim.

## 11. "Select a record" validation pattern (before an action on grid selection)

```csharp
var selected = list.Where(x => x.IsSelected).ToList();
if (selected.Count == 0)               // or: selected.Count == 0 || selected.Count > 1  (single-select only)
{
    await JS.InvokeVoidAsync("showSelectRecordAlert");
    return;
}
```

## 12. Popup close/reopen sequencing (learned pattern — order matters)

When an action must show an alert/message while a popup is open, the established
sequence is:
1. Set `ShowPopup = false;` `StateHasChanged();` (popup visually closes)
2. `await` the alert/JS call (waits for user to dismiss)
3. Set `ShowPopup = true;` `StateHasChanged();` (popup reopens) — only if staying open
   is the desired UX (e.g. validation failed, let them fix and retry)
4. If the action succeeded and should fully complete, leave the popup closed instead.

Example — "select a record" validation inside a nested popup:
```csharp
if (selected.Count == 0)
{
    ShowPopup = false;
    StateHasChanged();

    await JS.InvokeVoidAsync("showSelectRecordAlert");

    ShowPopup = true;
    StateHasChanged();
    return;
}
```

## 13. Cascading Class → Section pattern (used everywhere a Class dropdown exists)

```csharp
private List<SectionModel> sectionlist = new();
private SearchAnyRequestModel searchAny = new();

private async Task OnClassChanged()
{
    searchRequest.SectionCode = string.Empty;
    sectionlist = new();

    if (string.IsNullOrEmpty(searchRequest.ClassCode))
    {
        StateHasChanged();
        return;
    }

    searchAny.GroupCode = groupcode;
    searchAny.BranchCode = branchcode;
    searchAny.RequestName = searchRequest.ClassCode;

    string ApiUri1 = "ClassSection/GetClassWithSection";
    var apiResponse = await httpService.Post<ApiResponse<List<SectionModel>>>(ApiUri1, searchAny) ?? new();
    if (apiResponse.Success)
    {
        sectionlist = apiResponse.Data ?? new();
    }
    StateHasChanged();
}
```
Bind on the Class `InputSelect` with `@bind-Value:after="OnClassChanged"`.

**IMPORTANT — always keep separate `sectionlist`/`searchAny` instances per distinct
Class dropdown on a page.** If a page has both a header Class filter AND a nested
popup's own Class filter (e.g. Select Student popup), use two separate lists
(`sectionlist` vs `listSectionlist`) and two separate `SearchAnyRequestModel`
instances so they don't clobber each other.

## 14. Common reference models seen throughout (assume these exist already)

- `SessionModel` — `SessionId` (long), `SessionName` (string), `IsValid` (bool)
- `BranchClassModel` — `ClassCode`, `ClassName`, `IsValid`
- `SectionModel` — `SectionCode` or `SectionId`, `SectionName`
- `GenderModal` — `GenderCode`/`GenderName`, `IsValid`
- `SearchAnyRequestModel` — `GroupCode`, `BranchCode`, `RequestName` (generic
  "give me sections for this class code" request)
- `MstDistance` — `DistanceId`, `DistanceName`
- `ApiResponse<T>` — `Success` (bool), `Message` (string), `Code` (int), `Data` (T)

## 15. Standard `@code` skeleton for a search+grid page

```csharp
private List<SessionModel> sessionlist = new();
private List<BranchClassModel> classlist = new();
private List<SectionModel> sectionlist = new();
private List<TResponse> list = new();

private TRequest searchRequest = new();
private SearchAnyRequestModel searchAny = new();

private bool ShowSearchPopup;
private PaginationState pagination = new() { ItemsPerPage = 15 };

private long CurrentSession;
private bool IsLoading = false;
private string? nameFilter;

private string? branchcode;
private string? groupcode;
private string username = string.Empty;
private string currentUrl = "";

protected override void OnInitialized()
{
    PermissionState.OnChange += StateHasChanged;
    currentUrl = Nav.Uri;
}

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        CurrentSession = await _commonmethod.SetCurrentSessionData();

        var userResult = await sessionStorage.GetAsync<string>("UserName");
        username = userResult.Value ?? string.Empty;

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
    sessionlist = await httpService.Get<List<SessionModel>>("Session/GetSession") ?? new();
    classlist = await httpService.Get<List<BranchClassModel>>("BranchClass/GetClassCode") ?? new();
    StateHasChanged();
}

private void OpenSearch() => ShowSearchPopup = true;
private void ClosePopup() => ShowSearchPopup = false;

private async Task SearchStudents()
{
    searchRequest.GroupCode = groupcode;
    searchRequest.BranchCode = branchcode;
    searchRequest.SessionId = CurrentSession;

    ShowSearchPopup = false;
    IsLoading = true;
    StateHasChanged();

    try
    {
        string apiUrl = "Controller/Endpoint";
        var apiResponse = await httpService.Post<ApiResponse<List<TResponse>>>(apiUrl, searchRequest);

        list = (apiResponse != null && apiResponse.Success) ? (apiResponse.Data ?? new()) : new();

        if (apiResponse != null && apiResponse.Success && apiResponse.Code == 0)
        {
            await Alert.ShowWarning(apiResponse.Message ?? "No records found.");
        }
    }
    catch
    {
        list = new();
        await Alert.ShowWarning("An error occurred while searching.");
    }
    finally
    {
        IsLoading = false;
        StateHasChanged();
    }
}
```

## 16. Master CRUD pages (Country.razor / RTECategory.razor pattern)

For simple lookup masters (Category, Distance, etc.):
- Page heading: Title + Session dropdown (if session-scoped) + `ActionButtonGroup`
  with `Add`.
- Grid: name column w/ search, Status column w/ Both/Active/Inactive radio filter in
  `ColumnOptions`, Action column w/ `ActionButtonGroup` (Edit + Status toggle).
- Popup sized by field count (Rule B from early in the project):

| Field count | outer `col-xl-*` | each field row `col-lg-*` |
|---|---|---|
| 1–7 | `col-xl-5` | `col-lg-12` (one per row) |
| 8–15 | `col-xl-8` | `col-lg-4` (3 per row) |
| 16–20 | `col-xl-10` | `col-lg-4` |
| 21–30+ | `col-xl-12` | `col-lg-3` (4 per row) |

- `EditForm` + `DataAnnotationsValidator`, Save switches on response `Code`:
  `0` = already exists (warning), `1` = inserted (success), `2` = updated (success).
- Status toggle sends the row's id via a confirm dialog then reloads the grid.

## 18. Model validation — DataAnnotations (mandatory on every Add/Edit model)

Every model bound to an `EditForm` MUST carry proper `System.ComponentModel.DataAnnotations`
attributes. Never rely only on the client marking a field with `<span class="required">*</span>` —
the visual asterisk is cosmetic only; the actual enforcement comes from the attribute.

```csharp
using System.ComponentModel.DataAnnotations;

public class BranchModel
{
    public int BranchId { get; set; }

    [Required(ErrorMessage = "Group Code is required.")]
    public string? GroupCode { get; set; }

    [Required(ErrorMessage = "Branch Code is required.")]
    [StringLength(20, ErrorMessage = "Branch Code cannot exceed 20 characters.")]
    public string? BranchCode { get; set; }

    [Required(ErrorMessage = "Branch Name is required.")]
    [StringLength(100)]
    public string? BranchName { get; set; }

    [Required(ErrorMessage = "Contact Person is required.")]
    public string? ContactPerson { get; set; }

    [Required(ErrorMessage = "Contact Number is required.")]
    [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Enter a valid 10-digit contact number.")]
    public string? ContactNo { get; set; }

    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string? ContactEmailId { get; set; }
}
```

**Rules:**
- Every field marked `<span class="required">*</span>` in the UI MUST have a matching
  `[Required]` on the model — these two are a pair, never one without the other.
- Always give `[Required]`/`[StringLength]`/`[RegularExpression]` an explicit
  `ErrorMessage` — never leave the framework default message, since it references the
  C# property name, not the friendly label shown to the user.
- Dropdown "must select" fields (e.g. `CountryId`, `StateId` typed as `int`) use
  `[Range(1, int.MaxValue, ErrorMessage = "Please select a Country.")]` since `0` is
  usually the default/placeholder value and `[Required]` alone won't catch it for
  value types.
- If the user gives you a model without attributes, add the sensible ones yourself
  based on which fields show `<span class="required">*</span>` in the form, and flag
  the addition in the `// ASSUMPTIONS` comment block.

## 19. Submit button patterns — two distinct flows, never mix them

There are exactly two accepted ways a form gets submitted. Pick the one matching what
already exists on the page; don't introduce a third pattern.

### 19.1 `EditForm` + `OnValidSubmit` (preferred, used on almost every Add/Edit popup)

```razor
<EditForm Model="@model" OnValidSubmit="@OnSave">
    <DataAnnotationsValidator />
    <!-- fields with ValidationMessage next to each -->
    <div class="modal-footer">
        <div class="form-group btn-gr">
            <SaveUpdateButton Id="model.Id" TextSave="Submit" OnClickCancel="ClosePopup" />
        </div>
    </div>
</EditForm>
```
- `SaveUpdateButton`'s internal button is `type="submit"` (assume this — it's what
  makes `OnValidSubmit` fire). This means **`OnSave` is only ever called after all
  `[Required]`/attribute checks on `model` have already passed** — no manual
  validation needed at the top of `OnSave` for field-level rules.
- `OnSave` should therefore start directly with business-logic checks (duplicate
  check, confirm dialog, API call) — NOT with a `if (string.IsNullOrEmpty(...))`
  re-check of the same fields the attributes already cover. Re-checking there is
  redundant and a sign the model is missing an attribute instead.
- `DataAnnotationsValidator` MUST be the first child of every `EditForm` that uses
  this pattern — a form without it will show no validation messages even if the
  model has attributes.
- Every field needs a matching `<ValidationMessage For="@(() => model.Field)" />`
  right after its input — never share one `ValidationMessage` across fields, and
  never omit it for a `[Required]` field (silent failures are not acceptable).

### 19.2 Plain button + `@onclick` (used when Save isn't the natural form-submit action —
e.g. bulk "Submit Selected Rows" from a grid, or a button living outside any `<EditForm>`)

```razor
<button type="button" class="ui fade op-1 animated button form-btn" @onclick="OnSaveManual" tabindex="0">
    <div class="visible content">Submit</div>
    <div class="hidden content"><i class="bi bi-check2"></i></div>
</button>
```
```csharp
private async Task OnSaveManual()
{
    // Manual validation is MANDATORY here — there is no EditForm/DataAnnotationsValidator
    // to do it automatically, so every "required" rule must be re-checked in code.
    if (selectedRows.Count == 0)
    {
        await JS.InvokeVoidAsync("showSelectRecordAlert");
        return;
    }

    var invalidRow = selectedRows.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.RollNo));
    if (invalidRow != null)
    {
        await Alert.ShowWarning("Roll No is required for all selected students.");
        return;
    }

    // proceed to API call ...
}
```
- Use this pattern ONLY when the fields being submitted are outside an `EditForm`
  (grid inline edits, multi-row bulk actions) — never as a shortcut to avoid setting
  up `DataAnnotationsValidator` on a normal single-model form. If a model exists and
  attributes are already on it, always prefer 19.1.
- Every manual-validation branch must `return` immediately after showing the alert —
  never fall through to the API call after a failed check.

## 20. Alert-based validation messaging — when to use which

| Situation | What to show |
|---|---|
| A single `[Required]`/attribute-level field failure inside an `EditForm` | `<ValidationMessage>` inline under the field — NOT a popup alert. `OnValidSubmit` won't even fire, so no alert is needed. |
| A cross-field / business rule failure (e.g. "End Time must be after Start Time", "duplicate Branch Code") caught inside `OnSave` after attributes already passed | `await Alert.ShowWarning("...")` with a specific, human-readable message — never a generic "Invalid data." |
| "Please select a record" before an action on a grid | `await JS.InvokeVoidAsync("showSelectRecordAlert")` — the dedicated SweetAlert helper, not `Alert.ShowWarning`. |
| Server responded with `Code == 0` (duplicate / already exists) | `await Alert.ShowWarning(apiResponse.Message)` if the server supplies a specific message, else a fixed fallback string. |
| Server responded with `Code == 1` (inserted) / `Code == 2` (updated) | `await Alert.ShowSuccess(...)` / `await Alert.ShowUpdate(...)` — prefer the exact fixed string the user specified over echoing `apiResponse.Message` verbatim. |
| Unhandled exception in a `try/catch` | `await Alert.ShowError($"{@Localizer["Error"]}: {ex.Message}")` — always inside `catch`, never swallowed silently. |
| No changes detected on Edit (unchanged form re-submitted) | `await Alert.ShowWarning("No changes detected.")`, then reopen the popup (see §12 sequencing) — don't call the API at all. |

**Never** combine `ValidationMessage` and a popup alert for the *same* field-level
rule — pick one per failure type as per the table above, otherwise the user sees the
same complaint twice.

## 21. Pre-submit testing checklist (run through this on every Add/Edit form before considering it "done")

Before handing back a form/popup as finished, mentally (or actually) walk through:

1. **Empty submit** — click Submit with every field blank. Every `[Required]` field
   must show its `ValidationMessage`, and `OnSave`/API must NOT be called (verify no
   network call fires — the form should visibly stay open with red messages, not
   flash a loader).
2. **Partial fill** — fill only some required fields, submit again. Only the
   remaining unfilled required fields should show messages; previously-valid ones
   should clear their message as soon as they become valid (this is automatic with
   `DataAnnotationsValidator`, but confirm it wasn't accidentally short-circuited by
   a manual `if` check earlier in the flow).
3. **Format violations** — invalid email, wrong-length phone number, negative
   number where not allowed — confirm the corresponding `[RegularExpression]`/
   `[Range]`/`[EmailAddress]` attribute catches it with a readable message.
4. **Duplicate/business-rule check** — submit a value known to already exist (e.g.
   same `BranchCode` twice) — confirm the server's `Code == 0` path shows a warning
   and does NOT close the popup or reset the form (user should be able to fix and
   retry without re-typing everything).
5. **Successful submit** — all valid, new record — confirm `Code == 1` success alert
   fires with the correct message, popup closes, and the grid reloads/reflects the
   new row without a manual page refresh.
6. **Successful update** — edit an existing record, change one field, submit —
   confirm `Code == 2` update alert fires, popup closes, grid reflects the change.
7. **No-change edit** — open Edit, change nothing, submit — confirm the "No changes
   detected." warning fires (per §20) and the API is never called (check this isn't
   silently skipped for forms that also have file uploads — an unchanged file
   selection should still count as "no changes" unless a new file was actually
   picked, per the `isLogoChanged`/`isXxxChanged` boolean flag pattern already used
   for image fields).
8. **File upload fields specifically** — confirm: (a) selecting an oversized file
   (over the stated MB limit) shows a warning and does not proceed with the upload;
   (b) the delete/trash icon clears both the model's path field and the preview
   variable; (c) on Edit, the existing image loads into the preview correctly before
   any new file is chosen.
9. **Cancel/close mid-form** — fill some fields, click Cancel/×, reopen Add — confirm
   the model was reset to a fresh `new()` and stale values from the previous attempt
   don't leak into the reopened form.
10. **Loading state** — confirm `IsLoading`/loader overlay shows during the actual
    API round-trip and is guaranteed to reset in a `finally` block even if the API
    throws — never leave the UI stuck on a spinner after an exception.

If any of the above steps is skipped or fails, the form is not considered complete —
fix it before moving on, don't wait for the user to catch it in manual testing.

## 22. General working style / how to respond to follow-up requests

- When the user pastes a screenshot + API/model code and says "create this page",
  build a **complete, working `.razor` file** in one shot following all the above
  conventions — don't ask clarifying questions unless something is truly ambiguous;
  instead, make the most sensible assumption and **flag it explicitly** in a code
  comment block at the top of `@code`, e.g.:
```csharp
  // ---------------------------------------------------------------------
  // ASSUMPTIONS (adjust if different in your project):
  // - ResponseModel.SomeField assumed to be X — rename if different.
  // - Endpoint "Controller/Action" not explicitly given — swap if wrong.
  // ---------------------------------------------------------------------
```
- Every Add/Edit page built from scratch must include the §18–21 validation/testing
  conventions by default (model attributes + matching `ValidationMessage`s + the
  correct submit pattern from §19 + the alert table from §20) — these are not
  optional extras to be added only when asked; they are part of what "complete" means
  for any form page.
- When the user pastes back a corrected/edited version of a file, treat it as the
  new source of truth and apply only the delta they're asking for on top of it —
  don't regenerate unrelated parts from scratch.
- When the user says "only send this code" / "send only this code" / "get only
  this code" — reply with **just the requested snippet**, no full file, no
  explanation preamble, no "Here's..." — just the code block(s), optionally a very
  short note on what changed if genuinely useful.
- Common Hinglish phrases and what they mean:
  - "aise hi rakho" = keep it as-is / don't change
  - "complete correct karo" = finish and fix this properly
  - "bind karo" = wire up the data binding
  - "call karo" = call this API
  - "confirm lagao" = add a confirm dialog
  - "button add only" / "sirf button" = just add the button, stub the handler
    (`// TODO: no endpoint specified yet`), don't invent an API
  - "poup" = popup
  - "seacrh"/"seach" = search
  - "grid me" = in the grid
  - "sab popup per kar do" = move everything else into the popup, keep only the
    named fields in the page header
  - "validation lagao" / "check lagao" = add the §18–21 validation conventions to
    this specific form if not already present
- Files are created directly via `create_file` into `/mnt/user-data/outputs/`, then
  shown via `present_files`. For edits to an existing file, prefer `str_replace`;
  if a large restructure is needed, delete (`rm`) and recreate via `create_file`.
- Never invent an API endpoint or payload shape the user hasn't given — if it's
  missing, wire the button/click handler but leave the body as a `// TODO` stub
  with a one-line explanation, and ask for the endpoint only if truly blocking
  (usually just proceed with the stub and move on).

---

**End of handoff document.** Paste this at the top of a new conversation, then
continue giving instructions (screenshots, models, API code) exactly as before —
work will pick up following all of the above without needing to re-explain
anything.