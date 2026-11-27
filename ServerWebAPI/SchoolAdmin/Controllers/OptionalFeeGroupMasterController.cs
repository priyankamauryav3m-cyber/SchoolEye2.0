using ApplicationInterface.SchoolAdmin;
using DomainModel.SchoolAdmin;
using DomainModel.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.SchoolAdmin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "Admission")]
    [Authorize]
    public class OptionalFeeGroupMasterController : ControllerBase
    {
        private readonly IOptionalFeeGroupMaster _service;
        private readonly ILogger<OptionalFeeGroupMasterController> _logger;

        public OptionalFeeGroupMasterController(IOptionalFeeGroupMaster service, ILogger<OptionalFeeGroupMasterController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // add (SeedId 0) or update (SeedId > 0)
        [HttpPost("AddUpdateOptionalFeeGroup")]
        public async Task<IActionResult> AddUpdateOptionalFeeGroup([FromBody] OptionalFeeGroupMasterModel model)
        {
            if (model == null)
                return BadRequest(ApiResponse<object>.Fail("Optional fee group data not found."));
            if (string.IsNullOrWhiteSpace(model.GroupCode) || string.IsNullOrWhiteSpace(model.BranchCode))
                return BadRequest(ApiResponse<object>.Fail("Group / branch not found."));
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.ActivityGroupName))
                return BadRequest(ApiResponse<object>.Fail("Activity group name is required (up to 50 characters)."));

            try
            {
                var user = HttpContext.Items["User"] as UserModels;
                var loginName = !string.IsNullOrWhiteSpace(user?.LoginName) ? user.LoginName : user?.Username;
                if (!string.IsNullOrWhiteSpace(loginName))
                    model.CreatedBy = loginName;

                var result = await _service.AddUpdateOptionalFeeGroupAsync(model);
                return result switch
                {
                    1 => Ok(new ApiResponse<object> { Success = true, Code = 1, Status = 200, Message = "Optional fee group added successfully." }),
                    2 => Ok(new ApiResponse<object> { Success = true, Code = 2, Status = 200, Message = "Optional fee group updated successfully." }),
                    -1 => Ok(new ApiResponse<object> { Success = false, Code = -1, Status = 404, Message = "Optional fee group was not found." }),
                    3 => Ok(new ApiResponse<object> { Success = false, Code = 3, Status = 409, Message = "Activity group name already exists." }),
                    _ => Ok(new ApiResponse<object> { Success = false, Code = -99, Status = 500, Message = "Optional fee group could not be saved. Please try again." })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddUpdateOptionalFeeGroup failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("An error occurred while saving the optional fee group."));
            }
        }

        // list by activity group name (part) + status (1 / 0 / -1 all)
        [HttpPost("GetOptionalFeeGroups")]
        public async Task<IActionResult> GetOptionalFeeGroups([FromBody] OptionalFeeGroupMasterSearchModel search)
        {
            if (search == null || string.IsNullOrWhiteSpace(search.GroupCode) || string.IsNullOrWhiteSpace(search.BranchCode))
                return BadRequest(ApiResponse<List<OptionalFeeGroupMasterModel>>.Fail("Group / branch not found."));
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<List<OptionalFeeGroupMasterModel>>.Fail("Invalid search request."));

            try
            {
                var data = (await _service.GetOptionalFeeGroupsAsync(search)).ToList();
                return Ok(ApiResponse<List<OptionalFeeGroupMasterModel>>.Ok(data, "Data fetched successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetOptionalFeeGroups failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<List<OptionalFeeGroupMasterModel>>.Fail("An error occurred while loading the optional fee groups."));
            }
        }

        // mapped classes / fee heads of one group (Map Group popup opens with these checked)
        [HttpPost("GetMapping")]
        public async Task<IActionResult> GetMapping([FromBody] OptionalGroupMapSearchModel search)
        {
            if (search == null || string.IsNullOrWhiteSpace(search.GroupCode) || string.IsNullOrWhiteSpace(search.BranchCode))
                return BadRequest(ApiResponse<List<OptionalGroupFeeHeadMapRow>>.Fail("Group / branch not found."));
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<List<OptionalGroupFeeHeadMapRow>>.Fail("Invalid optional fee group."));

            try
            {
                var data = (await _service.GetMappingAsync(search)).ToList();
                return Ok(ApiResponse<List<OptionalGroupFeeHeadMapRow>>.Ok(data, "Data fetched successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMapping failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<List<OptionalGroupFeeHeadMapRow>>.Fail("An error occurred while loading the mapping."));
            }
        }

        // map / update / unmap the changed classes of one group (one transaction)
        [HttpPost("SaveMapping")]
        public async Task<IActionResult> SaveMapping([FromBody] OptionalGroupMapSaveModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.GroupCode) || string.IsNullOrWhiteSpace(model.BranchCode))
                return BadRequest(ApiResponse<OptionalGroupMapSaveResult>.Fail("Group / branch not found."));
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<OptionalGroupMapSaveResult>.Fail("Invalid mapping data."));

            var error = ValidateMapping(model);
            if (error != null)
                return BadRequest(ApiResponse<OptionalGroupMapSaveResult>.Fail(error));

            try
            {
                var user = HttpContext.Items["User"] as UserModels;
                var loginName = !string.IsNullOrWhiteSpace(user?.LoginName) ? user.LoginName : user?.Username;
                if (!string.IsNullOrWhiteSpace(loginName))
                    model.CreatedBy = loginName;

                var result = await _service.SaveMappingAsync(model);
                var message = result.Code switch
                {
                    1 => result.Unmapped > 0 && result.Mapped == 0 ? "Class unmapped successfully."
                         : result.Unmapped > 0 ? "Mapping saved successfully (" + result.Mapped + " mapped, " + result.Unmapped + " unmapped)."
                         : "Mapping saved successfully.",
                    -1 => "Optional fee group was not found or is deactive.",
                    -2 => "Select at least one fee head for class " + result.ClassName + ".",
                    -3 => "Invalid fee head / amount in class " + result.ClassName + ".",
                    -4 => "Till date is required for class " + result.ClassName + ".",
                    _ => "Mapping could not be saved. Please try again."
                };
                return Ok(new ApiResponse<OptionalGroupMapSaveResult>
                {
                    Success = result.Code == 1,
                    Code = result.Code,
                    Status = result.Code == 1 ? 200 : result.Code == -1 ? 404 : result.Code is -2 or -3 or -4 ? 400 : 500,
                    Message = message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveMapping failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<OptionalGroupMapSaveResult>.Fail("An error occurred while saving the mapping."));
            }
        }

        // null = ok
        private static string? ValidateMapping(OptionalGroupMapSaveModel model)
        {
            if (model.Classes == null || model.Classes.Count == 0)
                return "No class to save.";
            if (model.Classes.Any(c => string.IsNullOrWhiteSpace(c.ClassCode)))
                return "Class is required.";
            if (model.Classes.GroupBy(c => c.ClassCode!.Trim(), StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1))
                return "A class is sent more than once.";

            foreach (var cls in model.Classes)
            {
                var name = cls.ClassName ?? cls.ClassCode;
                cls.FeeHeads ??= new();
                if (cls.FeeHeads.Count == 0)
                    continue; // unmap

                if (cls.TillDate == null)
                    return "Till date is required for class " + name + ".";
                if (cls.FeeHeads.Any(h => h.FeeHeadId <= 0))
                    return "Invalid fee head in class " + name + ".";
                if (cls.FeeHeads.GroupBy(h => h.FeeHeadId).Any(g => g.Count() > 1))
                    return "A fee head is selected more than once in class " + name + ".";
                if (cls.FeeHeads.Any(h => h.Amount < 0))
                    return "Amount cannot be negative in class " + name + ".";
            }
            return null;
        }

        // IsValid true = activate, false = deactivate
        [HttpPost("ActiveDeactiveOptionalFeeGroup")]
        public async Task<IActionResult> ActiveDeactiveOptionalFeeGroup([FromBody] OptionalFeeGroupMasterStatusModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.GroupCode) || string.IsNullOrWhiteSpace(model.BranchCode))
                return BadRequest(ApiResponse<object>.Fail("Group / branch not found."));
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Invalid optional fee group."));

            try
            {
                var result = await _service.ActiveDeactiveOptionalFeeGroupAsync(model);
                if (result == 1)
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Code = 1,
                        Status = 200,
                        Message = model.IsValid ? "Optional fee group activated successfully." : "Optional fee group deactivated successfully."
                    });

                return Ok(new ApiResponse<object> { Success = false, Code = -1, Status = 404, Message = "Optional fee group was not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ActiveDeactiveOptionalFeeGroup failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("An error occurred while changing the status."));
            }
        }
    }
}
