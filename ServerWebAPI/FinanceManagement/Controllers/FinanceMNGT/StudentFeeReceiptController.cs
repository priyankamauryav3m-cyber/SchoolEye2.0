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

        // ---------------- Print Multi Receipt: ticked receipts, 4 per A4 page ----------------
        // header -> V3M_FIN_GetStudentReceiptById, fee heads -> USP_FIN_GetReceiptDetail (same as the single receipt Print)
        private const int MaxMultiReceipts = 500;

        [HttpPost("GenerateMultiReceiptPdf")]
        public async Task<IActionResult> GenerateMultiReceiptPdf([FromBody] StudentMultiReceiptPrintRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.GroupCode) || string.IsNullOrWhiteSpace(request.BranchCode))
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Group code and branch code are required.", Code = 0 });
            }
            var receiptIds = (request.ReceiptIds ?? new List<long>()).Distinct().ToList();
            if (receiptIds.Count == 0 || receiptIds.Any(x => x <= 0))
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Please select receipt.", Code = 0 });
            }
            if (receiptIds.Count > MaxMultiReceipts)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = $"You can print maximum {MaxMultiReceipts} receipts at one time.", Code = 0 });
            }

            try
            {
                // a few receipts at a time so a large selection does not open too many connections
                using var throttle = new SemaphoreSlim(5);
                var tasks = receiptIds.Select(async receiptId =>
                {
                    await throttle.WaitAsync();
                    try
                    {
                        var byId = new StudentReceiptByIdRequest { GroupCode = request.GroupCode, BranchCode = request.BranchCode, ReceiptId = receiptId };
                        var header = await _service.GetStudentReceiptById(byId);
                        var details = header == null ? new List<ReceiptDetailResponse>() : (await _service.GetReceiptDetail(byId))?.ToList() ?? new();
                        return ToReceiptPrint(header, details, request.SessionNames);
                    }
                    finally
                    {
                        throttle.Release();
                    }
                });

                // keep the order of the ticked rows
                var receipts = (await Task.WhenAll(tasks)).Where(x => x != null).Select(x => x!).ToList();
                if (receipts.Count == 0)
                {
                    return Ok(new ApiResponse<string> { Success = false, Message = "Receipt details not found to print.", Code = 0 });
                }

                var pdfBytes = await _generateFile.GenerateMultiReceiptPdf(receipts);
                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Receipt PDF generated successfully.",
                    Code = 1,
                    Data = Convert.ToBase64String(pdfBytes)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Unable to generate the receipts.",
                    Code = -1
                });
            }
        }

        // same mapping as the single receipt Print of View Student Receipt; null when the receipt has no fee heads
        private static AdmitChildReceiptPrintModel? ToReceiptPrint(StudentReceiptByIdResponse? header, List<ReceiptDetailResponse> details, Dictionary<long, string>? sessionNames)
        {
            if (header == null || details.Count == 0)
                return null;

            var receipt = new AdmitChildReceiptPrintModel
            {
                ReceiptNo = header.ReceiptNo,
                ReceiptDate = header.CreatedDate ?? DateTime.Today,
                RegistrationNo = header.RegistrationId,
                StudentNo = header.StudentNo,
                StudentName = header.StudentName,
                FatherName = header.FatherName,
                ClassName = string.IsNullOrWhiteSpace(header.ClassSection) ? header.ClassName : header.ClassSection,
                SessionName = sessionNames != null && header.SessionId.HasValue && sessionNames.TryGetValue(header.SessionId.Value, out var sessionName) ? sessionName : "",
                PeriodName = header.PeriodName,
                PaymentMode = header.PaymentMode,
                TotalAmount = header.PayableAmount ?? 0,
                Concession = 0,
                PaidAmount = header.PaidAmount ?? 0,
                Balance = header.Balance ?? 0,
                AmountHeader = "Payable",
                FeeHeads = details
                    .Select(x => new AdmitChildReceiptPrintFeeHead { FeeHeadName = x.HeadName, Amount = x.TotalAmount ?? 0 })
                    .ToList()
            };
            // late fee is not a fee head row, show it as its own line like the single receipt Print
            if ((header.LateFee ?? 0) > 0)
                receipt.FeeHeads.Add(new AdmitChildReceiptPrintFeeHead { FeeHeadName = "Late Fee", Amount = header.LateFee ?? 0 });
            return receipt;
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
            if (request.SettlementMode > 2)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Please select Keep as Advance or Refund to Parent.", Code = 0 });
            }
            if (request.SettlementMode == 2 && (string.IsNullOrWhiteSpace(request.OtpCode) || request.OtpCode.Trim().Length != 6 || !request.OtpCode.Trim().All(char.IsDigit)))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Please enter the 6 digit OTP sent to the parent.", Code = 0 });
            }
            try
            {
                request.CancelReason = request.CancelReason.Trim();
                request.OtpCode = request.OtpCode?.Trim();
                var result = await _service.CancelStudentReceipt(request);
                if (result == 1)
                {
                    var message = request.SettlementMode switch
                    {
                        1 => "Receipt cancelled. Amount kept as student advance, it will be adjusted in the next fee.",
                        2 => "Receipt cancelled. Amount refunded to the parent.",
                        _ => "Receipt cancelled successfully."
                    };

                    // refund done: confirmation mail with the refund receipt to the parent.
                    // The refund is already saved, so a mail failure is only reported, it does not undo the refund.
                    if (request.SettlementMode == 2)
                    {
                        try
                        {
                            var mailedTo = await _service.SendRefundConfirmation(new StudentReceiptByIdRequest
                            {
                                GroupCode = request.GroupCode,
                                BranchCode = request.BranchCode,
                                ReceiptId = request.ReceiptId
                            });
                            message += mailedTo != null ? $" Refund receipt mailed to {mailedTo}." : " Parent email not found, refund receipt was not mailed.";
                        }
                        catch (Exception mailEx)
                        {
                            Console.WriteLine($"Exception: {mailEx.Message}");
                            message += " Refund receipt mail could not be sent.";
                        }
                    }
                    return Ok(new ApiResponse<object> { Success = true, Message = message, Code = 1, Data = result });
                }
                if (result == -1)
                {
                    return Ok(new ApiResponse<object> { Success = false, Message = "Receipt is invalid or already cancelled.", Code = -1, Data = result });
                }
                if (result is -3 or -4 or -5)
                {
                    var message = result switch
                    {
                        -3 => "Invalid OTP.",
                        -4 => "OTP has expired or was not sent. Please send OTP again.",
                        _ => "Too many wrong OTP attempts. Please send OTP again."
                    };
                    return Ok(new ApiResponse<object> { Success = false, Message = message, Code = result, Data = result });
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

        // ---------------- Cancel -> Refund to Parent: V3M_FIN_GetReceiptRefundContacts / V3M_FIN_SaveReceiptRefundOtp ----------------
        // contacts are returned masked; the OTP is mailed by the server and never returned
        [HttpPost("GetReceiptRefundContacts")]
        public async Task<IActionResult> GetReceiptRefundContacts([FromBody] StudentReceiptByIdRequest request)
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
                var data = await _service.GetReceiptRefundContacts(request);
                if (data == null)
                {
                    return Ok(new ApiResponse<object> { Success = false, Message = "Receipt is invalid or already cancelled.", Code = -1 });
                }
                return Ok(new ApiResponse<ReceiptRefundContactsResponse>
                {
                    Success = true,
                    Message = data.HasEmail || data.HasMobile ? "Parent contacts retrieved successfully." : "No email or SMS mobile no. found for the parent.",
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
                    Message = "An error occurred while fetching the parent contacts.",
                    Code = -1
                });
            }
        }

        [HttpPost("SendReceiptRefundOtp")]
        public async Task<IActionResult> SendReceiptRefundOtp([FromBody] SendReceiptRefundOtpRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.GroupCode) || string.IsNullOrWhiteSpace(request.BranchCode))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Group code and branch code are required.", Code = 0 });
            }
            if (request.ReceiptId <= 0)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Please select receipt.", Code = 0 });
            }
            if (!string.Equals(request.Channel, "Email", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(request.Channel, "Mobile", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Please select where the OTP is sent.", Code = 0 });
            }
            if (string.IsNullOrWhiteSpace(request.RequestedBy))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "User not found.", Code = 0 });
            }
            try
            {
                var data = await _service.SendReceiptRefundOtp(request);
                var message = data.Result switch
                {
                    1 => $"OTP sent to {data.SentTo}. It is valid for {data.ValidMinutes} minutes.",
                    -1 => "Receipt is invalid or already cancelled.",
                    -2 => "OTP was sent just now. Please wait 30 seconds before sending again.",
                    -3 => "Selected contact is not available for this parent.",
                    -4 => "SMS OTP is not available yet. Please send the OTP on email.",
                    _ => "Unable to send OTP."
                };
                return Ok(new ApiResponse<SendReceiptRefundOtpResponse>
                {
                    Success = data.Result == 1,
                    Message = message,
                    Code = data.Result,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Unable to send OTP. Please try again.",
                    Code = -1
                });
            }
        }

        // refund receipt PDF (base64) of a receipt cancelled with "Refund to Parent", built from the database
        [HttpPost("GenerateRefundReceiptPdf")]
        public async Task<IActionResult> GenerateRefundReceiptPdf([FromBody] StudentReceiptByIdRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.GroupCode) || string.IsNullOrWhiteSpace(request.BranchCode))
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Group code and branch code are required." });
            }
            if (request.ReceiptId <= 0)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Please select receipt." });
            }
            try
            {
                var pdfBytes = await _service.GenerateRefundReceiptPdf(request);
                if (pdfBytes == null)
                {
                    return Ok(new ApiResponse<string> { Success = false, Message = "Refund receipt is available only for a refunded receipt." });
                }
                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = Convert.ToBase64String(pdfBytes),
                    Message = "PDF generated successfully"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Unable to generate the refund receipt."
                });
            }
        }

        // ---------------- Edit receipt: V3M_FIN_GetStudentReceiptForEdit / V3M_FIN_EditStudentReceipt ----------------
        [HttpPost("GetStudentReceiptForEdit")]
        public async Task<IActionResult> GetStudentReceiptForEdit([FromBody] StudentReceiptByIdRequest request)
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
                var data = await _service.GetStudentReceiptForEdit(request);
                if (data?.Header == null)
                {
                    return NotFound(new ApiResponse<object> { Success = false, Message = "Receipt record was not found.", Code = 0 });
                }
                return Ok(new ApiResponse<StudentReceiptForEditResponse>
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

        [HttpPost("EditStudentReceipt")]
        public async Task<IActionResult> EditStudentReceipt([FromBody] EditStudentReceiptRequest request)
        {
            var validationMessage = ValidateEditReceipt(request);
            if (validationMessage != null)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = validationMessage, Code = 0 });
            }
            try
            {
                request.EditReason = request.EditReason!.Trim();
                request.Remark = request.Remark?.Trim() ?? string.Empty;
                request.ChequeNo = request.ChequeNo?.Trim() ?? string.Empty;
                request.ChequeBank = request.ChequeBank?.Trim() ?? string.Empty;
                request.BankBranch = request.BankBranch?.Trim() ?? string.Empty;

                var result = await _service.EditStudentReceipt(request);
                var message = result.Result switch
                {
                    1 => $"Receipt edited successfully. Revised Receipt No: {result.NewReceiptNo}",
                    2 => "Receipt payment details updated successfully.",
                    3 => "Receipt cancelled. No fee head was left to re-issue.",
                    -1 => "Receipt is invalid or already cancelled.",
                    -2 => "Only the latest receipt of the student can be edited.",
                    -3 => "A fee head of this receipt was already removed through Edit Invoice (its paid amount is kept as student advance). This receipt cannot be edited.",
                    -4 => "One or more fee heads are not valid dues of this student.",
                    -5 => "Paid amount cannot be more than the due amount of a fee head.",
                    -6 => "A fee head with paid amount cannot be removed from the invoice.",
                    -7 => "Refund amount cannot be more than the amount received.",
                    -8 => "No changes found to update.",
                    _ => null
                };
                if (message == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                    {
                        Success = false,
                        Message = "An error occurred while editing the receipt.",
                        Code = -1
                    });
                }
                return Ok(new ApiResponse<EditStudentReceiptResponse>
                {
                    Success = result.Result > 0,
                    Message = message,
                    Code = result.Result,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while editing the receipt.",
                    Code = -1
                });
            }
        }

        private static string? ValidateEditReceipt(EditStudentReceiptRequest? request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.GroupCode) || string.IsNullOrWhiteSpace(request.BranchCode))
                return "Group code and branch code are required.";
            if (request.ReceiptId <= 0)
                return "Please select receipt.";
            if (string.IsNullOrWhiteSpace(request.EditedBy))
                return "User not found.";
            if (string.IsNullOrWhiteSpace(request.EditReason))
                return "Edit reason is required.";
            if (request.EditReason.Trim().Length > 250)
                return "Edit reason cannot exceed 250 characters.";
            if ((request.Remark?.Trim().Length ?? 0) > 200)
                return "Remarks cannot exceed 200 characters.";
            if (string.IsNullOrWhiteSpace(request.PaymentMode))
                return "Please select payment mode.";
            if (request.ReceiptDate == null || request.ReceiptDate.Value.Date > DateTime.Today)
                return "Please select a valid receipt date.";
            if (request.DifferenceMode != 1 && request.DifferenceMode != 2)
                return "Please select how the difference amount is settled.";
            if (request.Lines == null || request.Lines.Any(x => x.DetInvoiceId <= 0 || x.PaidAmount < 0))
                return "Fee head details are not valid.";
            if (request.Lines.GroupBy(x => x.DetInvoiceId).Any(g => g.Count() > 1))
                return "A fee head is selected more than once.";
            if (request.Lines.Any(x => x.RemoveFromInvoice && x.PaidAmount > 0))
                return "A fee head with paid amount cannot be removed from the invoice.";
            return null;
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
