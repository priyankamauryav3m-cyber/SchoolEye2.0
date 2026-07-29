using ApplicationInterface.SchoolMaster;
using DomainModel.SchoolMaster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.Addmission.Controllers.SchoolMaster
{
    [Authorize]
    [ApiExplorerSettings(GroupName = "Admission")]
    // [Authorize(AuthenticationSchemes = "LoginV3M")]
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
   
    public class CountryController : ControllerBase
    {
        private readonly ICountryRepository _service;

        public CountryController(ICountryRepository service)
        {
            _service = service;
        }

        [HttpGet("GetCountry")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _service.GetAllAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while fetching country data.",
                    error = ex.Message
                });
            }
        }
        [HttpPost("AddOrUpdateCountry")]
        public async Task<IActionResult> AddUpdateCountry([FromBody] CountryModel objCountry)
        {
            if (objCountry == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var result = await _service.AddUpdateCountry(objCountry);   
                return result switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Country name already exists",
                        Code=0
                       
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Country saved successfully",
                        Code=1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Country updated successfully",
                        Code=2
                    }),
                    _ => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unexpected error occurred"
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

        [HttpPost("DeleteCountry")]
        public async Task<IActionResult> Delete([FromBody] int countryId)
        {
            try
            {
                if (countryId <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID"));

                await _service.DeleteCountryData(countryId);

                return Ok(
                    ApiResponse<string>.Ok("Country status changed"));
            }
            catch (Exception)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail("Something went wrong."));
            }
        }

    }
}
