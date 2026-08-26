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
    public class FindStudentController : ControllerBase
    {
        private readonly IFindStudentRepository _service;

        public FindStudentController(IFindStudentRepository service)
        {
            _service = service;
        }

        [HttpPost("GetSearchedStudentByDetails")]
        public async Task<IActionResult> GetSearchedStudentByDetails([FromBody] FIndStudentRequest request)
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
                var data = await _service.GetSearchedStudentByDetails(request);

                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<FindStudentResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<FindStudentResponse>()
                    });
                }

                return Ok(new ApiResponse<IEnumerable<FindStudentResponse>>
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
