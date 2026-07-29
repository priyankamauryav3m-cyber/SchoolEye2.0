using ApplicationInterface.SchoolMaster;
using DomainModel.Admin;
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
    public class GenderController : ControllerBase
    {
        private readonly IGenderRepository _service;

        public GenderController(IGenderRepository service)
        {
            _service = service;
        }

        [HttpGet("GetGender")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _service.GetAllAsync();
                return Ok(data);
            }
            catch (Exception)
            {
                return StatusCode(500, "Something went wrong.");
            }
        }
        [HttpPost("DeleteGender")]
        public async Task<IActionResult> Delete([FromBody] int GenderId)
        {
            try
            {
                if (GenderId <= 0)
                    return BadRequest(ApiResponse<string>.Fail("Invalid ID"));

                await _service.DeleteAsync(GenderId);

                return Ok(ApiResponse<string>.Ok("Gender status changed"));
            }
            catch (Exception)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail("Something went wrong."));
            }
        }
        [HttpPost("AddOrUpdateGender")]
        public async Task<IActionResult> AddUpdateGender([FromBody] GenderModal objgender)
        {
            if (objgender == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                    
                });
            try
            {
                var returnValue = await _service.AddUpdateGender(objgender);
                return returnValue switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Gender name already exists",
                        Code=0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Gender added successfully",
                        Code=1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Gender updated successfully",
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
