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
    public class SummerActivityController : ControllerBase
    {
        private readonly ISummerActivity _service;
        private readonly ILogger<SummerActivityController> _logger;

        public SummerActivityController(ISummerActivity service, ILogger<SummerActivityController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // add (SAId 0) or update (SAId > 0)
        [HttpPost("AddUpdateSummerActivity")]
        public async Task<IActionResult> AddUpdateSummerActivity([FromBody] SummerActivityModel model)
        {
            if (model == null)
                return BadRequest(ApiResponse<object>.Fail("Summer activity data not found."));
            if (string.IsNullOrWhiteSpace(model.GroupCode) || string.IsNullOrWhiteSpace(model.BranchCode))
                return BadRequest(ApiResponse<object>.Fail("Group / branch not found."));

            try
            {
                var user = HttpContext.Items["User"] as UserModels;
                var loginName = !string.IsNullOrWhiteSpace(user?.LoginName) ? user.LoginName : user?.Username;
                if (!string.IsNullOrWhiteSpace(loginName))
                    model.CreatedBy = loginName;

                var result = await _service.AddUpdateSummerActivityAsync(model);
                return result switch
                {
                    1 => Ok(new ApiResponse<object> { Success = true, Code = 1, Status = 200, Message = "Summer activity added successfully." }),
                    2 => Ok(new ApiResponse<object> { Success = true, Code = 2, Status = 200, Message = "Summer activity updated successfully." }),
                    -1 => Ok(new ApiResponse<object> { Success = false, Code = -1, Status = 404, Message = "Summer activity was not found." }),
                    _ => Ok(new ApiResponse<object> { Success = false, Code = -99, Status = 500, Message = "Summer activity could not be saved. Please try again." })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddUpdateSummerActivity failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("An error occurred while saving the summer activity."));
            }
        }

        // list by session + activity (0 = all) + status (1 / 0 / -1 both)
        [HttpPost("GetSummerActivities")]
        public async Task<IActionResult> GetSummerActivities([FromBody] SummerActivitySearchModel search)
        {
            if (search == null || string.IsNullOrWhiteSpace(search.GroupCode) || string.IsNullOrWhiteSpace(search.BranchCode))
                return BadRequest(ApiResponse<List<SummerActivityModel>>.Fail("Group / branch not found."));

            try
            {
                var data = (await _service.GetSummerActivitiesAsync(search)).ToList();
                return Ok(ApiResponse<List<SummerActivityModel>>.Ok(data, "Data fetched successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSummerActivities failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<List<SummerActivityModel>>.Fail("An error occurred while loading the summer activities."));
            }
        }

        // tiny registration link of an active activity (saved in SummerActivities.Link)
        [HttpPost("GenerateLink")]
        public async Task<IActionResult> GenerateLink([FromBody] SummerActivityLinkRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.GroupCode) || string.IsNullOrWhiteSpace(request.BranchCode))
                return BadRequest(ApiResponse<SummerActivityLinkResult>.Fail("Group / branch not found."));

            try
            {
                var user = HttpContext.Items["User"] as UserModels;
                var createdBy = !string.IsNullOrWhiteSpace(user?.LoginName) ? user.LoginName : user?.Username;

                var result = await _service.GenerateLinkAsync(request, createdBy);
                var message = result.Code switch
                {
                    1 => "Url has been generated successfully.",
                    0 => "Tiny URL is not set for this branch (DefaultSettings: TinyUrl).",
                    2 => "Registration page URL is not set (appsettings: SummerActivity:RegistrationPageUrl).",
                    -1 => "Active summer activity was not found.",
                    _ => "Link could not be generated. Please try again."
                };
                return Ok(new ApiResponse<SummerActivityLinkResult>
                {
                    Success = result.Code == 1,
                    Code = result.Code,
                    Status = result.Code == 1 ? 200 : result.Code == -1 ? 404 : result.Code is 0 or 2 ? 503 : 500,
                    Message = message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GenerateLink failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<SummerActivityLinkResult>.Fail("An error occurred while generating the link."));
            }
        }

        // Excel of the searched activities (same filters as the grid), base64
        [HttpPost("ExportSummerActivities")]
        public async Task<IActionResult> ExportSummerActivities([FromBody] SummerActivitySearchModel search)
        {
            if (search == null || string.IsNullOrWhiteSpace(search.GroupCode) || string.IsNullOrWhiteSpace(search.BranchCode))
                return BadRequest(ApiResponse<string>.Fail("Group / branch not found."));

            try
            {
                var bytes = await _service.ExportSummerActivitiesAsync(search);
                return Ok(ApiResponse<string>.Ok(Convert.ToBase64String(bytes), "Excel generated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExportSummerActivities failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail("An error occurred while exporting the summer activities."));
            }
        }

        // IsValid true = activate, false = deactivate
        [HttpPost("ActiveDeactiveSummerActivity")]
        public async Task<IActionResult> ActiveDeactiveSummerActivity([FromBody] SummerActivityStatusModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.GroupCode) || string.IsNullOrWhiteSpace(model.BranchCode))
                return BadRequest(ApiResponse<object>.Fail("Group / branch not found."));

            try
            {
                var result = await _service.ActiveDeactiveSummerActivityAsync(model);
                if (result == 1)
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Code = 1,
                        Status = 200,
                        Message = model.IsValid ? "Summer activity activated successfully." : "Summer activity deactivated successfully."
                    });

                return Ok(new ApiResponse<object> { Success = false, Code = -1, Status = 404, Message = "Summer activity was not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ActiveDeactiveSummerActivity failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("An error occurred while changing the status."));
            }
        }
    }
}
