
using ApplicationInterface.FinanceMNGT;
using DomainModel.FinanceMNGT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyApp.Common;
using System.Security.Claims;

namespace ServerWebAPI.FeeMgt.Controllers.Gaps
{
    [ApiExplorerSettings(GroupName = "FeeMgt")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AdjustmentReceiptController : ControllerBase
    {
        private readonly IAdjustmentReceiptRepository _repo;
        private readonly ILogger<AdjustmentReceiptController> _logger;

        public AdjustmentReceiptController(IAdjustmentReceiptRepository repo, ILogger<AdjustmentReceiptController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        [HttpPost]
        [Route("AddAdjustmentReceipt")]
        public async Task<IActionResult> AddAdjustmentReceipt([FromBody] AdjustmentReceiptModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<string>.Fail("Invalid Request"));

                var result = await _repo.AddAdjustmentReceipt(model);
                return result switch
                {
                    "1" => Ok(new ApiResponse<object> { Success = true, Message = "Adjustment recorded successfully.", Code = 1 }),
                    "0" => Ok(new ApiResponse<object> { Success = false, Message = "Adjustment Type must be Add or Deduct.", Code = 0 }),
                    _ => Ok(new ApiResponse<object> { Success = false, Message = "Unknown operation result" })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Action}", nameof(AddAdjustmentReceipt));
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while processing your request."));
            }
        }

        [HttpPost]
        [Route("ApproveAdjustmentReceipt")]
        public async Task<IActionResult> ApproveAdjustmentReceipt([FromBody] AdjustmentApprovalRequest request)
        {
            try
            {
                if (request == null || request.AdjustmentId <= 0
                    || string.IsNullOrEmpty(request.GroupCode) || string.IsNullOrEmpty(request.BranchCode))
                    return BadRequest(ApiResponse<string>.Fail("Invalid Request"));

                // The approver is the server-verified authenticated identity from the JWT —
                // never a client-supplied value — so it cannot be spoofed and the self-approval
                // check below (enforced in FEE_UspApproveAdjustmentReceipt against the stored
                // CreatedBy) cannot be bypassed by the caller claiming to be someone else.
                var approvedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(approvedBy))
                    return Unauthorized(ApiResponse<string>.Fail("Unable to identify the current user."));

                var result = await _repo.ApproveAdjustmentReceipt(
                    request.AdjustmentId, request.GroupCode, request.BranchCode, request.SessionId, approvedBy);

                return result switch
                {
                    "1" => Ok(new ApiResponse<object> { Success = true, Message = "Adjustment approved successfully.", Code = 1 }),
                    "0" => Ok(new ApiResponse<object> { Success = false, Message = "Adjustment not found.", Code = 0 }),
                    "2" => Ok(new ApiResponse<object> { Success = false, Message = "Adjustment has already been processed.", Code = 2 }),
                    "3" => Ok(new ApiResponse<object> { Success = false, Message = "You cannot approve an adjustment you created yourself. A different user must approve it.", Code = 3 }),
                    _ => Ok(new ApiResponse<object> { Success = false, Message = "Unknown operation result" })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Action}", nameof(ApproveAdjustmentReceipt));
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while processing your request."));
            }
        }

        [HttpPost]
        [Route("ListAdjustmentReceipt")]
        public async Task<IActionResult> ListAdjustmentReceipt([FromBody] SearchAnyRequestModel request)
        {
            try
            {
                long? studentId = request.StudentId > 0 ? request.StudentId : null;
                var list = await _repo.GetAdjustmentReceiptList(request.GroupCode!, request.BranchCode!, request.SessionId, studentId);
                return Ok(ApiResponse<IEnumerable<AdjustmentReceiptModel>>.Ok(list));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Action}", nameof(ListAdjustmentReceipt));
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while processing your request."));
            }
        }
    }
}
