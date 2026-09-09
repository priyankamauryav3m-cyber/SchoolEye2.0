
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
    public class OptionalFeeGroupController : ControllerBase
    {
        private readonly IOptionalFeeGroupRepository _repo;
        private readonly ILogger<OptionalFeeGroupController> _logger;

        public OptionalFeeGroupController(IOptionalFeeGroupRepository repo, ILogger<OptionalFeeGroupController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        [HttpPost]
        [Route("AddUpdateOptionalFeeGroup")]
        public async Task<IActionResult> AddUpdateOptionalFeeGroup([FromBody] OptionalFeeGroupModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<string>.Fail("Invalid Request"));

                var result = await _repo.AddUpdateOptionalFeeGroup(model);
                return result switch
                {
                    "0" => Ok(new ApiResponse<object> { Success = false, Message = "Optional Fee Group code already exists.", Code = 0 }),
                    "1" => Ok(new ApiResponse<object> { Success = true, Message = "Optional Fee Group created successfully.", Code = 1 }),
                    "2" => Ok(new ApiResponse<object> { Success = true, Message = "Optional Fee Group updated successfully.", Code = 2 }),
                    _ => Ok(new ApiResponse<object> { Success = false, Message = "Unknown operation result" })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Action}", nameof(AddUpdateOptionalFeeGroup));
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while processing your request."));
            }
        }

        [HttpPost]
        [Route("ListOptionalFeeGroup")]
        public async Task<IActionResult> ListOptionalFeeGroup([FromBody] SearchAnyRequestModel request)
        {
            try
            {
                var list = await _repo.GetOptionalFeeGroupList(request.GroupCode!, request.BranchCode!, request.SessionId);
                return Ok(ApiResponse<IEnumerable<OptionalFeeGroupModel>>.Ok(list));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Action}", nameof(ListOptionalFeeGroup));
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while processing your request."));
            }
        }

        [HttpPost]
        [Route("ToggleOptionalFeeGroup")]
        public async Task<IActionResult> ToggleOptionalFeeGroup([FromBody] OptionalFeeGroupModel model)
        {
            try
            {
                if (model == null || model.OptionalGroupId <= 0
                    || string.IsNullOrEmpty(model.GroupCode) || string.IsNullOrEmpty(model.BranchCode))
                    return BadRequest(ApiResponse<string>.Fail("Invalid Request"));

                await _repo.ToggleOptionalFeeGroup(model);
                return Ok(ApiResponse<string>.Ok("Status updated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Action}", nameof(ToggleOptionalFeeGroup));
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while processing your request."));
            }
        }

        [HttpPost]
        [Route("MapClass")]
        public async Task<IActionResult> MapClass([FromBody] OptionalFeeGroupClassMapModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<string>.Fail("Invalid Request"));

                await _repo.MapClass(model);
                return Ok(ApiResponse<string>.Ok("Class mapped successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Action}", nameof(MapClass));
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while processing your request."));
            }
        }

        [HttpPost]
        [Route("MapFeeHead")]
        public async Task<IActionResult> MapFeeHead([FromBody] OptionalFeeGroupHeadModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<string>.Fail("Invalid Request"));

                await _repo.MapFeeHead(model);
                return Ok(ApiResponse<string>.Ok("Fee Head mapped successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Action}", nameof(MapFeeHead));
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while processing your request."));
            }
        }
    }
}
