using ApplicationInterface.FinanceMNGT;
using DomainModel.FinanceMNGT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
namespace ServerWebAPI.FinanceManagement.Controllers.Masters
{
    [ApiExplorerSettings(GroupName = "FinanceManagement")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TransportFeeController : ControllerBase
    {

        private readonly ITransportFeeConfigRepository _service;
        public TransportFeeController(ITransportFeeConfigRepository service)
        {
            _service = service;
        }

        [HttpPost("AddOrUpdateTransport")]
        public async Task<IActionResult> AddOrUpdateTransportData([FromBody] TransportFeeConfig transport)
        {
            if (transport == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var returnValue = await _service.AddOrUpdateTransportData(transport);
                return returnValue switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Transport(name already exists)",
                        Code = 0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Transport(Data inserted successfully)",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Transport(Data updated successfully)",
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

        [HttpGet("GetMapAmountDistance")]
        public async Task<IActionResult> GetDistance(long SessionId)
        {
            try
            {
                var data = await _service.GetDistanceMapAmount(SessionId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GetTransport")]
        public async Task<IActionResult> GetTransporData()
        {
            try
            {
                var data = await _service.GetTransporData();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = ex.Message }
                );
            }
        }


        [HttpPost("DeleteTransport")]
        public async Task<IActionResult> DeleteTransportData([FromBody] int Tid)
        {
            try
            {
                if (Tid <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID")
                    );
                await _service.DeleteTransportData(Tid);
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

