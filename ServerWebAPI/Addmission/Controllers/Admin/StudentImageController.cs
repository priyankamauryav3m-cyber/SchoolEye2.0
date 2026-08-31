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
    public class StudentImageController : ControllerBase
    {
        private readonly IStudentImageRepository _service;

        public StudentImageController(IStudentImageRepository service)
        {
            _service = service;
        }

        [HttpPost("GetStudentImage")]
        public async Task<IActionResult> GetStudentImageData([FromBody] StudentImageRequest request)
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
                var data = await _service.GetStudentImageData(request);
                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<StudentImageResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<StudentImageResponse>()
                    });
                }
                return Ok(new ApiResponse<IEnumerable<StudentImageResponse>>
                {
                    Success = true,
                    Message = "Student list retrieved successfully.",
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
                    Message = "An error occurred while searching for students.",
                    Code = -1
                });
            }
        }
    }
}
