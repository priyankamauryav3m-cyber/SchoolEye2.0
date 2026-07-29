using ApplicationInterface.FinanceMNGT.FeeMNGT;
using ApplicationInterface.GenerateFile;
using DomainModel.FinanceMNGT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.FinanceManagement.Controllers.FinanceMNGT
{
    [ApiExplorerSettings(GroupName = "FinanceManagement")]
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentFeeInvoiceController : ControllerBase
    {
        private readonly IStudentInvoiceRepository _service;
        private readonly IPrintStudentRec _generateFile;

        public StudentFeeInvoiceController(IStudentInvoiceRepository service, IPrintStudentRec printStudentRec)
        {
            _service = service;
            _generateFile = printStudentRec;
        }

        [HttpPost]
        [Route("GetForStudentGenerateInvoice")]
        public async Task<IActionResult> GetPromotionConcessionStudent(StudentForInvoiceRequestModel requestModel)
        {

            try
            {

                var resultlist = await _service.GetStudentForInvoiceGenerate(requestModel);

                if (resultlist == null || !resultlist.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<StudentFeeInvoiceResponseModel>>
                    {
                        Code = 0,
                        Success = true,
                        Message = "No Promote List Found",
                        Data = Enumerable.Empty<StudentFeeInvoiceResponseModel>()
                    });
                }

                return Ok(new ApiResponse<IEnumerable<StudentFeeInvoiceResponseModel>>
                {
                    Success = true,
                    Data = resultlist,

                });

            }
            catch (Exception ex)
            {

                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));

            }
        }
        [HttpPost("StudentChallanGenerate")]
        public async Task<IActionResult> SaveStudentChallanGenerate([FromBody] StudentInvoice request)
        {
            try
            {
                var result = await _service.SaveStudentChallanGenerateData(request);
                if (result > 0)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = true,
                        Message = "Challan Generated Successfully",
                        Code = 1
                    });
                }
                else
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Challan Generation Failed"
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

        [HttpGet("GetInvoiceTypeList")]
        public async Task<IActionResult> GetInvoiceTypeList()
        {
            try
            {
                var data = await _service.GetInvoiceTypeList();
                return Ok(data);

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
        [HttpPost("ClassBasedStudent")]
        public async Task<IActionResult> GetClassBasedStudent(SearchAnyRequestModel searchAnyRequest)
        {
            try
            {
                var result = await _service.GetStudentsByClassAsync(searchAnyRequest);
                if (result == null || !result.Any())
                    return NotFound("No students found.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("GetStudentInvoiceDues")]
        public async Task<IActionResult> GetStudentInvoiceDuesData(StudentInvoiceDuesRequest request)
        {
            try
            {
                var resultlist = await _service.GetStudentInvoiceDuesData(request);
                if (resultlist == null || !resultlist.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<StudentDuesModel>>
                    {
                        Code = 0,
                        Success = true,
                        Message = "No Student List Found",
                        Data = Enumerable.Empty<StudentDuesModel>()
                    });
                }
                return Ok(new ApiResponse<IEnumerable<StudentDuesModel>>
                {
                    Success = true,
                    Data = resultlist,

                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
        [HttpPost("GetStudentInvoiceDetails")]
        public async Task<IActionResult> GetInvoiceDetails(SearchAnyRequestModel requestModel)
        {
            var result = await _service.GetInvoiceDetailsAsync(requestModel);
            if (result == null)
            {
                return Ok(new ApiResponse<IEnumerable<InvoiceDetailsResponse>>
                {
                    Code = 0,
                    Success = true,
                    Message = "No  List Found",
                    Data = Enumerable.Empty<InvoiceDetailsResponse>()
                });
            }
            var pdfBytes = await _generateFile.GenerateStudentInvoicePrintPdf(result.ToList());
            var base64Pdf = Convert.ToBase64String(pdfBytes);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = base64Pdf,
                Message = "PDFList generated successfully"
            });
        }

        [HttpPost("GetStudentAdvanceBalance")]
        public async Task<IActionResult> GetStudentAdvanceDuesData(SearchStudentBalanceDto request)
        {
            try
            {
                var resultlist = await _service.GetStudentAdvanceBalanceData(request);
                if (resultlist == null || !resultlist.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<SearchStudentBalanceDto>>
                    {
                        Code = 0,
                        Success = true,
                        Message = "No Student List Found",
                        Data = Enumerable.Empty<SearchStudentBalanceDto>()
                    });
                }
                return Ok(new ApiResponse<IEnumerable<SearchStudentBalanceDto>>
                {
                    Success = true,
                    Data = resultlist,

                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));
            }
        }
        [HttpPost("GetStudentInvoicePreview")]
        public async Task<IActionResult> GetInvoiceDetailsPreview(SearchAnyRequestModel requestModel)
        {
            var result = await _service.GetInvoiceDetailsAsync(requestModel);
            if (result == null)
            {
                return Ok(new ApiResponse<IEnumerable<InvoiceDetailsResponse>>
                {
                    Code = 0,
                    Success = true,
                    Message = "No  List Found",
                    Data = Enumerable.Empty<InvoiceDetailsResponse>()
                });
            }
            return Ok(new ApiResponse<IEnumerable<InvoiceDetailsResponse>>
            {
                Success = true,
                Data = result,
                Message = "PDFList generated successfully"
            });
        }
        [HttpPost("StudentUpdateInvoiceDue")]
        public async Task<IActionResult> StudentUpdateChallanDueDate([FromBody] ChallanDueDateModal request)
        {

            try
            {
                if (request == null)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Data not found."
                    });
                var result = await _service.StudentUpdateChallanDueDate(request);
                if (result == 1)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Challan Due Date Updated Successfully.",
                        Code = 1,
                        Data = result
                    });
                }
                else
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Challan Due Date could not be updated.",
                        Code = 0,
                        Data = result
                    });
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while updating the Challan Due Date."
                    });
            }
        }
        [HttpPost]
        [Route("GetFeeHeadWithAmount")]
        public async Task<IActionResult> GetFeeHeadAmountData(SearchAnyRequestModel requestModel)
        {

            try
            {

                var resultlist = await _service.GetFeeHeadDropdown(requestModel);

                if (resultlist == null || !resultlist.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<FeeHeadDropdownModel>>
                    {
                        Code = 0,
                        Success = true,
                        Message = "No  List Found",
                        Data = Enumerable.Empty<FeeHeadDropdownModel>()
                    });
                }

                return Ok(new ApiResponse<IEnumerable<FeeHeadDropdownModel>>
                {
                    Success = true,
                    Data = resultlist,

                });

            }
            catch (Exception ex)
            {

                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));

            }
            

        }
        [HttpPost("GetTransportMonthWithStudent")]
        public async Task<IActionResult> GetDistanceData(SearchAnyRequestModel searchAnyRequest)
        {
            try
            {
                var data = await _service.GetMonthWithTranspoet(searchAnyRequest);
                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<TransportSelectMonthModel>>
                    {
                        Code = 0,
                        Success = true,
                        Message = "No  List Found",
                        Data = Enumerable.Empty<TransportSelectMonthModel>()
                    });
                }

                return Ok(new ApiResponse<IEnumerable<TransportSelectMonthModel>>
                {
                    Success = true,
                    Data = data,

                });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = ex.Message }
                );
            }
        }
        [HttpPost("AddFeeHeadToStudentChallan")]
        public async Task<IActionResult> AddFeeHeadToStudentChallanData([FromBody] FeeHeadToStudentChallan request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Request data is required."
                });
            }
            try
            {
                var result = await _service.AddFeeHeadToStudentChallanData(request);
                if (result == 1)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Code = 1,
                        Message = "Fee Head added to student challan successfully."
                    });
                }
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Code = 0,
                    Message = "Unable to add Fee Head to student challan."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Code = 500,
                        Message = ex.Message
                    });
            }
        }
        [HttpPost("RemoveFeeHeadToStudentChallan")]
        public async Task<IActionResult> RemoveFeeHeadToStudentChallan([FromBody] FeeHeadToStudentChallan request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Request data is required."
                });
            }
            try
            {
                var result = await _service.RemoveFeeHeadToStudentChallanData(request);
                if (result == 1)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Code = 1,
                        Message = "Fee Head added to student challan successfully."
                    });
                }
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Code = 0,
                    Message = "Unable to add Fee Head to student challan."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Code = 500,
                        Message = ex.Message
                    });
            }
        }
    }
}
