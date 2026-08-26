using ApplicationInterface.Admin;
using DomainModel.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using ServerWebAPI.Authorization;
using System;
using System.Threading.Tasks;

namespace ServerWebAPI.Addmission.Controllers.Admin
{
    //[Authorize]
    [ApiExplorerSettings(GroupName = "Admission")]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentDisciplineController : ControllerBase
    {
        private readonly IStudentDisciplineRepository _service;

        public StudentDisciplineController(IStudentDisciplineRepository service)
        {
            _service = service;
        }
        [HttpPost("AddStudentDisciplineData")]
        public async Task<IActionResult> AddStudentDiscipline([FromBody] AddStudentDisciplineRequest request)
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
                var returnValue = await _service.AddStudentDiscipline(request);
                if (returnValue == "1")
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Discipline record added successfully.",
                        Code = 1
                    });
                }
                return Ok(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to add the discipline record. See server logs for details.",
                    Code = 0
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while adding the discipline record.",
                    Code = -1
                });
            }
        }

        [HttpPost("GetIndisciplineCommentData")]
        public async Task<IActionResult> GetIndisciplineComment([FromBody] IndisciplineCommentRequest request)
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
                var data = await _service.GetIndisciplineComment(request);
                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<IndisciplineCommentResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<IndisciplineCommentResponse>()
                    });
                }
                return Ok(new ApiResponse<IEnumerable<IndisciplineCommentResponse>>
                {
                    Success = true,
                    Message = "Indiscipline comment list retrieved successfully.",
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
                    Message = "An error occurred while fetching the indiscipline comment list.",
                    Code = -1
                });
            }
        }

        [HttpPost("GetSearchedSiblingStudentData")]
        public async Task<IActionResult> GetSearchedSiblingStudent([FromBody] SiblingStudentRequest request)
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
                var data = await _service.GetSearchedSiblingStudent(request);
                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<SiblingStudentResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<SiblingStudentResponse>()
                    });
                }
                return Ok(new ApiResponse<IEnumerable<SiblingStudentResponse>>
                {
                    Success = true,
                    Message = "Sibling student list retrieved successfully.",
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
                    Message = "An error occurred while fetching the sibling student list.",
                    Code = -1
                });
            }
        }


        [HttpPost("GetSearchedStudent")]
        public async Task<IActionResult> GetSearchedStudent([FromBody] SiblingStudentRequest request)
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
                var data = await _service.GetSearchedStudent(request);

                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "No students found.",
                        Code = 0,
                        Data = Enumerable.Empty<SiblingStudentResponse>()
                    });
                }

                return Ok(new ApiResponse<IEnumerable<SiblingStudentResponse>>
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
        [HttpPost("EnableStudentDiscipline")]
        public async Task<IActionResult> EnableStudentDiscipline([FromBody] IndisciplineCommentResponse request)
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
                var returnValue = await _service.EnableStudentDiscipline(request);
                return Ok(new ApiResponse<object>
                {
                    Success = returnValue == 2,
                    Message = returnValue == 2
                        ? "Discipline record enabled successfully." : "Failed to enable the discipline record.",
                    Code = returnValue
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while enabling the discipline record.",
                    Code = -1
                });
            }
        }

        [HttpPost("DisbleStudentDisciplineData")]
        public async Task<IActionResult> DisbleStudentDiscipline([FromBody] IndisciplineCommentResponse request)
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
                var returnValue = await _service.DisbleStudentDiscipline(request);
                return Ok(new ApiResponse<object>
                {
                    Success = returnValue == 2,
                    Message = returnValue == 2
                        ? "Failed to add the discipline record." : "See server logs for details.",
                    Code = returnValue
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while enabling the discipline record.",
                    Code = -1
                });
            }
        }

    }
}
