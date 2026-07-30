using ApplicationInterface.SchoolMaster;
using DomainModel.Admin;
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

    public class GenerationIdConfigurationController : ControllerBase
    {
        private readonly IGenerationIdConfigurationRepository _service;

        public GenerationIdConfigurationController(IGenerationIdConfigurationRepository service)
        {
            _service = service;
        }

        [HttpGet("GetGenerationIdConfiguration/{sessionId}")]
        public async Task<IActionResult> GetAll(long sessionId)
        {
            try
            {
                var data = await _service.GetAllAsync(sessionId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while fetching generation id configuration data.",
                    error = ex.Message
                });
            }
        }
        [HttpPost("AddOrUpdateGenerationIdConfiguration")]
        public async Task<IActionResult> AddUpdateGenerationIdConfiguration([FromBody] List<GenerationIdConfigurationModel> objList)
        {
            if (objList == null || !objList.Any())
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            }

            try
            {
                int result = await _service.AddUpdateGenerationIdConfiguration(objList);

                return result switch
                {
                    0 => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Code = 0,
                        Message = "Keyword already exists."
                    }),

                    1 => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Code = 1,
                        Message = "Generation Id Configuration saved successfully."
                    }),

                    2 => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Code = 2,
                        Message = "Generation Id Configuration updated successfully."
                    }),

                    _ => StatusCode(500, new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while saving data."
                    })
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("DeleteGenerationIdConfiguration")]
        public async Task<IActionResult> Delete([FromBody] int sid)
        {
            try
            {
                if (sid <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID"));

                await _service.DeleteGenerationIdConfigurationData(sid);

                return Ok(
                    ApiResponse<string>.Ok("Generation Id Configuration status changed"));
            }
            catch (Exception)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail("Something went wrong."));
            }
        }

    }
}
