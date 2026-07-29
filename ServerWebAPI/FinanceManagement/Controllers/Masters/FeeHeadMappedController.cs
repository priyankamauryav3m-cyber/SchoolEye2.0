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
    public class FeeHeadMappedController : ControllerBase
    {
        private readonly IFeeHeadMappedRepository _service;
        public FeeHeadMappedController(IFeeHeadMappedRepository service)
        {
            _service = service;
        }

        [HttpPost("AddOrUpdatefeeheadMapped")]
        public async Task<IActionResult> AddUpdateFeeheadMapped([FromBody] ClassFeeHeadMappedModel mapped)
        {
            if (mapped == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var returnValue = await _service.AddUpdateFeeheadMapped(mapped);
                return returnValue switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " FeeHead(name already exists)",
                        Code = 0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " FeeHead(inserted successfully)",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " FeeHead(updated successfully)",
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



        [HttpGet("GetfeeheadMapped")]
        public async Task<IActionResult> GetfeeheadMappedData()
        {
            try
            {
                var data = await _service.GetfeeheadMappedData();
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
        [HttpPost("DeletefeeheadMapped")]
        public async Task<IActionResult> DeletefeeheadMappedData([FromBody] int ClassFeeId)
        {
            try
            {
                if (ClassFeeId <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID")
                    );
                await _service.DeletefeeheadMappedData(ClassFeeId);
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
