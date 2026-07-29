using ApplicationInterface.FinanceMNGT;
using DomainModel;
using DomainModel.FinanceMNGT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
namespace ServerWebAPI.FinanceManagement.Controllers.Masters
{
    [ApiExplorerSettings(GroupName = "FinanceManagement")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChequeBookController : Controller
    {
        private readonly IChequeBookRepository _service;
        public ChequeBookController(IChequeBookRepository service)
        {
            _service = service;
        }

        [HttpPost("AddOrUpdateCheckBook")]
        public async Task<IActionResult> AddUpdateCheckBook([FromBody] ChequeBookModel check)
        {
            if (check == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var returnValue = await _service.AddUpdateCheckBook(check);
                return returnValue switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " check(Book name already exists)",
                        Code = 0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " check(Book inserted successfully)",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " check(Book updated successfully)",
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

        [HttpGet("GetCheckBook")]
        public async Task<IActionResult> GetCheckBookData()
        {
            try
            {
                var data = await _service.GetCheckBookData();
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
        public async Task<IActionResult> DeleteFINBankData([FromBody] int CheqBookId)
        {
            try
            {
                if (CheqBookId <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID")
                    );
                await _service.DeleteFINBankData(CheqBookId);
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
