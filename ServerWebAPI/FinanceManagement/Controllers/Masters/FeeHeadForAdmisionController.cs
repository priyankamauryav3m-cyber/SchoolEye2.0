using ApplicationInterface.FinanceMNGT;
using DomainModel.FinanceMNGT;
using DomainModel.Resources.Resource;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MyApp.Common;

namespace ServerWebAPI.FinanceManagement.Controllers.Masters
{
    [ApiExplorerSettings(GroupName = "FinanceManagement")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FeeHeadForAdmisionController : ControllerBase
    {
        private readonly IFeeHeadForAdmisionRepository _feeHeadForAdmision;
        private readonly IStringLocalizer<Resource> _localizer;
        public FeeHeadForAdmisionController(IFeeHeadForAdmisionRepository feeHeadForAdmision, IStringLocalizer<Resource> localizer)
        {
            _feeHeadForAdmision = feeHeadForAdmision;
            _localizer = localizer;
        }
        [HttpPost]
        [Route("AddUpdateFeeHeadFor")]
        public async Task<IActionResult> AddupdateFeeHeadForAsync([FromBody] FeeHeadForAdmision feeHeadForAdmision)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<string>.Fail("Invalid Request"));
                var result = await _feeHeadForAdmision.AddUpdateFeeHeadForAdmision(feeHeadForAdmision);
                return result switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Fee(Head  already exists)",
                        Code = 0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Fee(Head inserted successfully)",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Fee(Head updated successfully)",
                        Code = 2
                    }),
                    _ => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unknown operation result"
                    })
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
        [HttpPost]
        [Route("DeleteFeeHeadFor")]
        public async Task<IActionResult> DeleteFeeHeadForAsync([FromQuery] int Faid)
        {
            try
            {
                if (Faid <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID")
                    );
                var result = await _feeHeadForAdmision.DeleteFeeHeadForAdmisionData(Faid);
                return Ok(
                   ApiResponse<string>.Ok("Data Deleted Successfully !")
               );
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
        [HttpGet]
        [Route("ListFeeHeadFor")]
        public async Task<IActionResult> ListFeeHeadForAsync()
        {
            try
            {
                var resultlist = await _feeHeadForAdmision.GetFeeHeadForAdmisionData();
                if (resultlist == null || !resultlist.Any())
                    return NotFound(ApiResponse<string>.Fail("No concession group history list found"));
                return Ok(resultlist);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
    }
}
