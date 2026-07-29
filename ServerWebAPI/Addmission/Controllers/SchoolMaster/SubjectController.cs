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
    public class SubjectController : ControllerBase
    {
        private readonly ISubjectRepository _service;

        public SubjectController(ISubjectRepository service)
        {
            _service = service;
        }

        [HttpPost("AddOrUpdateSubject")]
        public async Task<IActionResult> AddUpdateSubject([FromBody] SubjectModel objsubject)
        {
            if (objsubject == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var returnValue = await _service.AddUpdateSubject(objsubject);
                return returnValue switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Subject name already exists",
                        Code=0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Subject added successfully",
                        Code=1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Subject updated successfully",
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

        [HttpPost("DeleteOrSubject")]
        public async Task<IActionResult> DeleteReligion([FromBody] int SubjectId)
        {
            try
            {
                if (SubjectId <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID"));

                await _service.DeleteSubject(SubjectId);
                return Ok(
                    ApiResponse<string>.Ok("Religion status changed"));
            }
            catch (Exception)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail("Something went wrong."));
            }
        }

        [HttpGet("AllSubject")]
        public async Task<IActionResult> GetSubject()
        {
            try
            {
                var data = await _service.GetSubject();
                return Ok(data);
            }
            catch (Exception)
            {
                return StatusCode(500, "Something went wrong.");
            }
        }
    }
}
