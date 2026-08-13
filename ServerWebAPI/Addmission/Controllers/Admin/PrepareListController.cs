using ApplicationInterface.Admin;
using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using Infrastructure.Admin;
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
    public class PrepareListController : ControllerBase
    {
        private readonly IPrepareRepository _prepareRepository;
        public PrepareListController(IPrepareRepository prepareRepository)
        {
            _prepareRepository = prepareRepository;
        }
        [HttpPost("GetRegistrationInfoList")]
        public async Task<IActionResult> GetRegistrationInfoList( [FromBody] RegistrationInfoListRequest model)
        {
            if (model == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request.",
                    Code = 0
                });
            }

            try
            {
                var result = await _prepareRepository.GetRegistrationInfoList(model);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Registration info list retrieved successfully.",
                    Data = result
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while getting registration info list.",
                        Code = -1
                    });
            }
        }


        [HttpPost("AddPublishList")]
        public async Task<IActionResult> AddPublishList( [FromBody] PublishListModel model)
        {
            if (model == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request.",
                    Code = 0
                });
            }

            try
            {
                var result = await _prepareRepository.AddPublishList(model);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Publish list added successfully.",
                    Code = 1,
                    Data = result
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while adding publish list.",
                        Code = -1
                    });
            }
        }


        [HttpPost("MovePrepareList")]
        public async Task<IActionResult> GetListStatusData([FromBody] PublishListModel request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request.",
                    Code = 0
                });
            }

            try
            {
                var result = await _prepareRepository.GetListStatusData(request);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "List status data retrieved successfully.",
                    Code = 1,
                    Data = result
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while getting list status data.",
                        Code = -1
                    });
            }

        }
        [HttpPost("GetPublishList")]
        public async Task<IActionResult> GetPublishList(SearchAnyRequestModel searchAnyRequest)
        {
            try
            {
                var result = await _prepareRepository.GetAllAsync(searchAnyRequest);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Publish list retrieved successfully.",
                    Data = result
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while getting publish list.",
                        Code = -1
                    });
            }
        }
        [HttpPost("AddStudentInList")]
        public async Task<IActionResult> AddStudentInList([FromBody] AddStudentInListRequest request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request.",
                    Code = 0
                });
            }

            try
            {
                var result = await _prepareRepository.AddStudentInListAsync(request);

                return result switch
                {
                    1 => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Student added to publish list successfully.",
                        Code = 1
                    }),

                    _ => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Student could not be added to publish list.",
                        Code = 0
                    })
                };
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while adding student to publish list.",
                        Code = -1
                    });
            }
        }
        [HttpPost("DeleteStudentInList")]
        public async Task<IActionResult> DeleteStudentInList([FromBody] AddStudentInListRequest request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request.",
                    Code = 0
                });
            }

            try
            {
                var result = await _prepareRepository.DeleteStudentInListAsync(request);

                return result switch
                {
                    1 => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Student Deleted  successfully.",
                        Code = 1
                    }),

                    _ => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Student could not be added to publish list.",
                        Code = 0
                    })
                };
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while adding student to publish list.",
                        Code = -1
                    });
            }
        }
        [HttpPost("PublishStudentInList")]
        public async Task<IActionResult> PublishStudentInList([FromBody] AddStudentInListRequest request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request.",
                    Code = 0
                });
            }

            try
            {
                var result = await _prepareRepository.PublishStudentInListAsync(request);

                return result switch
                {
                    1 => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Student Publish to list  successfully.",
                        Code = 1
                    }),

                    _ => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Student could not be added to publish list.",
                        Code = 0
                    })
                };
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while adding student to publish list.",
                        Code = -1
                    });
            }
        }
        [HttpPost("GetPublishingListDetails")]
        public async Task<IActionResult> GetPublishingListDetails([FromBody] RegistrationInfoListRequest model)
        {
            if (model == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request.",
                    Code = 0
                });
            }

            try
            {
                var result = await _prepareRepository.GetPublishingListDetails(model);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Publishing list details retrieved successfully.",
                    Data = result
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while getting publishing list details.",
                        Code = -1
                    });
            }
        }
    }
}
