using ApplicationInterface.Admin;
using ApplicationInterface.SchoolMaster;
using DomainModel.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.Addmission.Controllers.Admin
{
    [Authorize]
    [ApiExplorerSettings(GroupName = "Admission")]
    [Route("api/[controller]")]
    [ApiController]
    public class ClassGenderController : ControllerBase
    {
        private readonly IClassGenderRepository _service;

        public ClassGenderController(IClassGenderRepository service)
        {
            _service = service;
        }

        [HttpPost("GetClassGenderWiseReport")]
        public async Task<IActionResult> GetClassGenderWiseReport([FromBody] GenderRequest request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found.",
                    Code = 0
                });
            }
            try
            {
                var data = await _service.GetClassGenderWiseReport(request);
                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<GenderResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<GenderResponse>()
                    });
                }
                return Ok(new ApiResponse<IEnumerable<GenderResponse>>
                {
                    Success = true,
                    Message = "Class gender wise report retrieved successfully.",
                    Code = 1,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while fetching the class gender wise report.",
                    Code = -1
                });
            }
        }
    }

}

