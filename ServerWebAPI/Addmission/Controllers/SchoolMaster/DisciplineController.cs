using ApplicationInterface.SchoolMaster;
using DomainModel.SchoolMaster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.Addmission.Controllers.SchoolMaster
{
    [Authorize]
    [ApiExplorerSettings(GroupName = "Admission")]
    // [Authorize(AuthenticationSchemes = "LoginV3M")]
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]

    public class DisciplineController : ControllerBase
    {
        private readonly IDisciplineRepository _service;

        public DisciplineController(IDisciplineRepository service)
        {
            _service = service;
        }

        [HttpGet("GetDiscipline")]
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
                    message = "An error occurred while fetching discipline data.",
                    error = ex.Message
                });
            }
        }
        [HttpPost("AddOrUpdateDiscipline")]
        public async Task<IActionResult> AddUpdateDiscipline([FromBody] DisciplineModel objDiscipline)
        {
            if (objDiscipline == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var result = await _service.AddUpdateDiscipline(objDiscipline);
                return result switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Discipline name already exists",
                        Code = 0

                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Discipline saved successfully",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Discipline updated successfully",
                        Code = 2
                    }),
                    _ => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unexpected error occurred"
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

        [HttpPost("DeleteDiscipline")]
        public async Task<IActionResult> Delete([FromBody] int disciplineId)
        {
            try
            {
                if (disciplineId <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID"));

                await _service.DeleteDisciplineData(disciplineId);

                return Ok(
                    ApiResponse<string>.Ok("Discipline status changed"));
            }
            catch (Exception)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail("Something went wrong."));
            }
        }

    }
}
