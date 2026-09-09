
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
    public class JournalController : ControllerBase
    {
        private readonly IJournalRepository _repo;
        private readonly ILogger<JournalController> _logger;

        public JournalController(IJournalRepository repo, ILogger<JournalController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        [HttpPost]
        [Route("AddUpdateAccountGroup")]
        public async Task<IActionResult> AddUpdateAccountGroup([FromBody] JournalAccountGroupModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<string>.Fail("Invalid Request"));

                var result = await _repo.AddUpdateAccountGroup(model);
                return result switch
                {
                    "1" => Ok(new ApiResponse<object> { Success = true, Message = "Account Group created successfully.", Code = 1 }),
                    "2" => Ok(new ApiResponse<object> { Success = true, Message = "Account Group updated successfully.", Code = 2 }),
                    _ => Ok(new ApiResponse<object> { Success = false, Message = "Unknown operation result" })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Action}", nameof(AddUpdateAccountGroup));
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while processing your request."));
            }
        }

        [HttpPost]
        [Route("ListAccountGroup")]
        public async Task<IActionResult> ListAccountGroup([FromBody] SearchAnyRequestModel request)
        {
            try
            {
                var list = await _repo.GetAccountGroupList(request.GroupCode!, request.BranchCode!);
                return Ok(ApiResponse<IEnumerable<JournalAccountGroupModel>>.Ok(list));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Action}", nameof(ListAccountGroup));
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while processing your request."));
            }
        }

        [HttpPost]
        [Route("AddUpdateAccount")]
        public async Task<IActionResult> AddUpdateAccount([FromBody] JournalAccountModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<string>.Fail("Invalid Request"));

                var result = await _repo.AddUpdateAccount(model);
                return result switch
                {
                    "1" => Ok(new ApiResponse<object> { Success = true, Message = "Account created successfully.", Code = 1 }),
                    "2" => Ok(new ApiResponse<object> { Success = true, Message = "Account updated successfully.", Code = 2 }),
                    _ => Ok(new ApiResponse<object> { Success = false, Message = "Unknown operation result" })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Action}", nameof(AddUpdateAccount));
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while processing your request."));
            }
        }

        [HttpPost]
        [Route("ListAccount")]
        public async Task<IActionResult> ListAccount([FromBody] SearchAnyRequestModel request)
        {
            try
            {
                var list = await _repo.GetAccountList(request.GroupCode!, request.BranchCode!);
                return Ok(ApiResponse<IEnumerable<JournalAccountModel>>.Ok(list));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Action}", nameof(ListAccount));
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while processing your request."));
            }
        }

        [HttpPost]
        [Route("AddJournalVoucher")]
        public async Task<IActionResult> AddJournalVoucher([FromBody] JournalVoucherModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<string>.Fail("Invalid Request"));

                if (model.Details == null || model.Details.Count < 2)
                    return BadRequest(ApiResponse<string>.Fail("A voucher needs at least one Debit and one Credit line."));

                if (model.Details.Any(d => d.DrCr != "D" && d.DrCr != "C"))
                    return BadRequest(ApiResponse<string>.Fail("Each voucher line must be marked as Debit (D) or Credit (C)."));

                model.TotalDebit = model.Details.Where(d => d.DrCr == "D").Sum(d => d.Amount);
                model.TotalCredit = model.Details.Where(d => d.DrCr == "C").Sum(d => d.Amount);

                if (model.TotalDebit != model.TotalCredit)
                    return Ok(new ApiResponse<object> { Success = false, Message = "Voucher is unbalanced — Total Debit must equal Total Credit.", Code = 3 });

                var (returnValue, voucherId) = await _repo.AddJournalVoucher(model);
                return returnValue switch
                {
                    "1" => Ok(new ApiResponse<object> { Success = true, Message = "Journal Voucher posted successfully.", Code = 1, Data = new { VoucherId = voucherId } }),
                    "0" => Ok(new ApiResponse<object> { Success = false, Message = "Voucher No already exists.", Code = 0 }),
                    "3" => Ok(new ApiResponse<object> { Success = false, Message = "Voucher is unbalanced — Total Debit must equal Total Credit.", Code = 3 }),
                    _ => Ok(new ApiResponse<object> { Success = false, Message = "Unknown operation result" })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Action}", nameof(AddJournalVoucher));
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while processing your request."));
            }
        }

        [HttpPost]
        [Route("ListJournalVoucher")]
        public async Task<IActionResult> ListJournalVoucher([FromBody] SearchAnyRequestModel request)
        {
            try
            {
                var list = await _repo.GetJournalVoucherList(request.GroupCode!, request.BranchCode!, request.SessionId);
                return Ok(ApiResponse<IEnumerable<JournalVoucherModel>>.Ok(list));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Action}", nameof(ListJournalVoucher));
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while processing your request."));
            }
        }

        [HttpGet]
        [Route("GetJournalVoucherDetail/{voucherId:long}")]
        public async Task<IActionResult> GetJournalVoucherDetail(long voucherId)
        {
            try
            {
                if (voucherId <= 0)
                    return BadRequest(ApiResponse<string>.Fail("Invalid Voucher"));

                var list = await _repo.GetJournalVoucherDetail(voucherId);
                return Ok(ApiResponse<IEnumerable<JournalVoucherDetailModel>>.Ok(list));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Action}", nameof(GetJournalVoucherDetail));
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while processing your request."));
            }
        }
    }
}
