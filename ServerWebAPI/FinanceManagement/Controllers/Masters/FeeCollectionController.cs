using ApplicationInterface.FinanceMNGT;
using ApplicationInterface.FinanceMNGT.FeeMNGTMasters;
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
    public class FeeCollectionController : ControllerBase
    {
        private readonly IFeeCollectionRepository _service;
        public FeeCollectionController(IFeeCollectionRepository service)
        {
            _service = service;
        }

        [HttpPost("AddOrUpdateFeeCollection")]
        public async Task<IActionResult> AddUpdateFeeCollection([FromBody] FeeCollectionModel fee)
        {
            if (fee == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Data not found."
                });
            try
            {
                var returnValue = await _service.AddUpdateFeeCollection(fee);
                return returnValue switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = " check(Book name already exists)",
                        Code = 0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " check(Book inserted successfully)",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " check(Book updated successfully)",
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


        [HttpGet("GetFeeCollection")]
        public async Task<IActionResult> GetFeeCollectionData()
        {
            try
            {
                var data = await _service.GetFeeCollectionData();
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



        [HttpPost("DeleteFeeCollection")]
        public async Task<IActionResult> DeleteFeeCollectionData([FromBody] int Sid)
        {
            try
            {
                if (Sid <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID")
                    );
                await _service.DeleteFeeCollectionData(Sid);
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
