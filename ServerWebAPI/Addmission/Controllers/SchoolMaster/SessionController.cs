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
    public class SessionController : ControllerBase
    {
        private readonly ISessionRepository _service;

        public SessionController(ISessionRepository service)
        {
            _service = service;
        }

        [HttpPost("AddOrUpdateSession")]
        public async Task<IActionResult> AddUpdateSession([FromBody] SessionModel Session)
        {
            if (Session == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var returnValue = await _service.AddUpdateSession(Session);
                return returnValue switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Session( name already exists",
                        Code = 0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Session( added successfully",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Session( updated successfully",
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

        [HttpGet("GetSession")]
        public async Task<IActionResult> GetSessionData()
        {
            try
            {
                var data = await _service.GetSessionData();

                if (data == null || !data.Any())
                {
                    return NotFound(
                        ApiResponse<string>.Fail("No Session records found")
                    );
                }
                return Ok(data);
                
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail("An error occurred while fetching Session records", ex.Message)
                );
            }
        }

        [HttpPost("DeleteSession")]
        public async Task<IActionResult> DeleteSessionData([FromBody] int SessionId)
        {
            try
            {
                if (SessionId <= 0)
                {
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID")
                    );
                }
                await _service.DeleteSessionData(SessionId);
                return Ok(
                    ApiResponse<string>.Ok("Session status changed")
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail("An error occurred while deleting the Session", ex.Message)
                );
            }
        }
    }
 }

