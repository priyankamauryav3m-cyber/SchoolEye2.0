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
    [Authorize]
    [ApiExplorerSettings(GroupName = "Admission")]
    [Route("api/[controller]")]
    [ApiController]
    public class DisabilityStudentController : ControllerBase
    {
        private readonly IDisabilityStudentRepository _service;

        public DisabilityStudentController(IDisabilityStudentRepository service)
        {
            _service = service;
        }

        [HttpPost("GetDisabilityStudentData")]
        public async Task<IActionResult> GetDisabilityStudentData([FromBody] DisabilityStudentRequest request)
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
                var data = await _service.GetDisabilityStudentData(request);
                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<DisabilityStudentResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<DisabilityStudentResponse>()
                    });
                }
                return Ok(new ApiResponse<IEnumerable<DisabilityStudentResponse>>
                {
                    Success = true,
                    Message = "Disability student data retrieved successfully.",
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
                    Message = "An error occurred while fetching disability student data.",
                    Code = -1
                });
            }
        }


        [HttpPost("UpdateStudentDisability")]
        public async Task<IActionResult> UpdateStudentDisabilityData([FromBody] StudentDisabilityRequest request)
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
                var status = await _service.UpdateStudentDisabilityData(request);
                return status switch
                {
                    2 => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Student disability data updated successfully.",
                        Code = 2
                    }),

                    1 => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Student not found.",
                        Code = 1
                    }),

                    _ => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Failed to update the student disability data. See server logs for details.",
                        Code = status
                    })
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating the student disability data.",
                    Code = -1
                });
            }
        }
    }
}
