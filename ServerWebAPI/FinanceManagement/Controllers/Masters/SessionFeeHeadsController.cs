using ApplicationInterface.FinanceMNGT;
using ApplicationInterface.FinanceMNGT.FeeMNGTMasters;
using DomainModel.FinanceMNGT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.FinanceManagement.Controllers.Masters
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "FinanceManagement")]
    [Authorize]
    public class SessionFeeHeadsController : ControllerBase
    {
        private readonly ISessionFeeHeadsRepository _service;
        public SessionFeeHeadsController(ISessionFeeHeadsRepository service)
        {
            _service = service;
        }

        [HttpPost("AddOrUpdateDetSession")]
        public async Task<IActionResult> AddUpdateSession([FromBody] DetSessionModel session)
        {
            if (session == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Session data not found.",
                    Code = 400
                });
            }

            try
            {
                var returnValue = await _service.AddUpdateSession(session);

                return returnValue switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Session record already exists.",
                        Code = 0
                    }),

                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Session data inserted successfully.",
                        Code = 1
                    }),

                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Session data updated successfully.",
                        Code = 2
                    }),

                    _ => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = returnValue ?? "Unknown response from database.",
                        Code = -1
                    })
                };
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while adding or updating the session record.",
                        Code = 500,
                        Data = ex.Message
                    });
            }
        }


        [HttpPost("GetSessionFeeHead")]
        public async Task<IActionResult> GetSessionFeeHead(SearchAnyRequestModel searchAny)
        {
            try
            {
                var data = await _service.GetSessionFeeHead(searchAny);
                return Ok(new ApiResponse<IEnumerable< DetSessionModel>>
                {
                    Success=true,
                    Message="Get Data Successfully !",
                    Data=data,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = ex.Message }
                );
            }
        }


        [HttpPost("DeleteSession")]
        public async Task<IActionResult> DeleteSessionData([FromBody] int Sid)
        {
            try
            {
                if (Sid <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID")
                    );
                await _service.DeleteSessionData(Sid);
                return Ok(
                    ApiResponse<string>.Ok("Data Deleted Successfully !")
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail(ex.Message)
            );
            }
        }
    }
}