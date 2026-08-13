using ApplicationInterface.Admin;
using DomainModel.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.Addmission.Controllers.Admin
{
    [Authorize]
    [ApiExplorerSettings(GroupName = "Admission")]
    [Route("api/[controller]")]
    [ApiController]
    public class ViewPublishListController : ControllerBase
    {
        private readonly IViewPublishListRepository _service;
        public ViewPublishListController(IViewPublishListRepository viewPublishListRepository)
        {
            _service = viewPublishListRepository;
        }
        [HttpPost("PublishingList")]
        public async Task<IActionResult> PublishingList([FromBody] PublishingListRequest model)
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
                var data = await _service.GetPublishingList(model);

                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<PublishingListResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<PublishingListResponse>()
                    });
                }

                return Ok(new ApiResponse<IEnumerable<PublishingListResponse>>
                {
                    Success = true,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while fetching the publishing list."
                });
            }
        }

    }
}
