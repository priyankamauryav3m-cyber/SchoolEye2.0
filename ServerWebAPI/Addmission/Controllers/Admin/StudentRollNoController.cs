using ApplicationInterface.Admin;
using DomainModel.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.Addmission.Controllers.Admin
{
    [ApiExplorerSettings(GroupName = "Admission")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentRollNoController : ControllerBase
    {
        private readonly IStudentRollNoRepository _service;

        public StudentRollNoController(IStudentRollNoRepository studentRollNoRepository)
        {
            _service = studentRollNoRepository;
        }
        [HttpPost("ViewStudentRollNoPreference")]
        public async Task<IActionResult> ViewStudentRollNoPreferenceData([FromBody] MapStudentRollNoRequest request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            }
            try
            {
                var result = await _service.ViewStudentRollNoPreference(request);
                if (result == 1)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Roll numbers mapped successfully.",
                        Code = 1
                    });
                }
                return Ok(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Roll number mapping failed. See server logs for details.",
                    Code = 0
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while mapping student roll numbers."
                });
            }
        }
     

        [HttpPost("GetSearchedStudentRollNo")]
        public async Task<IActionResult> GetSearchedStudentRollNo([FromBody] AdmSearchedStudentRequest request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            }

            try
            {
                var data = await _service.GetSearchedStudentRollNo(request);

                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "No students found.",
                        Code = 0,
                        Data = Enumerable.Empty<AdmSearchedStudentResponse>()
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while searching for students."
                });
            }
        }


    }
}
