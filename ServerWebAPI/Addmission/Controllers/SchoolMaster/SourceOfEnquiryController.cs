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
    [ApiController]
    [Route("api/[controller]")]
    public class SourceOfEnquiryController : ControllerBase
    {
        private readonly ISourceOfEnquiryRepository _service;

        public SourceOfEnquiryController(ISourceOfEnquiryRepository service)
        {
            _service = service;
        }

        [HttpGet("GetSourceOfEnquiry")]
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
                    message = "An error occurred while fetching source of enquiry data.",
                    error = ex.Message
                });
            }
        }

        [HttpPost("AddOrUpdateSourceOfEnquiry")]
        public async Task<IActionResult> AddOrUpdateSourceOfEnquiry([FromBody] SourceOfEnquiryModel model)
        {
            if (model == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            }

            try
            {
                var result = await _service.AddUpdateSourceOfEnquiry(model);

                return result switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Source of Enquiry already exists.",
                        Code = 0
                    }),

                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Source of Enquiry saved successfully.",
                        Code = 1
                    }),

                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Source of Enquiry updated successfully.",
                        Code = 2
                    }),

                    _ => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unexpected error occurred."
                    })
                };
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while adding or updating the record."
                    });
            }
        }

        [HttpPost("DeleteSourceOfEnquiry")]
        public async Task<IActionResult> Delete([FromBody] int sourceId)
        {
            try
            {
                if (sourceId <= 0)
                {
                    return BadRequest(ApiResponse<string>.Fail("Invalid Source Id."));
                }

                await _service.DeleteSourceOfEnquiry(sourceId);

                return Ok(ApiResponse<string>.Ok("Source of Enquiry status changed successfully."));
            }
            catch
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail("Something went wrong."));
            }
        }
    }
}