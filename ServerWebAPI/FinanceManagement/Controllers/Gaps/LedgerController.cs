
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
    public class LedgerController : ControllerBase
    {
        private readonly ILedgerRepository _repo;
        private readonly ILogger<LedgerController> _logger;

        public LedgerController(ILedgerRepository repo, ILogger<LedgerController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        [HttpPost]
        [Route("GetStudentLedger")]
        public async Task<IActionResult> GetStudentLedger([FromBody] StudentLedgerRequestModel request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<string>.Fail("Please select a Student."));

                var list = await _repo.GetStudentLedger(request);
                if (list == null || !list.Any())
                    return Ok(ApiResponse<IEnumerable<StudentLedgerEntryModel>>.Ok(list ?? Enumerable.Empty<StudentLedgerEntryModel>(),
                        "No ledger transactions found for this student.", code: 0));

                return Ok(ApiResponse<IEnumerable<StudentLedgerEntryModel>>.Ok(list));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Action}", nameof(GetStudentLedger));
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while processing your request."));
            }
        }

        [HttpPost]
        [Route("GetFeeHeadLedger")]
        public async Task<IActionResult> GetFeeHeadLedger([FromBody] FeeHeadLedgerRequestModel request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<string>.Fail("Please select a Fee Head."));

                var list = await _repo.GetFeeHeadLedger(request);
                if (list == null || !list.Any())
                    return Ok(ApiResponse<IEnumerable<FeeHeadLedgerEntryModel>>.Ok(list ?? Enumerable.Empty<FeeHeadLedgerEntryModel>(),
                        "No ledger transactions found for this Fee Head.", code: 0));

                return Ok(ApiResponse<IEnumerable<FeeHeadLedgerEntryModel>>.Ok(list));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Action}", nameof(GetFeeHeadLedger));
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while processing your request."));
            }
        }
    }
}
