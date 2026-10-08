using ApplicationInterface.SuperAdmin;
using DocumentFormat.OpenXml.EMMA;
using DomainModel.Admin;
using Infrastructure.SuperAdmin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MyApp.Common;
using System.Security.Claims;
using ServerWebAPI.Dependency;
using V3MAdminOnlyAttribute = ServerWebAPI.Authorization.V3MAdminOnlyAttribute;
using ApplicationInterface.User;

namespace ServerWebAPI.Addmission.Controllers.Admin
{
    [ApiExplorerSettings(GroupName = "Admission")]
    [Authorize]
    //[EnableRateLimiting("V3MAPI_Call_Limit")]
    [Route("api/[controller]")]
    [ApiController]
    public class SuperAdminModuleController : ControllerBase
    {
        private readonly ISuperAdmin _repo;
        private readonly IUser _users;
        public SuperAdminModuleController(ISuperAdmin repo, IUser users)
        {
            _repo = repo;
            _users = users;
        }

        // UI page guard: is the logged-in user (from JWT) a V3M Admin?
        [HttpGet("IsV3MAdmin")]
        public async Task<IActionResult> IsV3MAdmin()
        {
            var userSid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userSid))
                return Unauthorized(ApiResponse<string>.Fail("Unable to identify the current user."));
            try
            {
                bool isAdmin = await _users.IsUserInRoleAsync(userSid, V3MAdminOnlyAttribute.RoleName);
                return Ok(ApiResponse<bool>.Ok(isAdmin));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail("Something went wrong."));
            }
        }
        // Module  Data 
        [HttpPost("Add_Module")]
        public async Task<IActionResult> AddModule(SuperAdminModule module)
        {
            try
            {
                int result = await _repo.AddModuleData(module);

                if (result == -1)
                {
                    return BadRequest(
                        ApiResponse<string>.Fail("Module already exists!")
                    );
                }

                if (result > 0)
                {
                    return Ok(
                        ApiResponse<int>.Ok(result, "Module added successfully!")
                    );
                }

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail("Failed to add module")
                );
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail("Something went wrong.")
                );
            }
        }

        [HttpGet]
        [Route("GetAdd_Module")]
        public async Task<IActionResult> GetAddModule()
        {
            try
            {
                var result = await _repo.GetAddModuleData();

                if (result == null || !result.Any())
                {
                    return NotFound(
                        ApiResponse<string>.Fail("No modules found")
                    );
                }

                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail("Something went wrong.")
                );
            }
        }
        [HttpPost("Add_ModuleEdit")]
        public async Task<IActionResult> AddModuleEdit(SuperAdminModule module)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    ApiResponse<string>.Fail("Invalid module data")
                );
            }
            try
            {
                int result = await _repo.AddModuleEditData(module);
                if (result > 0)
                {
                    return Ok(
                        ApiResponse<int>.Ok(result, "Module updated successfully!")
                    );
                }
                if (result == 0)
                {
                    return NotFound(
                        ApiResponse<string>.Fail("Module not found")
                    );
                }
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail("Database error while updating module")
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail(ex.Message)
                );
            }
        }

        [HttpPost("Add_ModuleDelete")]
        public async Task<IActionResult> AddModuleDelete([FromBody] int ModuleId)
        {
            try
            {
                int result = await _repo.AddModuleDeleteData(ModuleId);

                if (result == 0)
                {
                    return NotFound(
                        ApiResponse<string>.Fail("Record not found")
                    );
                }

                return Ok(
                    ApiResponse<int>.Ok(result, "Module deleted successfully")
                );
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail("Something went wrong.")
                );
            }
        }

        //  Features 
        [HttpPost("Add_Features")]
        public async Task<IActionResult> AddFeatures(SuperAdminFeatures features)
        {
            try
            {
                int result = await _repo.AddFeaturesData(features);

                if (result == -1)
                {
                    return BadRequest(
                        ApiResponse<string>.Fail("Features already exists!")
                    );
                }

                if (result > 0)
                {
                    return Ok(
                        ApiResponse<int>.Ok(result, "Features added successfully!")
                    );
                }

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail("Failed to add module")
                );
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail("Something went wrong.")
                );
            }
        }

        [HttpPost("Add_FeaturesEdit")]
        public async Task<IActionResult> AddFeaturesEdit(SuperAdminFeatures features)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    ApiResponse<string>.Fail("Invalid module data")
                );
            }
            try
            {
                int result = await _repo.AddFeaturesEditData(features);
                if (result > 0)
                {
                    return Ok(
                        ApiResponse<int>.Ok(result, "Module updated successfully!")
                    );
                }
                if (result == 0)
                {
                    return NotFound(
                        ApiResponse<string>.Fail("Module not found")
                    );
                }
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail("Database error while updating module")
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail(ex.Message)
                );
            }
        }

        [HttpGet("GetByModule/{moduleId}")]
        public async Task<IActionResult> GetAddFeatures(int moduleId)
        {
            try
            {
                if (moduleId <= 0)
                {
                    return BadRequest("Invalid moduleId.");
                }
                var result = await _repo.GetAddFeaturesData(moduleId);
                if (result == null)
                {
                    return NotFound("No data found for the given moduleId.");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("Add_FeaturesDelete")]
        public async Task<IActionResult> AddFeaturesDelete([FromBody] int features)
        {
            try
            {
                if (features <= 0)
                {
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid feature id.")
                    );
                }
                int result = await _repo.AddFeaturesDeleteData(features);
                if (result == 0)
                {
                    return NotFound(
                        ApiResponse<string>.Fail("Record not found")
                    );
                }
                return Ok(
                    ApiResponse<int>.Ok(result, "Feature deleted successfully")
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail("An unexpected error occurred.")
                );
            }
        }

        // Activity
        [HttpPost("Add_Activity")]
        public async Task<IActionResult> AddActivity(SuperAdminActivity activity)
        {
            try
            {
                int result = await _repo.AddActivityData(activity);
                if (result == -1)
                {
                    return BadRequest(
                        ApiResponse<string>.Fail("Features already exists!")
                    );
                }
                if (result > 0)
                {
                    return Ok(
                        ApiResponse<int>.Ok(result, "Features added successfully!")
                    );
                }
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail("Failed to add module")
                );
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail("Something went wrong.")
                );
            }
        }

        // Activity page: returns 1 in Data when the activity is added
        [V3MAdminOnly]
        [HttpPost("InsertMstActivityListNew")]
        [ApiResponseValidation]
        public async Task<IActionResult> InsertMstActivityListNew([FromBody] ActivityCreateRequest activity)
        {
            if (activity == null)
                return BadRequest(ApiResponse<string>.Fail("Invalid activity data."));

            // audit user from the verified JWT, never from the client
            var createdBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(createdBy))
                return Unauthorized(ApiResponse<string>.Fail("Unable to identify the current user."));

            try
            {
                int result = await _repo.InsertMstActivityListNew(activity, createdBy);
                return result switch
                {
                    1 => Ok(ApiResponse<int>.Ok(1, "Activity added successfully.")),
                    0 => Conflict(ApiResponse<string>.Fail("Activity name already exists.")),
                    -2 => BadRequest(ApiResponse<string>.Fail("Selected feature was not found.")),
                    _ => StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail("Activity could not be added."))
                };
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail("Something went wrong.")
                );
            }
        }

        [HttpGet("GetActivity/{FeatureId}")]
        public async Task<IActionResult> GetAddActivity(int FeatureId)
        {
            try
            {
                var result = await _repo.GetAddActivityData(FeatureId);
                if (result == null || !result.Any())
                {
                    return NotFound(
                        ApiResponse<string>.Fail("No activities found")
                    );
                }
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail("Something went wrong.")
                );
            }
        }

        [V3MAdminOnly]
        [HttpGet("GetModuleTree")]
        public async Task<IActionResult> GetModuleTree()
        {
            try
            {
                var result = await _repo.GetModuleTreeData();
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail("Something went wrong.")
                );
            }
        }

        [V3MAdminOnly]
        [HttpPost("ControlMapping")]
        public async Task<IActionResult> AccessControlMapping([FromBody] List<ControlAccess> model)
        {
            try
            {
                var (inserted, updated) = await _repo.AccessControlMappingData(model);
                if (inserted > 0)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Access saved successfully",
                        ActionType = "Save"
                    });
                }
                if (updated > 0)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Access updated successfully",
                        ActionType = "Update"
                    });
                }
                return BadRequest(ApiResponse<object>.Fail("No changes detected"));
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("An unexpected error occurred.")
                );
            }
        }

        [V3MAdminOnly]
        [HttpGet("GetControlMappingByRole/{roleId}")]
        public async Task<IActionResult> GetControlMappingByRole(int roleId)
        {
            try
            {
                var data = await _repo.GetControlAccessByRole(roleId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred."
                );
            }
        }


        [HttpGet("GetRoleBased")]
        public async Task<IActionResult> RoleBasedShowRecord([FromQuery] int roleId)
        {
            try
            {
                var data = await _repo.GetRoleBasedShowRecord(roleId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred."
                );
            }
        }

        [HttpGet("RoleBasedActivity/{roleId}")]
        public async Task<IActionResult> GetRoleBasedActivity(int roleId)
        {
            try
            {
                var data = await _repo.GetRoleBasedActivity(roleId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred."
                );
            }
        }

        [V3MAdminOnly]
        [HttpPost("DeleteMapping")]
        public async Task<IActionResult> DeleteMapping([FromBody] List<int> accessIds)
        {
            try
            {
                await _repo.DeleteAccessMappings(accessIds);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Access deleted successfully"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Something went wrong."
                });
            }
        }
        // Role wise menu order
        [V3MAdminOnly]
        [HttpGet("RoleMenuOrder/{roleId}")]
        public async Task<IActionResult> GetRoleMenuOrder(int roleId)
        {
            if (roleId <= 0)
                return BadRequest(ApiResponse<string>.Fail("Invalid role."));
            try
            {
                var data = await _repo.GetRoleMenuOrder(roleId);
                return Ok(ApiResponse<IEnumerable<RoleMenuOrderRow>>.Ok(data, "Menu order fetched successfully."));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail("Something went wrong.")
                );
            }
        }

        [V3MAdminOnly]
        [HttpPost("RoleMenuOrder")]
        public async Task<IActionResult> SaveRoleMenuOrder([FromBody] RoleMenuOrderSaveRequest request)
        {
            if (request == null || !ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid menu order data."));

            if (request.Items.GroupBy(i => new { i.LevelType, i.RefId }).Any(g => g.Count() > 1))
                return BadRequest(ApiResponse<string>.Fail("Duplicate menu items in request."));

            // Audit user comes from the verified JWT, never from the client payload
            var createdBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(createdBy))
                return Unauthorized(ApiResponse<string>.Fail("Unable to identify the current user."));

            try
            {
                int result = await _repo.SaveRoleMenuOrder(request.RoleId, request.Items, createdBy);
                if (result == -1)
                    return NotFound(ApiResponse<string>.Fail("Role not found."));
                if (result == 0)
                    return BadRequest(ApiResponse<string>.Fail("No changes detected."));

                return Ok(ApiResponse<int>.Ok(result, "Menu order saved successfully."));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail("Something went wrong.")
                );
            }
        }

        [V3MAdminOnly]
        [HttpPost("RoleMenuOrder/Reset/{roleId}")]
        public async Task<IActionResult> ResetRoleMenuOrder(int roleId)
        {
            if (roleId <= 0)
                return BadRequest(ApiResponse<string>.Fail("Invalid role."));
            try
            {
                int result = await _repo.ResetRoleMenuOrder(roleId);
                return Ok(ApiResponse<int>.Ok(result, "Menu order reset to default."));
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail("Something went wrong.")
                );
            }
        }

        [HttpGet("GetDashboard")]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                var data = await _repo.GetDashboardData();
                return Ok(new ApiResponse<IEnumerable<DashboardModel>>
                {
                    Success = true,
                    Message = "Dashboard data fetched successfully.",
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Status = false,
                    Message = "An error occurred while fetching dashboard data.",
                    Error = ex.Message,
                    Data = (object)null
                });
            }
        }


    }
}
