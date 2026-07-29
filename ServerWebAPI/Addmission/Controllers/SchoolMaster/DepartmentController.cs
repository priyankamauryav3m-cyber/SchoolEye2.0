using ApplicationInterface.SchoolMaster;
using DomainModel.SchoolMaster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.Addmission.Controllers.SchoolMaster
{
    [ApiExplorerSettings(GroupName = "Admission")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentRepository _service;

        public DepartmentController(IDepartmentRepository service)
        {
            _service = service;
        }
        [HttpGet("GetDepartment")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _service.GetAllDepartmentAsync();
                return Ok(data);
            }
            catch (Exception)
            {
                return StatusCode(500, "Something went wrong.");
            }
        }

        [HttpPost("AddOrUpdateDepartMent")]
        public async Task<IActionResult> AddUpdateDepartMent([FromBody] DepartmentModel department)
        {
            if (department == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var returnValue = await _service.AddUpdateDepartment(department);
                return returnValue switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Department( name already exists",
                        Code=0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Department( added successfully",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Department( updated successfully",
                        Code = 2
                    }),
                    _ => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unknown operation result"
                    })
                };
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while adding or updating record."
                    });
            }
        }
        [HttpPost("DeleteDepartment")]
        public async Task<IActionResult> Delete([FromBody] int Id)
        {
            try
            {
                if (Id <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID"));
                await _service.DeleteDepartmentAsync(Id);
                return Ok(
                    ApiResponse<string>.Ok("Department status changed"));
            }
            catch (Exception)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail("Something went wrong."));
            }
        }
    }
}
