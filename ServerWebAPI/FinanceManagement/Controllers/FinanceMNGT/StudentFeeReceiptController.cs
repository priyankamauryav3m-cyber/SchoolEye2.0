using ApplicationInterface.FinanceMNGT.FeeMNGT;
using ApplicationInterface.GenerateFile;
using DomainModel.FinanceMNGT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using SchoolEye.ApplicationInterface.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ServerWebAPI.FinanceManagement.Controllers.FinanceMNGT
{
    [Authorize]
    [ApiExplorerSettings(GroupName = "FinanceManagement")]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentFeeReceiptController : ControllerBase
    {
        private readonly IStudentFeeReceiptRepository _service;
        private readonly IPrintStudentRec _generateFile;

        public StudentFeeReceiptController(IStudentFeeReceiptRepository service, IPrintStudentRec generateFile = null)
        {
            _service = service;
            _generateFile = generateFile;
        }

        [HttpPost("GetStudentFeeReceiptListData")]
        public async Task<IActionResult> GetStudentFeeReceiptList([FromBody] StudentFeeReceiptListRequest request)
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
                var data = await _service.GetStudentFeeReceiptList(request);
                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<StudentFeeReceiptListResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<StudentFeeReceiptListResponse>()
                    });
                }
                return Ok(new ApiResponse<IEnumerable<StudentFeeReceiptListResponse>>
                {
                    Success = true,
                    Message = "Student fee receipt list retrieved successfully.",
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
                    Message = "An error occurred while fetching the student fee receipt list.",
                    Code = -1
                });
            }
        }

        [HttpPost("GetClassGroup")]
        public async Task<IActionResult> GetClassGroup([FromBody] ClassGroupRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.GroupCode) || string.IsNullOrWhiteSpace(request.BranchCode))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Group code and branch code are required.",
                    Code = 0
                });
            }
            try
            {
                var data = await _service.GetClassGroup(request);
                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<ClassGroupResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<ClassGroupResponse>()
                    });
                }
                return Ok(new ApiResponse<IEnumerable<ClassGroupResponse>>
                {
                    Success = true,
                    Message = "Class group retrieved successfully.",
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
                    Message = "An error occurred while fetching the class group.",
                    Code = -1
                });
            }
        }

        [HttpPost("GetStudentReceiptById")]
        public async Task<IActionResult> GetStudentReceiptById([FromBody] StudentReceiptByIdRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.GroupCode) || string.IsNullOrWhiteSpace(request.BranchCode))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Group code and branch code are required.", Code = 0 });
            }
            if (request.ReceiptId <= 0)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Please select receipt.", Code = 0 });
            }
            try
            {
                var data = await _service.GetStudentReceiptById(request);
                if (data == null)
                {
                    return NotFound(new ApiResponse<object> { Success = false, Message = "Receipt record was not found.", Code = 0 });
                }
                return Ok(new ApiResponse<StudentReceiptByIdResponse>
                {
                    Success = true,
                    Message = "Receipt retrieved successfully.",
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
                    Message = "An error occurred while fetching the receipt.",
                    Code = -1
                });
            }
        }

        [HttpPost("GetReceiptDetail")]
        public async Task<IActionResult> GetReceiptDetail([FromBody] StudentReceiptByIdRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.GroupCode) || string.IsNullOrWhiteSpace(request.BranchCode))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Group code and branch code are required.", Code = 0 });
            }
            if (request.ReceiptId <= 0)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Please select receipt.", Code = 0 });
            }
            try
            {
                var data = await _service.GetReceiptDetail(request);
                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<ReceiptDetailResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<ReceiptDetailResponse>()
                    });
                }
                return Ok(new ApiResponse<IEnumerable<ReceiptDetailResponse>>
                {
                    Success = true,
                    Message = "Receipt details retrieved successfully.",
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
                    Message = "An error occurred while fetching the receipt details.",
                    Code = -1
                });
            }
        }
        [HttpPost("GenerateReceiptChqBounceChallan")]
        public async Task<IActionResult> GenerateReceiptChqBounceChallan([FromBody] ReceiptChqBounceRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.GroupCode) || string.IsNullOrWhiteSpace(request.BranchCode))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Group code and branch code are required.", Code = 0 });
            }
            if (request.ReceiptId <= 0)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Please select receipt.", Code = 0 });
            }
            if (request.CancelType != 1 && request.CancelType != 2)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Please select cancel type.", Code = 0 });
            }
            if (request.CancelledDate == null || request.CancelledDate == default(DateTime))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Please select date.", Code = 0 });
            }
            if (string.IsNullOrWhiteSpace(request.Remark))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Cancel reason is required.", Code = 0 });
            }
            if (request.Remark.Trim().Length > 250)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Cancel reason cannot exceed 250 characters.", Code = 0 });
            }
            if (string.IsNullOrWhiteSpace(request.CancelledBy))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "User not found.", Code = 0 });
            }
            try
            {
                request.Remark = request.Remark.Trim();
                var result = await _service.GenerateReceiptChqBounceChallan(request);
                return result switch
                {
                    1 => Ok(new ApiResponse<object> { Success = true, Message = "Receipt cancelled successfully.", Code = 1, Data = result }),
                    -2 => Ok(new ApiResponse<object> { Success = false, Message = "Receipt is invalid or already cancelled.", Code = -2, Data = result }),
                    -22 => Ok(new ApiResponse<object> { Success = false, Message = "Cheque bounce is already generated for this receipt.", Code = -22, Data = result }),
                    _ => StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while cancelling the receipt.",
                        Code = -1
                    })
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while cancelling the receipt.",
                    Code = -1
                });
            }
        }

        [HttpPost("CancelStudentReceipt")]
        public async Task<IActionResult> CancelStudentReceipt([FromBody] CancelStudentReceiptRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.GroupCode) || string.IsNullOrWhiteSpace(request.BranchCode))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Group code and branch code are required.", Code = 0 });
            }
            if (request.ReceiptId <= 0)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Please select receipt.", Code = 0 });
            }
            if (string.IsNullOrWhiteSpace(request.CancelReason))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Cancel reason is required.", Code = 0 });
            }
            if (request.CancelReason.Trim().Length > 250)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Cancel reason cannot exceed 250 characters.", Code = 0 });
            }
            if (string.IsNullOrWhiteSpace(request.CancelledBy))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "User not found.", Code = 0 });
            }
            try
            {
                request.CancelReason = request.CancelReason.Trim();
                var result = await _service.CancelStudentReceipt(request);
                if (result == 1)
                {
                    return Ok(new ApiResponse<object> { Success = true, Message = "Receipt cancelled successfully.", Code = 1, Data = result });
                }
                if (result == -1)
                {
                    return Ok(new ApiResponse<object> { Success = false, Message = "Receipt is invalid or already cancelled.", Code = -1, Data = result });
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while cancelling the receipt.",
                    Code = -1
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while cancelling the receipt.",
                    Code = -1
                });
            }
        }

        // ---------------- Chq Report: Chq Combine (Fin_GetOnlyChqCombineReport) / Print (V3M_FIN_PrintViewStudentReceipts) ----------------
        // only one status at a time (Active = 1 or Cancelled = 0), PDF / Excel as base64

        [HttpPost("GenerateChqCombinePdf")]
        public Task<IActionResult> GenerateChqCombinePdf([FromBody] StudentChequeReportRequest request) =>
            GenerateChequeReport(request, async () =>
            {
                var data = (await _service.GetChqCombineReport(request)).ToList();
                return data.Count == 0 ? null : await _generateFile.GenerateChqCombinePdf(request, data);
            }, "Cheque combine PDF generated successfully.");

        [HttpPost("GenerateChqCombineExcel")]
        public Task<IActionResult> GenerateChqCombineExcel([FromBody] StudentChequeReportRequest request) =>
            GenerateChequeReport(request, async () =>
            {
                var data = (await _service.GetChqCombineReport(request)).ToList();
                return data.Count == 0 ? null : await _generateFile.GenerateChqCombineExcel(request, data);
            }, "Cheque combine Excel generated successfully.");

        [HttpPost("GeneratePrintViewReceiptsPdf")]
        public Task<IActionResult> GeneratePrintViewReceiptsPdf([FromBody] StudentChequeReportRequest request) =>
            GenerateChequeReport(request, async () =>
            {
                var data = (await _service.GetPrintViewStudentReceipts(request)).ToList();
                return data.Count == 0 ? null : await _generateFile.GeneratePrintViewReceiptsPdf(request, data);
            }, "Receipt PDF generated successfully.");

        [HttpPost("GeneratePrintViewReceiptsExcel")]
        public Task<IActionResult> GeneratePrintViewReceiptsExcel([FromBody] StudentChequeReportRequest request) =>
            GenerateChequeReport(request, async () =>
            {
                var data = (await _service.GetPrintViewStudentReceipts(request)).ToList();
                return data.Count == 0 ? null : await _generateFile.GeneratePrintViewReceiptsExcel(request, data);
            }, "Receipt Excel generated successfully.");

        // shared validation / response of the four cheque report endpoints; generate returns null when there is no data
        private async Task<IActionResult> GenerateChequeReport(StudentChequeReportRequest request, Func<Task<byte[]?>> generate, string successMessage)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.GroupCode) || string.IsNullOrWhiteSpace(request.BranchCode))
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Group code and branch code are required.", Code = 0 });
            }
            if (request.Status != 1 && request.Status != 0)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = "You cannot print both(Active & Cancelled) receipt at one time. Please select any one.", Code = 0 });
            }
            if (request.FromDate == default || request.ToDate == default || request.FromDate.Date > request.ToDate.Date)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Please provide a valid date range.", Code = 0 });
            }
            if (!string.IsNullOrWhiteSpace(request.ReceiptIds) &&
                request.ReceiptIds.Split(',').Any(x => !long.TryParse(x.Trim(), out var id) || id <= 0))
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Invalid receipt selection.", Code = 0 });
            }

            // All cheque types = "" (both SPs); "0" in V3M_FIN_PrintViewStudentReceipts would drop non-cheque receipts
            request.ChqType = string.IsNullOrWhiteSpace(request.ChqType) || request.ChqType.Trim() == "0" ? "" : request.ChqType.Trim();
            request.ReceiptIds = request.ReceiptIds?.Trim() ?? "";

            try
            {
                var bytes = await generate();
                if (bytes == null)
                {
                    return Ok(new ApiResponse<string> { Success = false, Message = "No cheque records found.", Code = 0 });
                }
                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = successMessage,
                    Code = 1,
                    Data = Convert.ToBase64String(bytes)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Unable to generate the cheque report.",
                    Code = -1
                });
            }
        }

        [HttpPost("GenerateStudentReceiptHeadWisePdf")]
        public async Task<IActionResult> GenerateStudentReceiptHeadWisePdf([FromBody] StudentReceiptHeadWiseRequest request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Data = null,
                    Message = "Invalid request."
                });
            }

            if (request.FromDate == default || request.ToDate == default ||request.FromDate.Date > request.ToDate.Date)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Data = null,
                    Message = "Please provide a valid date range."
                });
            }

            try
            {

                
                var data = await _service.GetWithModeReceiptHeadWiseDetail(request);

                var pdfBytes = await _generateFile.GenerateStudentReceiptHeadWisePdf(request, data);

                var base64Pdf = Convert.ToBase64String(pdfBytes);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = base64Pdf,
                    Message = "Student Receipt Head-Wise PDF generated successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Data = null,
                    Message = $"PDF generation failed: {ex.Message}"
                });
            }
        }

        [HttpPost("GenerateStudentReceiptHeadWiseExcel")]
        public async Task<IActionResult> GenerateStudentReceiptHeadWiseExcel([FromBody] StudentReceiptHeadWiseRequest request)
        {
            if (request == null)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Data = null,
                    Message = "Invalid request."
                });
            }

            if (request.FromDate == default ||
                request.ToDate == default ||
                request.FromDate.Date > request.ToDate.Date)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Data = null,
                    Message = "Please provide a valid date range."
                });
            }

            try
            {
                var data = await _service.GetWithModeReceiptHeadWiseDetail(request);

                var excelBytes = await _generateFile
                    .GenerateStudentReceiptHeadWiseExcel(request, data);

                var base64Excel = Convert.ToBase64String(excelBytes);

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = base64Excel,
                    Message = "Student Receipt Head-Wise Excel generated successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Data = null,
                    Message = $"Excel generation failed: {ex.Message}"
                });
            }
        }

    }
}
