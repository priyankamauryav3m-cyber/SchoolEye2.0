using ApplicationInterface.Admin;
using ApplicationInterface.FinanceMNGT.FeeMNGTMasters;
using DomainModel.FinanceMNGT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using System.Numerics;

namespace ServerWebAPI.Addmission.Controllers.Admin
{
    [ApiExplorerSettings(GroupName = "Admission")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentPromotionController : ControllerBase
    {
        private readonly IStudentPromotionRepository _service;
        public StudentPromotionController(IStudentPromotionRepository service)
        {
            _service = service;
        }
        [HttpPost("GetStudentPromotion")]
        public async Task<IActionResult> GetClassWiseStudentPromotion([FromBody] SearchAnyRequestModel searchAny)
        {
            try
            {
                var result = await _service.GetClassWiseStudentPromotion(searchAny);
                return Ok(new ApiResponse<IEnumerable<ClassWiseStudentForPromotion>>
                {
                    Success = true,
                    Data = result
                });
             }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An error occurred while fetching data.",
                    Error = ex.Message
                });
            }
        }


        [HttpPost("PromoteStudentClass")]
        public async Task<IActionResult> PromoteStudentClass([FromBody] List<PromoteClassModel> Promote)
        {
            if (Promote == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var returnValue = await _service.PromoteStudentClass(Promote);

                if (returnValue.All(x => x == "1"))
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Student Promoted Successfully !",
                        Code = 1
                    });
                }

                if (returnValue.Any(x => x == "-1"))
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Student Dues Exists, please clear dues before demote",
                        Code = -1
                    });
                }

                if (returnValue.Any(x => x == "-2"))
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Student receipt Exists, please clear all dues and receipt",
                        Code = -2
                    });
                }

                if (returnValue.Any(x => x == "-3"))
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Student Attendance Exists, please delete attendance before demote",
                        Code = -3
                    });
                }

                if (returnValue.Any(x => x == "-4"))
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Student balance should be zero",
                        Code = -4
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Unknown operation result"
                });
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while adding or updating record."
                    });
            }
        }

        [HttpPost("GetStudentNotPromoted")]
        public async Task<IActionResult> NotStudentPromototed([FromBody] SearchAnyRequestModel searchAny)
        {
            try
            {
                var result = await _service.GetAllNotPromotedStudent(searchAny);
                return Ok(new ApiResponse<IEnumerable<StudentNotPromotedModel>>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An error occurred while fetching data.",
                    Error = ex.Message
                });
            }
        }
    }
}
