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
    public class StateController : ControllerBase
    {
        private readonly IStateRepository _service;

        public StateController(IStateRepository service)
        {
            _service = service;
        }

        // ================= GET =================
        [HttpGet("GetState")]
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

        [HttpPost("AddOrUpdateState")]
        public async Task<IActionResult> AddUpdateState([FromBody] StateModel objState)
        {
            if (objState == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var returnValue = await _service.AddUpdateState(objState);
                return returnValue switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "State name already exists",
                        Code=0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "State added successfully",
                        Code=1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "State updated successfully",
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

        // ================= DELETE =================
        [HttpPost("DeleteState")]
        public async Task<IActionResult> Delete([FromBody] int stateId)
        {
            try
            {
                if (stateId <= 0)
                    return BadRequest(ApiResponse<string>.Fail("Invalid ID"));

                await _service.DeleteStateData(stateId);
                return Ok(ApiResponse<string>.Ok("State status changed"));
            }
            catch (Exception)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail("Something went wrong."));
            }
        }
    }
}
