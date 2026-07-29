using ApplicationInterface.SchoolMaster;
using DomainModel.FinanceMNGT;
using DomainModel.SchoolMaster;
using Infrastructure.SchoolMaster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MyApp.Common;

namespace ServerWebAPI.Addmission.Controllers.SchoolMaster
{
    [ApiExplorerSettings(GroupName = "Admission")]
    [Authorize]
    //[EnableRateLimiting("V3MAPI_Call_Limit")]
    [Route("api/[controller]")]
    [ApiController]
    public class ClassSectionController : ControllerBase
    {
        private readonly IClassSectionRepository _service;
        public ClassSectionController(IClassSectionRepository service)
        {
            _service = service;
        }

        [HttpGet("GetClassSection")]
        public async Task<IActionResult> GetClassSectionData()
        {
            try
            {
                var data = await _service.GetClassSectionData();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = ex.Message }
                );
            }
        }

        [HttpGet("GetCategory")]
        public async Task<IActionResult> GetCategoryData()
        {
            try
            {
                var data = await _service.GetCategoryData();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = ex.Message }
                );
            }
        }

        [HttpGet("GetDistance")]
        public async Task<IActionResult> GetDistanceData()
        {
            try
            {
                var data = await _service.GetDistanceData();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail(ex.Message)
                );
            }
        }


        [HttpGet("GetMstMotherTongue")]
        public async Task<IActionResult> GetMstMotherTongueData()
        {
            try
            {
                var data = await _service.GetMotherTongueData();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail(ex.Message)
                );
            }
        }

        [HttpGet("GetVisaType")]
        public async Task<IActionResult> GetVisaTypeData()
        {
            try
            {
                var data = await _service.GetVisaTypeData();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail(ex.Message)
                );
            }
        }

        [HttpGet("GetPassportTypeName")]
        public async Task<IActionResult> GetPassportTypeNameData()
        {
            try
            {
                var data = await _service.GetPassportTypeNameData();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail(ex.Message)
                );
            }
        }

        [HttpGet("GetBranchName")]
        public async Task<IActionResult> GetBranchNameData()
        {
            try
            {
                var data = await _service.GetBranchNameData();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail(ex.Message)
                );
            }
        }
        [HttpPost("GetClassWithSection")]
        public async Task<IActionResult> GetClassSection([FromBody] SearchAnyRequestModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = true,
                        Message = "Invalid request data."
                    });
                }

                var result = await _service.GetClassSection(model);

                return Ok(new ApiResponse<IEnumerable<SectionModel>>
                {
                    Success = true,
                    Message = "Section list fetched successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}
