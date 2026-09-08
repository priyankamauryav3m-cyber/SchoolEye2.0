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
    public class StudentRTEController : ControllerBase
    {
        private readonly IStudentRTERepository _service;

        public StudentRTEController(IStudentRTERepository service)
        {
            _service = service;
        }

        [HttpPost("GetSearchedStudentRTEData")]
        public async Task<IActionResult> GetSearchedStudentRTE([FromBody] StudentRTERequest request)
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
                var data = await _service.GetSearchedStudentRTE(request);
                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<StudentRTEResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<StudentRTEResponse>()
                    });
                }

                return Ok(new ApiResponse<IEnumerable<StudentRTEResponse>>
                {
                    Success = true,
                    Message = "RTE student list retrieved successfully.",
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
                    Message = "An error occurred while searching for RTE students.",
                    Code = -1
                });
            }
        }

        [HttpPost("AddUpdateRTEStudentData")]
        public async Task<IActionResult> AddUpdateRTEStudentData([FromBody] RTEStudentDataRequest request)
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
                var status = await _service.AddUpdateRTEStudentData(request);

                return status switch
                {
                    1 => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "RTE student data added successfully.",
                        Code = 1
                    }),

                    2 => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "RTE student data updated successfully.",
                        Code = 2
                    }),

                    _ => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unknown operation result.",
                        Code = 0
                    })
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while saving the RTE student data.",
                    Code = -1
                });
            }
        }
    }
}
