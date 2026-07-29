using ApplicationInterface.Admin;
using ApplicationInterface.SuperAdmin;
using DomainModel.SchoolMaster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.Addmission.Controllers.Admin
{
    [ApiExplorerSettings(GroupName = "Admission")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BookMarkController : ControllerBase
    {
        private readonly IBookMarkRepository _service;
        public BookMarkController(IBookMarkRepository service)
        {
            _service = service;
        }
        [HttpPost("AddOrUpdateBookMarks")]
        public async Task<IActionResult> AddOrUpdateBookMarksData([FromBody] BookMarkModel objbook)
        {
            if (objbook == null)
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var returnValue = await _service.AddOrUpdateBookMarksData(objbook);
                return returnValue switch
                {
                    "0" => Ok(new ApiResponse<string>
                    {
                        Success = true,
                        Message = "Book Mark  already exists",
                        Code = 0
                    }),
                    "1" => Ok(new ApiResponse<string>
                    {
                        Success = true,
                        Message = "Book Marks added successfully",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<string>
                    {
                        Success = true,
                        Message = "Book Marks updated successfully",
                        Code = 2
                    }),
                  
                    _ => Ok(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Unknown operation result"
                    })
                };
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>
                    {
                        Success = false,
                        Message = "An error occurred while adding or updating record."
                    });
            }


        }
        [HttpGet("GetBookMarks")]
        public async Task<IActionResult> GetBookMarksData(string createdby)
        {
            try
            {
                var data = await _service.GetBookMarksData(createdby);
                return Ok(data);
            }
            catch (Exception)
            {
                return StatusCode(500, "Something went wrong.");
            }
        }

        [HttpPost("DeleteBookMarks")]
        public async Task<IActionResult> DeleteBookMarksData([FromBody] int BookMarkId)
        {
            try
            {
                if (BookMarkId <= 0)
                    return BadRequest(ApiResponse<string>.Fail("Invalid ID"));
                await _service.DeleteBookMarksData(BookMarkId);
                return Ok(ApiResponse<string>.Ok("Book Marks Deleted Successfully"));
            }
            catch (Exception)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail("Something went wrong."));
            }
        }
    }
}
