using ApplicationInterface.Admin;
using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.Addmission.Controllers.Admin
{
    [ApiExplorerSettings(GroupName = "Admission")]
    [Route("api/[controller]")]
    [ApiController]
    public class EnquiryController : ControllerBase
    {
        private readonly IEnquiryRepository _repo;

        public EnquiryController(IEnquiryRepository repo)
        {
            _repo = repo;
        }

        [HttpPost("AddOrUpdateEnquiry")]
        public async Task<IActionResult> SubmitEnquiry([FromBody] EnquiryListResponse model)
        {
            if (model == null)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found."
                });
            try
            {
                var returnValue = await _repo.SubmitEnquiryData(model);
                if (!string.IsNullOrEmpty(returnValue) && returnValue != "-2" && returnValue != "2" && returnValue != "0")
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Enquiry saved successfully",
                        Data = returnValue,
                        Code = 1
                    });
                }
                if (returnValue == "-2")
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Enquiry Data Not Found",
                        Code = -2
                    });
                }
                if (returnValue == "2")
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Enquiry updated successfully",
                        Code = 2
                    });
                }
                return Ok(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Unknown operation result",
                    Code = 0
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }


        [HttpPost("GetEnquiryList")]
        public async Task<IActionResult> GetEnquiryListofData([FromBody] EnquiryRequestDto request)
        {
            try
            {
                var data = await _repo.GetEnquiryListofData(request);

                return Ok(new ApiResponse<IEnumerable<EnquiryListResponse>>
                {
                    Success = true,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("GetFollowupDetails")]

        public async Task<IActionResult> GetFollowupDetails(SearchAnyRequestModel searchAnyRequest)
        {

            try
            {
                if (searchAnyRequest == null)
                {
                    return BadRequest("Invalid parameters");
                }
                var data = await _repo.GetFollowupDetails(searchAnyRequest);
                return Ok(new ApiResponse<IEnumerable<FollowupDetailsResponse>>
                {
                    Success = true,
                    Data = data

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

        [HttpPost("AddFollowup")]

        public async Task<IActionResult> AddFollowupData([FromBody] AddFollowupRequest request)
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
                var returnValue = await _repo.AddFollowupDetails(request);

                return returnValue switch
                {

                    "2" => Ok(new ApiResponse<string>
                    {
                        Success = true,
                        Message = "Followup inserted and updated successfully",
                        Code = 2
                    }),

                    "-1" => Ok(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Enquiry data not found",
                        Code = -1
                    }),

                    _ => Ok(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Unknown operation result",
                        Code = 0
                    })
                };
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
   
        [HttpPost("GetEnquirySummaryData")]
        public async Task<IActionResult> GetEnquirySummaryData([FromBody] EnquirySummaryTableRequest request)
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
                var data = await _repo.GetEnquirySummaryData(request);
                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<EnquirySummaryTableResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<EnquirySummaryTableResponse>()
                    });
                }
                return Ok(new ApiResponse<IEnumerable<EnquirySummaryTableResponse>>
                {
                    Success = true,
                    Message = "Enquiry summary retrieved successfully.",
                    Code = 1,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while fetching the enquiry summary.",
                    Code = -1
                });
            }
        }

    }
}
