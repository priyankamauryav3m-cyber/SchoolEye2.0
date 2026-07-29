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
    public class ClassController : ControllerBase
    {
        private readonly IClassRepository _service;

        public ClassController(IClassRepository service)
        {
            _service = service;
        }
        [HttpGet("GetClass")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _service.GetClassData();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while fetching class data.",
                    error = ex.Message
                });
            }
        }
        [HttpPost("DeleteClass")]
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(ApiResponse<string>.Fail("Invalid ID"));

                await _service.DeleteClassData(id);

                return Ok(ApiResponse<string>.Ok("ClassName status changed"));
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail($"An error occurred: {ex.Message}")
                );
            }
        }

        [HttpPost("AddOrUpdateClass")]
        public async Task<IActionResult> AddUpdateClass([FromBody] ClassModel objClass)
        {
            if (objClass == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var returnValue = await _service.AddUpdateClass(objClass);
                return returnValue switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Class name already exists",
                        Code=0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Class added successfully",
                        Code=1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Class updated successfully",
                        Code=2
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
    }
}
