using ApplicationInterface.SchoolAdmin;
using DomainModel.SchoolAdmin;
using DomainModel.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.SchoolAdmin.Controllers
{
    [ApiController]
    [Route("api/DynamicFormController")]
    [ApiExplorerSettings(GroupName = "Admission")]
    [Authorize]
    public class DynamicFormControllerController : ControllerBase
    {
        private readonly IDynamicFormController _service;
        private readonly ILogger<DynamicFormControllerController> _logger;

        public DynamicFormControllerController(IDynamicFormController service, ILogger<DynamicFormControllerController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // add (DFCId 0) or update (DFCId > 0)
        [HttpPost("AddUpdateField")]
        public async Task<IActionResult> AddUpdateField([FromBody] DynamicFormControllerModel model)
        {
            if (model == null)
                return BadRequest(ApiResponse<object>.Fail("Field data not found."));
            if (string.IsNullOrWhiteSpace(model.GroupCode) || string.IsNullOrWhiteSpace(model.BranchCode) || model.SessionId <= 0)
                return BadRequest(ApiResponse<object>.Fail("Group / branch / session not found."));

            try
            {
                var user = HttpContext.Items["User"] as UserModels;
                var loginName = !string.IsNullOrWhiteSpace(user?.LoginName) ? user.LoginName : user?.Username;
                if (!string.IsNullOrWhiteSpace(loginName))
                    model.CreatedBy = loginName;

                var result = await _service.AddUpdateFieldAsync(model);
                return result switch
                {
                    1 => Ok(new ApiResponse<object> { Success = true, Code = 1, Status = 200, Message = "Field added successfully." }),
                    2 => Ok(new ApiResponse<object> { Success = true, Code = 2, Status = 200, Message = "Field updated successfully." }),
                    -1 => Ok(new ApiResponse<object> { Success = false, Code = -1, Status = 404, Message = "Field was not found." }),
                    -2 => Ok(new ApiResponse<object> { Success = false, Code = -2, Status = 422, Message = "Invalid field name: only letters, digits and _ are allowed." }),
                    -3 => Ok(new ApiResponse<object> { Success = false, Code = -3, Status = 422, Message = "Field name is required." }),
                    -4 => Ok(new ApiResponse<object> { Success = false, Code = -4, Status = 409, Message = "This field name already exists in this activity." }),
                    -7 => Ok(new ApiResponse<object> { Success = false, Code = -7, Status = 409, Message = "This field name is already used with another data type. Select the same data type to use it in this activity." }),
                    -8 => Ok(new ApiResponse<object> { Success = false, Code = -8, Status = 409, Message = "Data type cannot be changed: this field name is also used by another activity." }),
                    -5 => Ok(new ApiResponse<object> { Success = false, Code = -5, Status = 404, Message = "Activity was not found in this branch and session." }),
                    -6 => Ok(new ApiResponse<object> { Success = false, Code = -6, Status = 422, Message = "This field name is reserved. Use another name." }),
                    _ => Ok(new ApiResponse<object> { Success = false, Code = -99, Status = 500, Message = "Field could not be saved (the data type may not fit the saved data). Please try again." })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddUpdateField failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("An error occurred while saving the field."));
            }
        }

        // list by session + activity (0 = all) + field name + status (1 / 0 / -1 all)
        [HttpPost("GetFields")]
        public async Task<IActionResult> GetFields([FromBody] DynamicFormControllerSearchModel search)
        {
            if (search == null || string.IsNullOrWhiteSpace(search.GroupCode) || string.IsNullOrWhiteSpace(search.BranchCode))
                return BadRequest(ApiResponse<List<DynamicFormControllerModel>>.Fail("Group / branch not found."));

            try
            {
                var data = (await _service.GetFieldsAsync(search)).ToList();
                return Ok(ApiResponse<List<DynamicFormControllerModel>>.Ok(data, "Data fetched successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetFields failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<List<DynamicFormControllerModel>>.Fail("An error occurred while loading the fields."));
            }
        }

        // IsValid true = activate, false = deactivate
        [HttpPost("ActiveDeactiveField")]
        public async Task<IActionResult> ActiveDeactiveField([FromBody] DynamicFormControllerStatusModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.GroupCode) || string.IsNullOrWhiteSpace(model.BranchCode))
                return BadRequest(ApiResponse<object>.Fail("Group / branch not found."));

            try
            {
                var result = await _service.ActiveDeactiveFieldAsync(model);
                return result switch
                {
                    1 => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Code = 1,
                        Status = 200,
                        Message = model.IsValid ? "Field activated successfully." : "Field deactivated successfully."
                    }),
                    -4 => Ok(new ApiResponse<object> { Success = false, Code = -4, Status = 409, Message = "The same field name is already active in this activity. Deactivate that one first." }),
                    _ => Ok(new ApiResponse<object> { Success = false, Code = -1, Status = 404, Message = "Field was not found." })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ActiveDeactiveField failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("An error occurred while changing the status."));
            }
        }
    }
}
