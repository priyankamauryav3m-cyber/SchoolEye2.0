using ApplicationInterface.SchoolMaster;
using DomainModel.SchoolMaster;
using Infrastructure.SchoolMaster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.Addmission.Controllers.SchoolMaster
{
    [ApiExplorerSettings(GroupName = "Admission")]
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ClassSubjectController : ControllerBase
    {
        private readonly IClassSubjectRepository _service;

        public ClassSubjectController(IClassSubjectRepository service)
        {
            _service = service;
        }
        [HttpGet("GetClassSubject")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _service.GetAllAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while fetching class subject data.",
                    error = ex.Message
                });
            }
        }

        [HttpPost("AddOrUpdateClassSubject")]
        public async Task<IActionResult> AddUpdateClassSubject([FromBody] ClassSubjectModel model)
        {
            if (model == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var returnValue = await _service.AddUpdateClassSubject(model);
                return returnValue switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "ClassSubject name already exists",
                        Code = 0
                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "ClassSubject added successfully",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "ClassSubject updated successfully",
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

        [HttpPost("DeleteClassSubject")]
        public async Task<IActionResult> Delete([FromBody] int mapId)
        {
            try
            {
                if (mapId <= 0)
                    return BadRequest(ApiResponse<string>.Fail("Invalid ID"));
                await _service.DeleteAsync(mapId);
                return Ok(
                    ApiResponse<string>.Ok("ClassSubject status changed"));
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail($"An error occurred: {ex.Message}")
                );
            }
        }
    }
}
