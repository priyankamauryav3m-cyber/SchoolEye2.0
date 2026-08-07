using ApplicationInterface.SchoolMaster;
using DomainModel.SchoolMaster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.Addmission.Controllers.SchoolMaster
{
    [Authorize]
    [ApiExplorerSettings(GroupName = "Admission")]
    // [Authorize(AuthenticationSchemes = "LoginV3M")]
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]

    public class InteractionPanelController : ControllerBase
    {
        private readonly IInteractionPanelRepository _service;

        public InteractionPanelController(IInteractionPanelRepository service)
        {
            _service = service;
        }

        [HttpGet("GetInteractionPanel")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _service.GetAllAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while fetching interaction panel data.",
                    error = ex.Message
                });
            }
        }
        [HttpPost("AddOrUpdateInteractionPanel")]
        public async Task<IActionResult> AddUpdateInteractionPanel([FromBody] InteractionPanelModel objPanel)
        {
            if (objPanel == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var result = await _service.AddUpdateInteractionPanel(objPanel);
                return result switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Interaction Panel already exists",
                        Code = 0

                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Interaction Panel saved successfully",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Interaction Panel updated successfully",
                        Code = 2
                    }),
                    _ => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unexpected error occurred"
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

        [HttpPost("DeleteInteractionPanel")]
        public async Task<IActionResult> Delete([FromBody] int pid)
        {
            try
            {
                if (pid <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID"));

                await _service.DeleteInteractionPanelData(pid);

                return Ok(
                    ApiResponse<string>.Ok("Interaction Panel status changed"));
            }
            catch (Exception)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail("Something went wrong."));
            }
        }

    }
}
