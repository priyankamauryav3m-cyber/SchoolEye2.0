using ApplicationInterface.FinanceMNGT;
using ApplicationInterface.SchoolMaster;
using DomainModel.FinanceMNGT;
using DomainModel.SchoolMaster;
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
    public class FinanceController : ControllerBase
    {
        private readonly IFINBankRepository _service;
        public FinanceController(IFINBankRepository service)
        {
            _service = service;
        }

        [HttpPost("AddOrUpdateFINBank")]
        public async Task<IActionResult> AddUpdateFINBank([FromBody] BankModel bank)
        {
            if (bank == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var returnValue = await _service.AddUpdateFINBank(bank);
                return returnValue switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Bank( name already exists",
                        Code = 0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Bank( added successfully",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Bank( updated successfully",
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

        [HttpGet("GetBank")]
        public async Task<IActionResult> GetBankData()
        {
            try
            {
                var data = await _service.GetBankData();
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

        [HttpPost("DeleteFINBank")]
        public async Task<IActionResult> DeleteFINBankData([FromBody] int BankId)
        {
            try
            {
                if (BankId <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID")
                    );
                await _service.DeleteFINBankData(BankId);
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
