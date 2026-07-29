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
    public class ClassCodeController : ControllerBase
    {
        private readonly IClassCodeRepository _service;


        public ClassCodeController(IClassCodeRepository service)
        {
            _service = service;
        }

        [HttpGet("GetClassCode")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(data);
        }
        
        [HttpPost("DeleteClassCode")]
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            if (id <= 0)
                return BadRequest(
                    ApiResponse<string>.Fail("Invalid ID"));

            await _service.DeleteAsync(id);

            return Ok(
                ApiResponse<string>.Ok("ClassCode status changed"));
        }

        [HttpPost("AddOrUpdateClasscode")]
        public async Task<IActionResult> AddUpdateClasscode([FromBody] ClassCodeModel objClasscode)
        {
            if (objClasscode == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var returnValue = await _service.AddUpdateClasscode(objClasscode);
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
