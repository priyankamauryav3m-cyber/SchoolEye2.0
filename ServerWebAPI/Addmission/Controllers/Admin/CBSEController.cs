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
    [Authorize]
    [ApiExplorerSettings(GroupName = "Admission")]
    [Route("api/[controller]")]
    [ApiController]
    public class CBSEController : ControllerBase
    {
        private readonly IUpdateStudentCBSERegNoRepository _service;

        public CBSEController(IUpdateStudentCBSERegNoRepository service)
        {
            _service = service;
        }

        [HttpPost("GetStudentBoardRollNo")]
        public async Task<IActionResult> GetSearchedStudent([FromBody] AdmSearchedStudentRequest request)
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
                var data = await _service.GetStudentBoardRollNo(request);

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
        [HttpPost("AddUpdateStudentBoardRollNo")]
        [HttpPost("AddUpdateStudentCBSERegNo")]
        public async Task<IActionResult> AddUpdateStudentBoardData([FromBody] AddUpdateStudentBoardRollNoRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Invalid request.",
                        Data = string.Empty
                    });
                }

                if (string.IsNullOrWhiteSpace(request.StudentId))
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Student data is required.",
                        Data = string.Empty
                    });
                }

                var path = HttpContext.Request.Path.Value ?? "";

                string result;
                string message;

                if (path.EndsWith("AddUpdateStudentBoardRollNo", StringComparison.OrdinalIgnoreCase))
                {
                    result = await _service.AddUpdateStudentBoardRollNo(request);
                    message = "Board Roll Number updated successfully.";
                }
                else
                {
                    result = await _service.AddUpdateStudentCBSERegNo(request);
                    message = "CBSE Registration Number updated successfully.";
                }

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = string.Empty
                });
            }
        }
        //[HttpPost("AddUpdateStudentBoardRollNo")]
        //public async Task<IActionResult> AddUpdateStudentBoardRollNo( [FromBody] AddUpdateStudentBoardRollNoRequest request)
        //{
        //    try
        //    {
        //        if (request == null)
        //        {
        //            return BadRequest(new ApiResponse<string>
        //            {
        //                Success = false,
        //                Message = "Invalid request.",
        //                Data = string.Empty
        //            });
        //        }

        //        if (string.IsNullOrWhiteSpace(request.StudentId))
        //        {
        //            return BadRequest(new ApiResponse<string>
        //            {
        //                Success = false,
        //                Message = "Student data is required.",
        //                Data = string.Empty
        //            });
        //        }

        //        var resultBoard = await _service.AddUpdateStudentBoardRollNo(request);
        //        var resultReg = await _service.AddUpdateStudentCBSERegNo(request);

        //        return Ok(new ApiResponse<string>
        //        {
        //            Success = true,
        //            Message = "Board Roll Number updated successfully.",
        //            Data = resultBoard
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new ApiResponse<string>
        //        {
        //            Success = false,
        //            Message = ex.Message,
        //            Data = string.Empty
        //        });
        //    }
        //}
        //[HttpPost("AddUpdateStudentCBSERegNo")]
        //public async Task<IActionResult> AddUpdateStudentCBSERegNo([FromBody] UpdateStudentCBSERegNoRequest request)
        //{
        //    if (request == null || request.Students == null || request.Students.Count == 0)
        //    {
        //        return BadRequest(new ApiResponse<object>
        //        {
        //            Success = false,
        //            Message = "At least one student is required."
        //        });
        //    }
        //    try
        //    {
        //        var results = await _service.AddUpdateStudentCBSERegNo(request);

        //        return Ok(new ApiResponse<object>
        //        {
        //            Success = true,
        //            Message = "Processed successfully. Check each student's IsUpdated flag " +
        //                       "(false = skipped, that CBSE Reg No is already used by another student).",
        //            Data = results
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Exception: {ex.Message}");
        //        return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
        //        {
        //            Success = false,
        //            Message = "An error occurred while updating CBSE registration numbers."
        //        });
        //    }
        //}
    }
}
