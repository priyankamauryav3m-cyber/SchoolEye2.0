
using ApplicationInterface.FinanceMNGT;
using DomainModel.FinanceMNGT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyApp.Common;

namespace ServerWebAPI.FeeMgt.Controllers.Gaps
{
    [ApiExplorerSettings(GroupName = "FeeMgt")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MiscellaneousChallanController : ControllerBase
    {
        private readonly IMiscellaneousChallanReceiptRepository _repo;
        private readonly ILogger<MiscellaneousChallanController> _logger;

        public MiscellaneousChallanController(IMiscellaneousChallanReceiptRepository repo, ILogger<MiscellaneousChallanController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        [HttpPost]
        [Route("AddUpdateMiscChallan")]
        public async Task<IActionResult> AddUpdateMiscChallan([FromBody] MiscellaneousChallanModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<string>.Fail("Invalid Request"));

                var result = await _repo.AddUpdateMiscChallan(model);
                return result switch
                {
                    "1" => Ok(new ApiResponse<object> { Success = true, Message = "Miscellaneous Challan generated successfully.", Code = 1 }),
                    "2" => Ok(new ApiResponse<object> { Success = true, Message = "Miscellaneous Challan updated successfully.", Code = 2 }),
                    _ => Ok(new ApiResponse<object> { Success = false, Message = "Unknown operation result" })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Action}", nameof(AddUpdateMiscChallan));
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while processing your request."));
            }
        }

        [HttpPost]
        [Route("ListMiscChallan")]
        public async Task<IActionResult> ListMiscChallan([FromBody] SearchAnyRequestModel request)
        {
            try
            {
                long? studentId = request.StudentId > 0 ? request.StudentId : null;
                var list = await _repo.GetMiscChallanList(request.GroupCode!, request.BranchCode!, request.SessionId, studentId);
                return Ok(ApiResponse<IEnumerable<MiscellaneousChallanModel>>.Ok(list));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Action}", nameof(ListMiscChallan));
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while processing your request."));
            }
        }

        [HttpPost]
        [Route("AddMiscReceipt")]
        public async Task<IActionResult> AddMiscReceipt([FromBody] MiscellaneousReceiptModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<string>.Fail("Invalid Request"));

                var result = await _repo.AddMiscReceipt(model);
                return result switch
                {
                    "0" => Ok(new ApiResponse<object> { Success = false, Message = "Receipt No already exists. Please use a unique Receipt No.", Code = 0 }),
                    "1" => Ok(new ApiResponse<object> { Success = true, Message = "Miscellaneous Receipt recorded successfully.", Code = 1 }),
                    _ => Ok(new ApiResponse<object> { Success = false, Message = "Unknown operation result" })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Action}", nameof(AddMiscReceipt));
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while processing your request."));
            }
        }

        [HttpPost]
        [Route("ListMiscReceipt")]
        public async Task<IActionResult> ListMiscReceipt([FromBody] SearchAnyRequestModel request)
        {
            try
            {
                long? studentId = request.StudentId > 0 ? request.StudentId : null;
                var list = await _repo.GetMiscReceiptList(request.GroupCode!, request.BranchCode!, request.SessionId, studentId);
                return Ok(ApiResponse<IEnumerable<MiscellaneousReceiptModel>>.Ok(list));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Action}", nameof(ListMiscReceipt));
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while processing your request."));
            }
        }
    }
}
