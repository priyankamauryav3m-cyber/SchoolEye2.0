using ApplicationInterface.FinanceMNGT;
using DomainModel.FinanceMNGT;
using DomainModel.Resources.Resource;
using Infrastructure.FinanceMNGT.FeeMNGTMasters;
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
    public class FeeCollectionPeriodConfigController : ControllerBase
    {
        private readonly IFeeCollectionPeriodConfigRepository _feeCollectionPeriodConfig;
        private readonly IStringLocalizer<Resource> _localizer;
        public FeeCollectionPeriodConfigController(IFeeCollectionPeriodConfigRepository feeCollectionPeriodConfig, IStringLocalizer<Resource> localizer)
        {
            _feeCollectionPeriodConfig = feeCollectionPeriodConfig;
            _localizer = localizer;
        }
        [HttpPost]
        [Route("AddUpdateFeeCollection")]
        public async Task<IActionResult> AddUpdateFeeCollectionAsync([FromBody] FeeCollectionPeriodConfig feeCollectionPeriodConfig)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<string>.Fail("Invalid Request"));
                var result = await _feeCollectionPeriodConfig.AddUpdateFeeCollectionPeriodConfig(feeCollectionPeriodConfig);
                return result switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Fee(Period Number already exists)",
                        Code = 0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Fee(Data inserted successfully)",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Fee(Data updated successfully)",
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
        [Route("DeleteFeeCollection")]
        public async Task<IActionResult> DeleteFeeCollectionAsync([FromQuery] int FcID)
        {
            try
            {
                if (FcID <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID")
                    );
                var result = await _feeCollectionPeriodConfig.DeleteFeeCollectionPeriodConfigData(FcID);
                return Ok(
                   ApiResponse<string>.Ok("Data Deleted Successfully !")
               );
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
        [HttpPost]
        [Route("GetFeeCollectionPeriod")]
        public async Task<IActionResult> ListFeeCollectionAsync(SearchAnyRequestModel requestModel)
        {
            try
            {
                var resultlist = await _feeCollectionPeriodConfig.GetFeeCollectionPeriodConfigData(requestModel);
                if (resultlist == null || !resultlist.Any())
                    return NotFound(ApiResponse<string>.Fail("No concession group history list found"));
                return Ok(resultlist);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
        [HttpPost("AddLateFeeConfigration")]
        public async Task<IActionResult> InsertLateFeeConfigration([FromBody] LateFeeConfigration request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Invalid Request");
                }
                int result = await _feeCollectionPeriodConfig.InsertLateFeeConfigration(request);
                return result switch
                {
                    0 => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Fee(Period Number already exists)",
                        Code = 0
                    }),
                    1 => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Fee(Data inserted successfully)",
                        Code = 1
                    }),
                    2 => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Fee(Data updated successfully)",
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
                return StatusCode(500, new
                {
                    Status = false,
                    Message = ex.Message
                });
            }
        }
        [HttpPost]
        [Route("GetLateFeeConfigList")]
        public async Task<IActionResult> GetLateFeeConfigList(LateFeeConfigData requestModel)
        {
            try
            {
                var resultlist = await _feeCollectionPeriodConfig.GetLateFeeConfigListData(requestModel);
                if (resultlist == null || !resultlist.Any())
                    return NotFound(ApiResponse<string>.Fail("No concession group history list found"));
                return Ok(resultlist);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
        [HttpPost]
        [Route("GetClassesList")]
        public async Task<IActionResult> GetClassesListData(LateFeeConfigration requestModel)
        {
            try
            {
                var resultlist = await _feeCollectionPeriodConfig.GetClassesListData(requestModel);
                if (resultlist == null || !resultlist.Any())
                    return NotFound(ApiResponse<string>.Fail("No Classes List  found"));
                return Ok(resultlist);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
        [HttpPost]
        [Route("UpdateLateFee")]
        public async Task<IActionResult> UpdateLateFeeData([FromBody] LateFeeConfigData LateFee)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<string>.Fail("Invalid Request"));
                var result = await _feeCollectionPeriodConfig.UpdateLateFeeDataData(LateFee);
                return result switch
                {
                    -1 => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Fee(Late Fee already exists)",
                        Code = 0
                    }),
                    1 => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Fee(Late Updated successfully)",
                        Code = 1
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
        [Route("ActivateDeactivateLateFeeConfig")]
        public async Task<IActionResult> ActivateDeactivateLateFeeConfig([FromBody] ActivateModal LateFee)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<string>.Fail("Invalid Request"));
                var result = await _feeCollectionPeriodConfig.ActivateDeactivateLateFeeConfig(LateFee);
                return result switch
                {
                    -1 => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Late( Fee already active.)",
                        Code = 0
                    }),
                    1 => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Fee(Activated  successfully)",
                        Code = 1
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

        [HttpGet("GetPeriodTypeName")]
        public async Task<IActionResult> PeriodType()
        {
            try
            {
                var data = await _feeCollectionPeriodConfig.GetPeriodType();
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
        [HttpPost("GetQuarterlyMonthMapping")]
        public async Task<IActionResult> GetQuarterlyMonthMapping([FromBody] SearchAnyRequestModel request)
        {
            try
            {
                var data = await _feeCollectionPeriodConfig.GetQuarterlyMonthMapping(request);

                return Ok(new ApiResponse<IEnumerable<FeeCollectionMonthMappingModel>>
                {
                    Success = true,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

    }
}
