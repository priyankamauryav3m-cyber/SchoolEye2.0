using ApplicationInterface.FinanceMNGT;
using ApplicationInterface.FinanceMNGT.FeeMNGTMasters;
using Azure.Core;
using DomainModel.FinanceMNGT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.FinanceManagement.Controllers.FinanceMNGT
{
    [ApiExplorerSettings(GroupName = "FinanceManagement")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ViewStudentController : ControllerBase
    {

        private readonly IViewStudentRepository _service;
        public ViewStudentController(IViewStudentRepository service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("ViewStudentList")]
        public async Task<IActionResult> GetViewStudentListData(StudentRequest request)
        {
            try
            {
                var resultlist = await _service.GetViewStudentListData(request);
                if (resultlist == null || !resultlist.Any())
                    return NotFound(ApiResponse<string>.Fail(" Not Found  View Student List"));
                return Ok(new ApiResponse<IEnumerable<ViewStudentModal>>
                {
                    Success = true,
                    Data = resultlist
                });
            
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
            }

        }
        [HttpPost]
        [Route("ViewGetSearchedStudentList")]
        public async Task<IActionResult> GetViewSearchedStudentListData(GetSearchedStudentRequestModel request)
        {
            try
            {
                var resultlist = await _service.GetSearchedStudentListData(request);
                if (resultlist == null || !resultlist.Any())
                    return NotFound(ApiResponse<string>.Fail(" Not Found  View Student List"));
                return Ok(new ApiResponse<IEnumerable<GetSearchedViewStudentModel>>
                {
                    Success = true,
                    Data = resultlist
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
            }

        }
        [HttpPost("GetStudentPersonalDetails")]
        public async Task<IActionResult> GetStudentDetails([FromBody] SearchAnyRequestModel requestModel)
        {
            try
            {
                var data = await _service.GetStudentDetailsAsync(requestModel);
                if (data == null)
                {
                    return NotFound(ApiResponse<string>.Fail("Student details not found."));
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPost("GetStudentParentDetails")]
        public async Task<IActionResult> GetStudentParentDetails([FromBody] SearchAnyRequestModel requestModel)
        {
            try
            {
                var data = await _service.GetStudentParentDetails(requestModel);
                if (data == null)
                {
                    return NotFound(ApiResponse<string>.Fail("Student details not found."));
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
            }

        }
        [HttpPost]
        [Route("GetSiblingDetails")]
        public async Task<IActionResult> GetSiblingDetailsData(SearchAnyRequestModel request)
        {
            try
            {
                var resultlist = await _service.GetSiblingDetailsData(request);
                if (resultlist == null)
                    return NotFound(ApiResponse<string>.Fail(" Not Found  View Student List"));
                return Ok(new ApiResponse<SiblingDetailsModel>
                {
                    Success = true,
                    Data = resultlist
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
        [HttpPost("AddUpdateSibling")]
        public async Task<IActionResult> AddUpdateSibling(AddSiblingRequest request)
        {
            var result = await _service.AddUpdateSiblingAsync(request);

            return Ok(new ApiResponse<int>
            {
                
                Success =true,
                Data = result,
                Message = result > 0 ? "Sibling mapped successfully.": "Operation failed.",
                Code=1
            });
        }
        [HttpPost("DeleteSiblingData")]
        public async Task<IActionResult> UpdateSiblingData([FromBody] SearchAnyRequestModel model)
        {
            try
            {
                var result = await _service.UpdateSiblingData(model);

                if (!result)
                {
                    return BadRequest(ApiResponse<string>.Fail(
                        "No record found to update."));
                }

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Sibling ID updated successfully.",
                    Data = "Success"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    ApiResponse<string>.Fail(
                        $"An error occurred: {ex.Message}"));
            }
        }
        [HttpPost]
        [Route("GetSiblingList")]
        public async Task<IActionResult> GetSiblingListData(SearchAnyRequestModel request)
        {
            try
            {
                var resultlist = await _service.GetSiblingListData(request);
                if (resultlist == null)
                    return NotFound(ApiResponse<string>.Fail(" Not Found  View Student List"));
                return Ok(resultlist);

            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
        [HttpPost]
        [Route("GetStudentAddressDetails")]
        public async Task<IActionResult> GetStudentAddressDetailsData([FromBody] SearchAnyRequestModel request)
        {
            if (request == null)
                return BadRequest("Invalid request.");
            try
            {
                var result = await _service.GetStudentAddressDetails(request);
                return Ok(new ApiResponse<StudentAddressDetailsModel>
                {
                    Success = true,
                    Message = "Get Address Successfullyy !",
                    Data = result
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An error occurred while fetching Student Address list.",
                    Error = ex.Message
                });
            }
        }
        [HttpPost]
        [Route("UpdateStudentPersonalDetails")]
        public async Task<IActionResult> UpdateStudentPersonalDetailData([FromBody] StudentViewDetailsModel request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Request data not found."
                });
            }
            try
            {
                var returnValue = await _service.AddOrUpdateStudentPersonalDetails(request);
                return returnValue switch
                {
                    -1 => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Admission No already exists.",
                        Code = -1
                    }),
                    -2 => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "GSRN No already exists.",
                        Code = -2
                    }),
                    -3 => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "EWS ID already exists.",
                        Code = -3
                    }),
                    -4 => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "APAAR ID already exists.",
                        Code = -4
                    }),
                    -5 => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "PEN No already exists.",
                        Code = -5
                    }),
                    2 => Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Student details Updated  successfully.",
                        Code = 2
                    }),
                    _ => Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unknown operation result.",
                        Code = -99
                    })
                };
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = ex.Message
                    });
            }

        }
        [HttpPost]
        [Route("GetStudentOtherDetail")]
        public async Task<IActionResult> GetOtherDetailsData(SearchAnyRequestModel request)
        {
            try
            {
                var resultlist = await _service.GetStudentOtherDetails(request);
                if (resultlist == null)
                    return NotFound(ApiResponse<string>.Fail(" Not Found  View Student List"));
                return Ok(resultlist);

            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
        [HttpPost("SaveStudentParentDetails")]
        public async Task<IActionResult> SaveStudentParentDetailsData([FromBody] StudentParentDetailsModel request)
        {
            try
            {
                var result = await _service.SaveStudentParentDetailsData(request);
                if (result > 0)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = true,
                        Message = "Parent Details Successfully",
                        Code = 2
                    });
                }
                else
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Parent Details Failed"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Status = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("SaveStudentAddress")]
        public async Task<IActionResult> SaveStudentAddressData([FromBody] StudentAddressDetailsModel request)
        {
            try
            {
                var result = await _service.SaveStudentAddressData(request);
                if (result > 0)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = true,
                        Message = "Save Address Successfully",
                        Code = 1
                    });
                }
                else
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Address Details Failed"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Status = false,
                    Message = ex.Message
                });
            }
        }
        [HttpPost("SavePassportDetails")]
        public async Task<IActionResult> SavePassportDetailsData([FromBody] StudentPassportVisaModel request)
        {
            try
            {
                var result = await _service.SavePassportDetailsData(request);
                if (result > 0)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = true,
                        Message = "Save Address Successfully",
                        Code = 1
                    });
                }
                else
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Address Details Failed"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Status = false,
                    Message = ex.Message
                });
            }
        }
        [HttpPost("GetStudentVisitors")]
        public async Task<IActionResult> GetStudentVisitorsData(SearchAnyRequestModel model)
        {
            try
            {
                var result = await _service.GetStudentVisitorsData(model);
                if (result == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "No Visitor Data.",
                        Data = null
                    });
                }
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while processing your request.",
                    Data = null
                });
            }
        }
        [HttpPost("SaveStudentVisitors")]
        public async Task<IActionResult> SaveStudentVisitorsData([FromBody] StudentVisitorsModel res)
        {
            if (res == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data.",
                    Code = 0
                });
            }
            try
            {
                int result = await _service.SaveStudentVisitorsData(res);
                if (result <= 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Student Visitor Details could not be saved.",
                        Code = 0
                    });
                }
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Student Visitor Details Saved Successfully.",
                    Code = 1
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = ex.Message,
                        Code = 0
                    });
            }
        }
        [HttpPost("RemoveProfileImage")]
        public async Task<IActionResult> RemoveProfileImage([FromBody] ProfileImageModal model)
        {
            try
            {
                var result = await _service.RemoveProfileImageData(model);
                if (result <= 0)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Profile image not found or already removed.",
                        Data = null,
                        Code = 0
                    });
                }
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Profile image removed successfully.",
                    Data = null,
                    Code = 1
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null,
                    Code = -1
                });
            }
        }

    }
}
