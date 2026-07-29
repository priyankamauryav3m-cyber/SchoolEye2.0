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

    public class DocumentController : ControllerBase
    {
        private readonly IDocumentRepository _service;

        public DocumentController(IDocumentRepository service)
        {
            _service = service;
        }

        [HttpGet("GetDocument")]
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
                    message = "An error occurred while fetching document data.",
                    error = ex.Message
                });
            }
        }
        [HttpPost("AddOrUpdateDocument")]
        public async Task<IActionResult> AddUpdateDocument([FromBody] DocumentModel objDocument)
        {
            if (objDocument == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var result = await _service.AddUpdateDocument(objDocument);
                return result switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Document name already exists",
                        Code = 0

                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Document saved successfully",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Document updated successfully",
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

        [HttpPost("DeleteDocument")]
        public async Task<IActionResult> Delete([FromBody] int docId)
        {
            try
            {
                if (docId <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID"));

                await _service.DeleteDocumentData(docId);

                return Ok(
                    ApiResponse<string>.Ok("Document status changed"));
            }
            catch (Exception)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail("Something went wrong."));
            }
        }

    }
}
