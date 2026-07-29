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
    public class MapFeeTemplateWithCourseController : ControllerBase
    {
        private readonly IMapFeeTemplateWithCourseRepository _service;
        public MapFeeTemplateWithCourseController(IMapFeeTemplateWithCourseRepository service)
        {
            _service = service;
        }

        [HttpPost("GetClassWiseFeeTemplate")]
        public async Task<IActionResult> GetClassWiseFeeTemplate([FromBody] SearchAnyRequestModel request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Invalid request data.");
                if (string.IsNullOrEmpty(request.GroupCode) || string.IsNullOrEmpty(request.BranchCode))
                    return BadRequest("GroupCode and BranchCode are required.");
                var result = await _service.GetClassWiseFeeTemplate(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost("SaveOrUpdateClasswiseFeeTemplate")]
        public async Task<IActionResult> SaveOrUpdateClasswiseFeeTemplate([FromBody] ClassWiseFeeTemplateModel request)
        {
            if (request == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var returnValue = await _service.SaveOrUpdateClasswiseFeeTemplateData(request);
                return returnValue switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " check(FeeTemplate name already exists)",
                        Code = 0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " check(FeeTemplate inserted successfully)",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " check(FeeTemplate updated successfully)",
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

