using ApplicationInterface.FinanceMNGT;
using DomainModel.FinanceMNGT;
using DomainModel.Resources.Resource;
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
    public class SocietyController : ControllerBase
    {
        private readonly ISocietyRepository _society;
        private readonly IStringLocalizer<Resource> _localizer;
        public SocietyController(ISocietyRepository society, IStringLocalizer<Resource> localizer)
        {
            _society = society;
            _localizer = localizer;
        }
        [HttpPost]
        [Route("AddUpdateSociety")]
        public async Task<IActionResult> AddupateSocietyAsync([FromBody] SocietyModel society)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<string>.Fail("Invalid Request"));
                var result = await _society.AddUpdateSociety(society);
                return result switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Society( Number already exists)",
                        Code = 0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Society(Data inserted successfully)",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = " Society(Data updated successfully)",
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
        [Route("DeleteSociety")]
        public async Task<IActionResult> DeleteSocietyAsync([FromBody] int Sid)
        {
            try
            {
                if (Sid <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID")
                    );
                var result = await _society.DeleteSociety(Sid);
                return Ok(
                   ApiResponse<string>.Ok("Data Deleted Successfully !")
               );
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
        [HttpGet]
        [Route("ListSociety")]
        public async Task<IActionResult> ListSocietyAsync()
        {
            try
            {
                var resultlist = await _society.GetSociety();
                if (resultlist == null || !resultlist.Any())
                    return NotFound(ApiResponse<string>.Fail("No concession group history list found"));
                return Ok(resultlist);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
    }
}
