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
    public class DisabilityTypeController : ControllerBase
    {
        private readonly IDisabilityTypeRepository _service;

        public DisabilityTypeController(IDisabilityTypeRepository service)
        {
            _service = service;
        }

        [HttpPost("AddOrUpdateDisabilityType")]
        public async Task<IActionResult> AddUpdateDisabilityType([FromBody] DisabilityTypeModel objdisabilitytype)
        {
            if (objdisabilitytype == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var returnValue = await _service.AddUpdateDisabilityType(objdisabilitytype);
                return returnValue switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Disability type already exists",
                        Code = 0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Disability type added successfully",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Disability type updated successfully",
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

        [HttpPost("DeleteOrDisabilityType")]
        public async Task<IActionResult> DeleteDisabilityType([FromBody] int DisabilityTypeId)
        {
            try
            {
                if (DisabilityTypeId <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID"));

                await _service.DeleteDisabilityType(DisabilityTypeId);
                return Ok(
                    ApiResponse<string>.Ok("Disability type status changed"));
            }
            catch (Exception)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail("Something went wrong."));
            }
        }

        [HttpGet("GetAllDisabilityType")]
        public async Task<IActionResult> GetDisabilityType()
        {
            try
            {
                var data = await _service.GetDisabilityType();
                return Ok(data);
            }
            catch (Exception)
            {
                return StatusCode(500, "Something went wrong.");
            }
        }
    }
}
