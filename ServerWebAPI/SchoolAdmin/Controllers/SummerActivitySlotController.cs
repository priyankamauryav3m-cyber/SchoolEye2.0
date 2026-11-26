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
    public class SummerActivitySlotController : ControllerBase
    {
        private readonly ISummerActivitySlot _service;
        private readonly ILogger<SummerActivitySlotController> _logger;

        public SummerActivitySlotController(ISummerActivitySlot service, ILogger<SummerActivitySlotController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // add (SASId 0) or update (SASId > 0)
        [HttpPost("AddUpdateSlot")]
        public async Task<IActionResult> AddUpdateSlot([FromBody] SummerActivitySlotModel model)
        {
            if (model == null)
                return BadRequest(ApiResponse<object>.Fail("Slot data not found."));
            if (string.IsNullOrWhiteSpace(model.GroupCode) || string.IsNullOrWhiteSpace(model.BranchCode) || model.SessionId <= 0)
                return BadRequest(ApiResponse<object>.Fail("Group / branch / session not found."));

            try
            {
                var user = HttpContext.Items["User"] as UserModels;
                var loginName = !string.IsNullOrWhiteSpace(user?.LoginName) ? user.LoginName : user?.Username;
                if (!string.IsNullOrWhiteSpace(loginName))
                    model.CreatedBy = loginName;

                var result = await _service.AddUpdateSlotAsync(model);
                return result switch
                {
                    1 => Ok(new ApiResponse<object> { Success = true, Code = 1, Status = 200, Message = "Slot added successfully." }),
                    2 => Ok(new ApiResponse<object> { Success = true, Code = 2, Status = 200, Message = "Slot updated successfully." }),
                    -1 => Ok(new ApiResponse<object> { Success = false, Code = -1, Status = 404, Message = "Slot was not found." }),
                    -2 => Ok(new ApiResponse<object> { Success = false, Code = -2, Status = 404, Message = "Activity / slot was not found in this branch and session." }),
                    _ => Ok(new ApiResponse<object> { Success = false, Code = -99, Status = 500, Message = "Slot could not be saved. Please try again." })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddUpdateSlot failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("An error occurred while saving the slot."));
            }
        }

        // list by session + activity (0 = all) + slot name + status (1 / 0 / -1 both)
        [HttpPost("GetSlots")]
        public async Task<IActionResult> GetSlots([FromBody] SummerActivitySlotSearchModel search)
        {
            if (search == null || string.IsNullOrWhiteSpace(search.GroupCode) || string.IsNullOrWhiteSpace(search.BranchCode))
                return BadRequest(ApiResponse<List<SummerActivitySlotModel>>.Fail("Group / branch not found."));

            try
            {
                var data = (await _service.GetSlotsAsync(search)).ToList();
                return Ok(ApiResponse<List<SummerActivitySlotModel>>.Ok(data, "Data fetched successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSlots failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<List<SummerActivitySlotModel>>.Fail("An error occurred while loading the slots."));
            }
        }

        // IsValid true = activate, false = deactivate
        [HttpPost("ActiveDeactiveSlot")]
        public async Task<IActionResult> ActiveDeactiveSlot([FromBody] SummerActivitySlotStatusModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.GroupCode) || string.IsNullOrWhiteSpace(model.BranchCode))
                return BadRequest(ApiResponse<object>.Fail("Group / branch not found."));

            try
            {
                var result = await _service.ActiveDeactiveSlotAsync(model);
                if (result == 1)
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Code = 1,
                        Status = 200,
                        Message = model.IsValid ? "Slot activated successfully." : "Slot deactivated successfully."
                    });

                return Ok(new ApiResponse<object> { Success = false, Code = -1, Status = 404, Message = "Slot was not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ActiveDeactiveSlot failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("An error occurred while changing the status."));
            }
        }
    }
}
