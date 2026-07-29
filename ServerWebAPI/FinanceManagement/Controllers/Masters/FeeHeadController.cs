using ApplicationInterface.FinanceMNGT;
using DocumentFormat.OpenXml.EMMA;
using DomainModel.FinanceMNGT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using MyApp.Common;

namespace ServerWebAPI.FinanceManagement.Controllers.Masters
{
    [ApiExplorerSettings(GroupName = "FinanceManagement")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FeeHeadController : ControllerBase
    {
        private readonly IFeeHeadRepository _feeHeadRepository;
        public FeeHeadController(IFeeHeadRepository feeHeadRepository)
        {
            _feeHeadRepository = feeHeadRepository;
        }

        [HttpGet]
        [Route("GetFeeHeadList")]
        public async Task<IActionResult> GetFeeHeadList()
        {
            try
            {
                var feeHeadList = await _feeHeadRepository.GetFeeHeadData();
                if (feeHeadList == null || !feeHeadList.Any())
                    return NotFound(ApiResponse<string>.Fail("No fee heads found"));
                return Ok(feeHeadList);

            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
        [HttpPost]
        [Route("AddUpdateFeeHead")]
        public async Task<IActionResult> AddFeeHead([FromBody] FeeHeadModel feeHead)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<string>.Fail("Invalid Request"));

                var result = await _feeHeadRepository.AddUpdateFeeHead(feeHead);
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
        [Route("DeleteFeeHead")]
        public async Task<IActionResult> DeleteFeeHead([FromBody] int FeeHeadId)
        {
            try
            {
                if (FeeHeadId <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID")
                    );
                var result = await _feeHeadRepository.DeleteFeeHeadData(FeeHeadId);
                return Ok(
                   ApiResponse<string>.Ok("Data Deleted Successfully !")
               );
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
  
    }
}
