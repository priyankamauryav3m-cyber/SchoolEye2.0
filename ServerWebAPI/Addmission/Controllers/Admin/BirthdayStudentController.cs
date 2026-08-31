using ApplicationInterface.Admin;
using DomainModel.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using ServerWebAPI.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerWebAPI.Addmission.Controllers.Admin
{
    //[Authorize]
    [ApiExplorerSettings(GroupName = "Admission")]
    [Route("api/[controller]")]
    [ApiController]
    public class BirthdayStudentController : ControllerBase
    {
        private readonly IBirthdayStudentRepository _service;

        public BirthdayStudentController(IBirthdayStudentRepository service)
        {
            _service = service;
        }

        [HttpPost("GetBirthdayStudentData")]
        public async Task<IActionResult> GetBirthdayStudent([FromBody] BirthdayStudentRequest request)
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
                var data = await _service.GetBirthdayStudent(request);
                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<BirthdayStudentResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<BirthdayStudentResponse>()
                    });
                }

                return Ok(new ApiResponse<IEnumerable<BirthdayStudentResponse>>
                {
                    Success = true,
                    Message = "Birthday student list retrieved successfully.",
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
                    Message = "An error occurred while fetching the birthday student list.",
                    Code = -1
                });
            }
        }
    }
}
