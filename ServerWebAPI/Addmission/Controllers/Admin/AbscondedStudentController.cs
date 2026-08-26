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
    public class GetAbscondedStudentController : ControllerBase
    {
        private readonly IAbscondedStudentRepository _service;

        public GetAbscondedStudentController(IAbscondedStudentRepository service)
        {
            _service = service;
        }

        [HttpPost("GetAbscondedStudentData")]
        public async Task<IActionResult> GetAbscondedStudent([FromBody] GetAbscondedStudentRequest request)
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
                var data = await _service.GetAbscondedStudent(request);

                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<GetAbscondedStudentResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<GetAbscondedStudentResponse>()
                    });
                }

                return Ok(new ApiResponse<IEnumerable<GetAbscondedStudentResponse>>
                {
                    Success = true,
                    Message = "Absconded student list retrieved successfully.",
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
                    Message = "An error occurred while fetching the absconded student list.",
                    Code = -1
                });
            }
        }
        [HttpPost("AbscondStudent")]
        public async Task<IActionResult> AbscondStudent([FromBody] GetAbscondedStudentRequest request)
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
                var returnValue = await _service.AbscondStudent(request);
                if (returnValue == "2")
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = returnValue == "2" ? "Student marked as absconded successfully." : "Abscond operation failed.",
                        Code = 2
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while marking the student as absconded.",
                        Code = -1
                    });
                }
             
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while marking the student as absconded.",
                    Code = -1
                });
            }
        }

        [HttpPost("UnAbscondStudentData")]
        public async Task<IActionResult> UnAbscondStudent([FromBody] GetAbscondedStudentRequest request)
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
                var returnValue = await _service.UnAbscondStudent(request);
                if (returnValue == "2")
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = returnValue == "2" ? "Student marked as Unabsconded successfully." : "Abscond operation failed.",
                        Code = 2
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while marking the student as absconded.",
                        Code = -1
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while un-absconding the student.",
                    Code = -1
                });
            }
        }


    }
}
