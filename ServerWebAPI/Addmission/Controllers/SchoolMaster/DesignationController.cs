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
    public class DesignationController : ControllerBase
    {
        private readonly IDesignationRepository _service;

        public DesignationController(IDesignationRepository service)
        {
            _service = service;
        }
        [HttpGet("GetDesignation")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllDesignationAsync();
            return Ok(data);
        }


        [HttpPost("DeleteDesignation")]
        public async Task<IActionResult> Delete([FromBody] int Id)
        {
            if (Id <= 0)
                return BadRequest(
                    ApiResponse<string>.Fail("Invalid ID"));

            await _service.DeleteDesignationAsync(Id);

            return Ok(
                ApiResponse<string>.Ok("Designation status changed"));
        }

        [HttpPost("AddOrUpdateDesignation")]
        public async Task<IActionResult> AddUpdateDesignation([FromBody] DesignationModel objDesignation)
        {
            if (objDesignation == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var returnValue = await _service.AddUpdateDesignation(objDesignation);
                return returnValue switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Designation name already exists",
                        Code=0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Designation added successfully",
                        Code=1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Designation updated successfully",
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
