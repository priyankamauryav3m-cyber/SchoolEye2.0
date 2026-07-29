using ApplicationInterface.FinanceMNGT;
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
    public class PaymentModeController : ControllerBase
    {
        private readonly IPaymentModeRepository _service;
        public PaymentModeController(IPaymentModeRepository service)
        {
            _service = service;
        }

        [HttpPost("AddOrUpdatePaymentMode")]
        public async Task<IActionResult> AddUpdateBankAccount([FromBody] PaymentModel account)
        {
            if (account == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var returnValue = await _service.AddUpdatePaymentMode(account);
                return returnValue switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Bank(PaymentMode already exists)",
                        Code = 0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Bank(PaymentMode inserted successfully)",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Bank(PaymentMode updated successfully)",
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
                    }
                  );
            }
        }

        [HttpGet("GetBankPaymentMode")]
        public async Task<IActionResult> GetBankAccountData()
        {
            try
            {
                var data = await _service.GetPaymentModeData();
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

        [HttpPost("DeletePaymentMode")]
        public async Task<IActionResult> DeleteBankAccountData([FromBody] int pmid)
        {
            try
            {
                if (pmid <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID")
                    );
                await _service.DeletePaymentModeData(pmid);
                return Ok(
                    ApiResponse<string>.Ok("Data Deleted Successfully !")
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail(ex.Message)
                );
            }
        }
    }
}

