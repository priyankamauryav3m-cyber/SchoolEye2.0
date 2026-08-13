using ApplicationInterface.SchoolMaster;
using Azure.Core;
using DomainModel.FinanceMNGT;
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

    public class ClassDocumentController : ControllerBase
    {
        private readonly IClassDocumentRepository _service;

        public ClassDocumentController(IClassDocumentRepository service)
        {
            _service = service;
        }

        [HttpPost("GetClassDocument")]
        public async Task<IActionResult> GetAll(SearchAnyRequestModel searchAnyRequest)
        {
            try
            {
                var data = await _service.GetAllAsync(searchAnyRequest);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while fetching class document mapping data.",
                    error = ex.Message
                });
            }
        }
        [HttpPost("MapDocument")]
        public async Task<IActionResult> MapDocument([FromBody] ClassDocumentModel objMapping)
        {
            if (objMapping == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var result = await _service.MapDocumentWithClass(objMapping);
                return result switch
                {
                    0 => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "This Document is already mapped to the selected Class",
                        Code = 0
                    }),
                    1 => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Document mapped to Class successfully",
                        Code = 1
                    }),
                    _ => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while mapping the document."
                    })
                };
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while mapping the document."
                    });
            }
        }

        [HttpPost("DeleteClassDocument")]
        public async Task<IActionResult> Delete([FromBody] UpdateClassDocumentRequest request)
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
                var result = await _service.DeleteClassDocumentData(request);

                return result switch
                {
                    1 => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Document unmapped successfully.",
                        Code = 1
                    }),

                    0 => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Document mapping not found.",
                        Code = 0
                    }),

                    _ => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while unmapping the document.",
                        Code = -1
                    })
                };
            }
            catch
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while unmapping the document.",
                        Code = -1
                    });
            }
        }
        [HttpPost("UpdateMandatory")]
        public async Task<IActionResult> UpdateMandatory( [FromBody] UpdateClassDocumentRequest request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            }
            try
            {
                var result = await _service.UpdateMandatoryAsync(request);

                return result switch
                {
                    1 => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Document marked as mandatory successfully.",
                        Code = 1
                    }),

                    2 => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Document marked as non-mandatory successfully.",
                        Code = 2
                    }),

                    0 => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Document status is already the same.",
                        Code = 0
                    }),

                    _ => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while updating document status.",
                        Code = -1
                    })
                };
            }
            catch
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while updating document status."
                    });
            }
        }

    }
}
