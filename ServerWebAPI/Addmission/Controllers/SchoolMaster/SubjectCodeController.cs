using ApplicationInterface.SchoolMaster;
using DomainModel.Admin;
using DomainModel.SchoolMaster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.Addmission.Controllers.SchoolMaster
{
    [ApiExplorerSettings(GroupName = "Admission")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectCodeController : ControllerBase
    {
        private readonly ISubjectCodeRepository _service;

        public SubjectCodeController(ISubjectCodeRepository service)
        {
            _service = service;
        }

        [HttpPost("AddOrUpdateSubjectCode")]
        public async Task<IActionResult> AddUpdateSubjectCode([FromBody] SubjectCodeMaster objSubject)
        {
            if (objSubject == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var returnValue = await _service.AddUpdateSubjectCode(objSubject);
                return returnValue switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "SubjectCode name already exists",
                        Code=0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "SubjectCode added successfully",
                        Code=1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "SubjectCode updated successfully",
                        Code=2

                    }),
                    _ => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unknown operation result"
                    })
                };
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

        [HttpPost("DeleteOrSubjectode")]
        public async Task<IActionResult> DeleteSubjectode([FromBody] int Sid)
        {
            try
            {
                if (Sid <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID"));

                await _service.DeleteSubjectCode(Sid);

                return Ok(
                    ApiResponse<string>.Ok("Religion status changed"));
            }
            catch (Exception)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail("Something went wrong."));
            }
        }


        [HttpGet("AllSubjectCode")]
        public async Task<IActionResult> GetSubjectCode()
        {
            try
            {
                var data = await _service.GetSubjectCode();
                return Ok(data);
            }
            catch (Exception)
            {
                return StatusCode(500, "Something went wrong.");
            }
        }
    }
}
