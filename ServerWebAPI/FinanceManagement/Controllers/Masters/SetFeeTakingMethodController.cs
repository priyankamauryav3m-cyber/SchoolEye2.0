using ApplicationInterface.FinanceMNGT.FeeMNGTMasters;
using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.FinanceManagement.Controllers.Masters
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "FinanceManagement")]
    [Authorize]
    public class SetFeeTakingMethodController : ControllerBase
    {
        private readonly ISetFeeTakingMethodRepository _service;
        public SetFeeTakingMethodController(ISetFeeTakingMethodRepository service)
        {
            _service = service;
        }
        [HttpPost("GetFeeHeadsOfTemplate")]
        public async Task<IActionResult> GetFeeHeadTemplate([FromBody] SearchAnyRequestModel request)
        {

            try
            {
                var result = await _service.GetFeeHeadsOfTemplateData(request);
                return Ok(new ApiResponse<IEnumerable<dynamic>>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                 //   Message = _localizer["Error"]
                });
            }
        }

        //[HttpPost("SetFeeTakingMethod")]
        //public async Task<IActionResult> SaveFeeCollectionConfig([FromBody] FeeTakingMethod method)
        //{
        //    try
        //    {
        //        if (method == null)
        //            return BadRequest("Invalid request data.");
        //        var result = await _service.SaveFeeCollectionConfig(method);
        //        if (result.Status)
        //            return Ok(result);
        //        return BadRequest(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new
        //        {
        //            Message = "An error occurred while saving fee collection config.",
        //            Error = ex.Message
        //        });
        //    }
        //}

        [HttpPost("SaveFeeTakingMethod")]
        public async Task<IActionResult> SaveFeeCollectionConfig([FromBody] FeeTakingMethod method)
        {
            if (method == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var returnValue = await _service.SaveFeeCollectionConfig(method);
                return returnValue switch
                {
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " FeeTaking(Data updated successfully)",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " FeeTaking(Data inserted successfully)",
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
    }
}
