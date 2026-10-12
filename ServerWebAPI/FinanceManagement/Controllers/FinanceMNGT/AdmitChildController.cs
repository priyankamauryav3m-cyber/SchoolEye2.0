using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using SchoolEye.ApplicationInterface.Admin;
using System;
using System.Threading.Tasks;


namespace SchoolEye.ServerWebAPI.Addmission.Controllers.Admin
{
    [Authorize]
    [ApiExplorerSettings(GroupName = "FinanceManagement")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdmitChildController : ControllerBase
    {
        private readonly IAdmitChildRepository _service;

        public AdmitChildController(IAdmitChildRepository service)
        {
            _service = service;
        }

        [HttpPost("AdmitChildData")]
        public async Task<IActionResult> AdmitChild([FromBody] AdmitChildsRequest request)
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
                var result = await _service.AdmitChild(request);
                return result.ReturnValue switch
                {
                    1 => Ok(new ApiResponse<AdmitChildResult>
                    {
                        Success = true,
                        Message = "Child admitted successfully.",
                        Code = 1,
                        Data = result
                    }),
                    0 => BadRequest(new ApiResponse<AdmitChildResult>
                    {
                        Success = false,
                        Message = "Student could not be admitted. No student number was generated.",
                        Code = 0,
                        Data = null
                    }),
                    _ => StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<AdmitChildResult>
                    {
                        Success = false,
                        Message = "Admission failed and was rolled back.",
                        Code = -1,
                        Data = null
                    })
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while admitting the child.",
                    Code = -1
                });
            }
        }
        [HttpPost("GetRegistrationDetails")]
        public async Task<IActionResult> GetRegistrationDetails([FromBody] RegistrationDetailsRequest request)
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
                var data = await _service.GetRegistrationDetails(request);

                if (data == null)
                {
                    return Ok(new ApiResponse<RegistrationDetailsResponse>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = null
                    });
                }

                return Ok(new ApiResponse<RegistrationDetailsResponse>
                {
                    Success = true,
                    Message = "Registration details retrieved successfully.",
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
                    Message = "An error occurred while fetching the registration details.",
                    Code = -1
                });
            }
        }
        [HttpPost("GetClassMapTemplateFeeHead")]
        public async Task<IActionResult> GetClassMapTemplateFeeHead([FromBody] ClassMapTemplateFeeHeadRequest request)
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
                var data = await _service.GetClassMapTemplateFeeHead(request);

                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<ClassMapTemplateFeeHeadResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<ClassMapTemplateFeeHeadResponse>()
                    });
                }

                return Ok(new ApiResponse<IEnumerable<ClassMapTemplateFeeHeadResponse>>
                {
                    Success = true,
                    Message = "Class fee heads retrieved successfully.",
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
                    Message = "An error occurred while fetching the class fee heads.",
                    Code = -1
                });
            }
        }
        [HttpPost("AdmitChildWithReceipt")]
        public async Task<IActionResult> AdmitChildWithReceipt([FromBody] AdmitChildWithReceiptRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RegistrationId) || request.SessionId <= 0
                || request.PeriodId <= 0 || request.FeeHeads == null || request.FeeHeads.Count == 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Data not found.",
                    Code = 0
                });
            }
            if (request.PaidAmount < 0 || request.PaidAmount > request.TotalAmount - request.Concession)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Paid amount can not be more than payable amount.",
                    Code = 0
                });
            }
            try
            {
                var result = await _service.AdmitChildWithReceipt(request);
                return result.ReturnValue switch
                {
                    1 => Ok(new ApiResponse<AdmitChildWithReceiptResponse>
                    {
                        Success = true,
                        Message = "Child admitted successfully.",
                        Code = 1,
                        Data = result
                    }),
                    2 => BadRequest(new ApiResponse<AdmitChildWithReceiptResponse>
                    {
                        Success = false,
                        Message = "Receipt could not be generated. Admission was rolled back.",
                        Code = 0,
                        Data = null
                    }),
                    _ => BadRequest(new ApiResponse<AdmitChildWithReceiptResponse>
                    {
                        Success = false,
                        Message = "Student could not be admitted. No student number was generated.",
                        Code = 0,
                        Data = null
                    })
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while admitting the child. Nothing was saved.",
                    Code = -1
                });
            }
        }
    }
}
