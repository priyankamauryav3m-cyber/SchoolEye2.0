using ApplicationInterface.SuperAdmin;
using DocumentFormat.OpenXml.EMMA;
using DomainModel.Admin;
using Infrastructure.SuperAdmin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MyApp.Common;

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
        public SuperAdminModuleController(ISuperAdmin repo)
        {
            _repo = repo;
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
