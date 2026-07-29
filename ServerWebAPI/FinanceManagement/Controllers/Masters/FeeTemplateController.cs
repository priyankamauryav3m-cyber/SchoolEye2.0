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
    public class FeeTemplateController : ControllerBase
    {
        private readonly IFeeTemplateRepository _service;
        public FeeTemplateController(IFeeTemplateRepository service)
        {
            _service = service;
        }

        [HttpPost("AddOrUpdateFeeTemplate")]
        public async Task<IActionResult> AddUpdateFeeTemplateData([FromBody] FeeTemplateModel feeTem)
        {
            if (feeTem == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var returnValue = await _service.AddUpdateFeeTemplateData(feeTem);
                return returnValue switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Fee(Template name already exists)",
                        Code = 0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Fee(Template inserted successfully)",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Fee(Template updated successfully)",
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


        [HttpGet("GetFeeTemplate")]
        public async Task<IActionResult> GetFeeTemplateData()
        {
            try
            {
                var data = await _service.GetFeeTemplateData();
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



        [HttpPost("DeleteFeeTemplate")]
        public async Task<IActionResult> DeleteFeeTemplateData([FromBody] int FeeTemplateId)
        {
            try
            {
                if (FeeTemplateId <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID")
                    );
                await _service.DeleteFeeTemplateData(FeeTemplateId);
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
