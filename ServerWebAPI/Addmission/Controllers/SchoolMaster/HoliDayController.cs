using ApplicationInterface.SchoolMaster;
using DomainModel.Admin;
using DomainModel.SchoolMaster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.Addmission.Controllers.SchoolMaster
{
    [ApiExplorerSettings(GroupName = "Admission")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class HoliDayController : ControllerBase
    {
        private readonly IHolidayRepository _service;

        public HoliDayController(IHolidayRepository service)
        {
            _service = service;
        }

        [HttpGet("GetHoliDay")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _service.GetAllHoliday();
                return Ok(data);
            }
            catch (Exception)
            {
                return StatusCode(500, "Something went wrong.");
            }
        }
        [HttpPost("DeleteHoliday")]
        public async Task<IActionResult> Delete([FromBody] int HoliDayId)
        {
            try
            {
                if (HoliDayId <= 0)
                    return BadRequest(ApiResponse<string>.Fail("Invalid ID"));
                await _service.DeleteHoliday(HoliDayId);
                return Ok(ApiResponse<string>.Ok("HoliDay status changed"));
            }
            catch (Exception)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail("Something went wrong."));
            }
        }
        [HttpPost("AddOrUpdateHoliDay")]
        public async Task<IActionResult> AddUpdateHoliDay([FromBody] HolidayModal objHoliDay)
        {
            if (objHoliDay == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."

                });
            try
            {
                var returnValue = await _service.AddUpdateHoliday(objHoliDay);
                return returnValue switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "HoliDay name already exists",
                        Code = 0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "HoliDay added successfully",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "HoliDay updated successfully",
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
    }
}
