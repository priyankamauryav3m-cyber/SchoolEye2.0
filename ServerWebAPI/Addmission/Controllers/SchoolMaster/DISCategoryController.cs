using ApplicationInterface.SchoolMaster;
using DomainModel.SchoolMaster;
using Infrastructure.SchoolMaster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.Addmission.Controllers.SchoolMaster
{
    [ApiExplorerSettings(GroupName = "Admission")]
    [Route("api/[controller]")]
    [ApiController]
    public class DISCategoryController : ControllerBase
    {

        private readonly IDisCategoryRepository _service;

        public DISCategoryController(IDisCategoryRepository service)
        {
            _service = service;
        }

        [HttpPost("AddOrUpdateDisCategoryData")]
        public async Task<IActionResult> AddUpdateDisCategory([FromBody] DisCategoryModel objCategory)
        {
            if (objCategory == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var result = await _service.AddUpdateDisCategory(objCategory);
                return result switch
                {
                    "0" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Category name already exists",
                        Code = 0

                    }),
                    "1" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Category saved successfully",
                        Code = 1
                    }),
                    "2" => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Category updated successfully",
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


        [HttpGet("GetDisCategory")]
        public async Task<IActionResult> GetDisCategoryData( int sessionId)
        {
            try
            {
                var data = await _service.GetDisCategoryData(sessionId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while fetching category data.",
                    error = ex.Message
                });
            }
        }



        [HttpPost("DeleteDisCategory")]
        public async Task<IActionResult> DeleteDisCategoryData([FromBody] int categoryId)
        {
            try
            {
                if (categoryId <= 0)
                    return BadRequest(
                        ApiResponse<string>.Fail("Invalid ID"));

                await _service.DeleteDisCategoryData(categoryId);

                return Ok(
                    ApiResponse<string>.Ok("Category status changed"));
            }
            catch (Exception)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail("Something went wrong."));
            }
        }
    }
}
