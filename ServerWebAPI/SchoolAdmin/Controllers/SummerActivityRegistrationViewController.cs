using ApplicationInterface.SchoolAdmin;
using DomainModel.SchoolAdmin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.SchoolAdmin.Controllers
{
    // Summer Activity page, Registrations tab: submitted registrations of the branch
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "Admission")]
    [Authorize]
    public class SummerActivityRegistrationViewController : ControllerBase
    {
        private readonly ISummerActivityRegistrationView _service;
        private readonly ILogger<SummerActivityRegistrationViewController> _logger;

        public SummerActivityRegistrationViewController(ISummerActivityRegistrationView service, ILogger<SummerActivityRegistrationViewController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // session + activity (0 = all) + status (1 / 0 / -1 all) + payment mode ("" all, NOTPAID, UPI ...)
        [HttpPost("GetRegistrations")]
        public async Task<IActionResult> GetRegistrations([FromBody] SummerActivityRegistrationSearchModel search)
        {
            if (search == null || string.IsNullOrWhiteSpace(search.GroupCode) || string.IsNullOrWhiteSpace(search.BranchCode))
                return BadRequest(ApiResponse<List<SummerActivityRegistrationListModel>>.Fail("Group / branch not found."));

            try
            {
                var data = (await _service.GetRegistrationsAsync(search)).ToList();
                return Ok(ApiResponse<List<SummerActivityRegistrationListModel>>.Ok(data, "Data fetched successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetRegistrations failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<List<SummerActivityRegistrationListModel>>.Fail("An error occurred while loading the registrations."));
            }
        }

        // filled form of one registration
        [HttpPost("GetRegistrationDetail")]
        public async Task<IActionResult> GetRegistrationDetail([FromBody] SummerActivityRegistrationKeyModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.GroupCode) || string.IsNullOrWhiteSpace(model.BranchCode))
                return BadRequest(ApiResponse<List<SummerActivityRegistrationFieldValue>>.Fail("Group / branch not found."));

            try
            {
                var data = (await _service.GetRegistrationDetailAsync(model)).ToList();
                return Ok(ApiResponse<List<SummerActivityRegistrationFieldValue>>.Ok(data, "Data fetched successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetRegistrationDetail failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<List<SummerActivityRegistrationFieldValue>>.Fail("An error occurred while loading the registration."));
            }
        }

        // payment modes saved in the branch (filter)
        [HttpGet("GetPaymentModes")]
        public async Task<IActionResult> GetPaymentModes([FromQuery] string groupCode, [FromQuery] string branchCode)
        {
            if (string.IsNullOrWhiteSpace(groupCode) || string.IsNullOrWhiteSpace(branchCode))
                return BadRequest(ApiResponse<List<string>>.Fail("Group / branch not found."));

            try
            {
                var data = (await _service.GetPaymentModesAsync(groupCode, branchCode)).ToList();
                return Ok(ApiResponse<List<string>>.Ok(data, "Data fetched successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPaymentModes failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<List<string>>.Fail("An error occurred while loading the payment modes."));
            }
        }

        // IsValid true = activate, false = deactivate
        [HttpPost("ActiveDeactiveRegistration")]
        public async Task<IActionResult> ActiveDeactiveRegistration([FromBody] SummerActivityRegistrationStatusModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.GroupCode) || string.IsNullOrWhiteSpace(model.BranchCode))
                return BadRequest(ApiResponse<object>.Fail("Group / branch not found."));

            try
            {
                var result = await _service.ActiveDeactiveAsync(model);
                return result == 1
                    ? Ok(new ApiResponse<object> { Success = true, Code = 1, Status = 200, Message = model.IsValid ? "Registration activated successfully." : "Registration deactivated successfully." })
                    : Ok(new ApiResponse<object> { Success = false, Code = -1, Status = 404, Message = "Registration was not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ActiveDeactiveRegistration failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("An error occurred while changing the status."));
            }
        }
    }
}
