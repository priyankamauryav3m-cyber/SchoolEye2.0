using ApplicationInterface.Admin;
using DomainModel.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using ServerWebAPI.Authorization;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ApplicationInterface;


namespace ServerWebAPI.Addmission.Controllers.Admin
{
    //[Authorize]
    [ApiExplorerSettings(GroupName = "Admission")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdmissionDateController : ControllerBase
    {
        private readonly IAdmissionDateRepository _service;
        public AdmissionDateController(IAdmissionDateRepository service)
        {
            _service = service;
        }

        [HttpPost("UpdateStudentAdmissionDate")]
        public async Task<IActionResult> UpdateStudentAdmissionDate([FromBody] UpdateStudentAdmissionDateRequest request)
        {
            try
            {
                var returnValue = await _service.UpdateStudentAdmissionDate(request);

                if (returnValue == "1")
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = true,
                        Message = "Student admission date updated successfully.",
                        Data = returnValue,
                        Code=1
                    });
                }

                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = returnValue,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>
                    {
                        Success = false,
                        Message = ex.Message,
                        Data = null
                    });
            }
        }


        [HttpPost("GetSearchedStudent")]
        public async Task<IActionResult> GetSearchedStudent([FromBody] StuSearchedStudentRequest request)
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
                        Success = false,
                        Message = "No students found.",
                        Code = 0,
                        Data = Enumerable.Empty<StuSearchedStudentResponse>()
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



        //  this is  demo only remove this method 
        [HttpPost("GetClassRegistrationDocuments")]
        public async Task<IActionResult> GetClassRegistrationDocuments([FromBody] ClassRegistrationDocumentsRequest request)
        {
            try
            {
                var data =
                    await _service.GetClassRegistrationDocumentsAsync(request);

                if (data == null || !data.Any())
                {
                    return NotFound(new ApiResponse<IEnumerable<ClassRegistrationDocumentsResponse>>
                    {
                        Success = false,
                        Message = "No registration documents found.",
                        Code = 0,
                        Data = null
                    });
                }

                return Ok(new ApiResponse<IEnumerable<ClassRegistrationDocumentsResponse>>
                {
                    Success = true,
                    Message = "Registration documents retrieved successfully.",
                    Code = 1,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"GetClassRegistrationDocuments Error: {ex.Message}");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message =
                            "An error occurred while fetching registration documents.",
                        Code = 0,
                        Data = null
                    });
            }
        }





    }
}
