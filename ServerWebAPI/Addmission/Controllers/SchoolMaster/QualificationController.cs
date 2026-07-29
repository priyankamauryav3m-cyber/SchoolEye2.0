using ApplicationInterface.SchoolMaster;
using DomainModel.SchoolMaster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.Addmission.Controllers.SchoolMaster
{
    [ApiExplorerSettings(GroupName = "Admission")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class QualificationController : ControllerBase
    {
        private readonly IQualification _qualification;
        private readonly ILogger<QualificationController> _logger;
        public QualificationController(ILogger<QualificationController> logger, IQualification qualification)
        {
            _logger = logger;
            _qualification = qualification;
        }


        [HttpGet("GetQualification")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _qualification.GetAllQualification();
                return Ok(data);
            }
            catch (Exception)
            {
                return StatusCode(500, "Something went wrong.");
            }
        }
        [HttpPost("DeleteQualification")]
        public async Task<IActionResult> Delete([FromBody] int QualificationId)
        {
            try
            {
                if (QualificationId <= 0)
                    return BadRequest(ApiResponse<string>.Fail("Invalid ID"));
                await _qualification.DeleteQualification(QualificationId);
                return Ok(ApiResponse<string>.Ok("Qualification status changed"));
            }
            catch (Exception)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail("Something went wrong."));
            }
        }
        [HttpPost("AddOrUpdateQualification")]
        public async Task<IActionResult> AddUpdateQualification([FromBody] Qualification objQualification)
        {
            if (objQualification == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."

                });
            try
            {
                var returnValue = await _qualification.AddUpdateQualification(objQualification);
                return returnValue switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Qualification name already exists",
                        Code = 0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Qualification added successfully",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Qualification updated successfully",
                        Code = 2
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
    }
}
