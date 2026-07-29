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
    public class MonthController : ControllerBase
    {
        private readonly IMonthRepository _service;
        public MonthController(IMonthRepository service)
        {
            _service = service;
        }
        [HttpPost("AddOrUpdateMonth")]
        public async Task<IActionResult> AddUpdateMonth([FromBody] MonthModel month)
        {
            if (month == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var returnValue = await _service.AddUpdateMonth(month);
                return returnValue switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Month(name already exists",
                        Code = 0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Month(Data inserted successfully !",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Month( updated successfully",
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

        [HttpGet("GetMonth")]
        public async Task<IActionResult> GetMonthData()
        {
            try
            {
                var data = await _service.GetMonthData();
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


        [HttpPost("DeleteMonth")]
        public async Task<IActionResult> DeleteMonthData([FromBody] int Sid)
        {
            try
            {
                if (Sid <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID")
                    );
                await _service.DeleteMonthData(Sid);
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
