using ApplicationInterface.FinanceMNGT.FeeMNGT;
using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using static DomainModel.FinanceMNGT.FeeCollectionDuesNew;

namespace ServerWebAPI.FinanceManagement.Controllers.FinanceMNGT
{
    [ApiExplorerSettings(GroupName = "FinanceManagement")]
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StudentFeeController : ControllerBase
    {
        private readonly IStudentFeeRepository _studentFeeRepository;

        public StudentFeeController(IStudentFeeRepository studentFeeRepository)
        {
            _studentFeeRepository = studentFeeRepository;
        }

        [HttpPost("GetStudentDetailsForFee")]
        public async Task<IActionResult> GetStudentDetailsForFee([FromBody] StudentDetailsForFeeRequest request)
        {
         
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResponse<List<StudentDetailsForFee>>
                    {
                        Code = 0,
                        Message = "Invalid request.",   
                        Data = null
                    });
                }

                var response = await _studentFeeRepository.GetStudentDetailsForFeeAsync(request);

                if (response == null)

                    return NotFound(ApiResponse<string>.Fail("No student List  found"));
                return Ok(new ApiResponse<IEnumerable<StudentDetailsForFee>>
                {
                    Success = true,
                    Data = response
                });

            }
            catch (Exception ex)
            {

                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));

            }
        }

        [HttpPost("GetStudentFeeCollectionDues")]
        public async Task<IActionResult> GetStudentDetailsForFeeCollection([FromBody] GetStudentFeeHeadDuesForAdjustmentRequest request)
        {

            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResponse<List<FeeCollectionDuesNew>>
                    {
                        Code = 0,
                        Message = "Invalid request.",
                        Data = null
                    });
                }
                var response = await _studentFeeRepository.GetStudentDetailsForFeeDue(request);
                if (response == null)
                    return NotFound(ApiResponse<string>.Fail("No  List  found"));
                return Ok(new ApiResponse<IEnumerable<FeeCollectionDuesNew>>
                {
                    Success = true,
                    Data = response
                });

            }
            catch (Exception ex)
            {

                return StatusCode(500, ApiResponse<string>.Fail($"An error occurred: {ex.Message}"));

            }
        }
        // partly paid months of a student with every fee head line (paid in full too) - Fee Collection month summary
        [HttpPost("GetStudentPartPaidMonths")]
        public async Task<IActionResult> GetStudentPartPaidMonths([FromBody] GetStudentFeeHeadDuesForAdjustmentRequest request)
        {
            try
            {
                if (request == null || request.StudentId <= 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request."
                    });
                }

                var response = await _studentFeeRepository.GetStudentPartPaidMonths(request);
                return Ok(new ApiResponse<IEnumerable<StudentPartPaidMonthLine>>
                {
                    Success = true,
                    Data = response
                });
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<string>.Fail("Unable to load the partly paid months."));
            }
        }

        [HttpPost("AdjustStudentFeeCollection")]
        public async Task<IActionResult> AdjustStudentFeeHeadWiseData([FromBody] AdjustStudentFeeHeadWiseRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request."
                    });
                }

                // paying more than the due makes a negative balance on the receipt (due 20000, paid 40000 -> -20000)
                var overPayMessage = ValidateFeeCollectionAmounts(request);
                if (overPayMessage != null)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = overPayMessage
                    });
                }

                var result = await _studentFeeRepository.AdjustStudentFeeHeadWise(request);

                if (result == null)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Fee adjustment failed."
                    });
                }

                if (string.IsNullOrWhiteSpace(result.StudentReceiptNo))
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Receipt could not be generated."
                    });
                }

                return Ok(new ApiResponse<AdjustStudentFeeHeadWiseResponse>
                {
                    Success = true,
                    Message = "Fee adjusted successfully.",
                    Data = result
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

        // null = amounts are fine; otherwise the message for the user
        private static string? ValidateFeeCollectionAmounts(AdjustStudentFeeHeadWiseRequest request)
        {
            var details = request.FeeHeadDetails;
            if (details == null || details.Count == 0)
                return "No fee dues selected to collect.";

            if (request.PaidAmount < 0 || details.Any(d => d.PaidAmount < 0))
                return "Paying amount cannot be negative.";

            // each fee head row: paid now cannot be more than what is still due on it
            if (details.Any(d => d.PaidAmount > d.AdjustAmount))
                return "Paying amount cannot be more than the due amount.";

            // cash over what the fee heads take is allowed only as advance: the request must carry it as a
            // negative balance of at least that much (due 20000, paid 40000 -> Balance -20000)
            var extra = request.PaidAmount - details.Sum(d => d.PaidAmount);
            if (extra > 0 && request.Balance > -extra)
                return "Extra amount over the due must be kept as advance.";

            return null;
        }

        [HttpPost("GetStudentLedgerChallanMiniDetails")]
        public async Task<IActionResult> GetStudentLedgerChallanMiniDetails([FromBody] StudentLedgerChallanMiniDetailsRequest request)
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
                var data = await _studentFeeRepository.GetStudentLedgerChallanMiniDetails(request);
                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<StudentLedgerChallanMiniDetailsResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<StudentLedgerChallanMiniDetailsResponse>()
                    });
                }
                return Ok(new ApiResponse<IEnumerable<StudentLedgerChallanMiniDetailsResponse>>
                {
                    Success = true,
                    Message = "Student details retrieved successfully.",
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
                    Message = "An error occurred while fetching the student details.",
                    Code = -1
                });
            }
        }
        [HttpPost("GetStudentChallan")]
        public async Task<IActionResult> GetStudentChallanDetails([FromBody] StudentChallanRequest request)
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
                var data = await _studentFeeRepository.GetStudentChallanDetails(request);

                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<StudentChallanResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<StudentChallanResponse>()
                    });
                }

                return Ok(new ApiResponse<IEnumerable<StudentChallanResponse>>
                {
                    Success = true,
                    Message = "Student challan retrieved successfully.",
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
                    Message = "An error occurred while fetching the student challan.",
                    Code = -1
                });
            }
        }
        [HttpPost("GetStudentReceipts")]
        public async Task<IActionResult> GetStudentReceipts([FromBody] StudentReceiptsRequest request)
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
                var data = await _studentFeeRepository.GetStudentReceipts(request);
                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<StudentReceiptsResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<StudentReceiptsResponse>()
                    });
                }
                return Ok(new ApiResponse<IEnumerable<StudentReceiptsResponse>>
                {
                    Success = true,
                    Message = "Student receipts retrieved successfully.",
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
                    Message = "An error occurred while fetching the student receipts.",
                    Code = -1
                });
            }
        }
        [HttpPost("GetStudentListForBalanceAdjustmentData")]
        public async Task<IActionResult> GetStudentListForBalanceAdjustment([FromBody] StudentListForBalanceAdjustmentRequest request)
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
                var data = await _studentFeeRepository.GetStudentListForBalanceAdjustment(request);

                if (data == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid TransType. Use Advance, Adjustment or Waive Off.",
                        Code = 0
                    });
                }

                if (!data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<StudentListForBalanceAdjustmentResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<StudentListForBalanceAdjustmentResponse>()
                    });
                }

                return Ok(new ApiResponse<IEnumerable<StudentListForBalanceAdjustmentResponse>>
                {
                    Success = true,
                    Message = "Student list retrieved successfully.",
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
                    Message = "An error occurred while fetching the student list.",
                    Code = -1
                });
            }
        }

        // Adjust advance -> Apply. TransType ADVANCE / ADJUSTMENT: advance is adjusted against the selected
        // dues by V3M_FIN_UspAdjustAdvanceAmount; WAIVE OFF: the waive off amount by V3M_FIN_UspAdjustWaiveOffAmount.
        [HttpPost("AdjustAdvanceAmount")]
        public async Task<IActionResult> AdjustAdvanceAmount([FromBody] AdjustAdvanceAmountRequest request)
        {
            if (request == null)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Invalid request.", Code = 0 });

            if (string.IsNullOrWhiteSpace(request.GroupCode) || string.IsNullOrWhiteSpace(request.BranchCode) ||
                request.SessionId <= 0 || request.StudentId <= 0)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Student details are missing.", Code = 0 });

            var detInvoiceIds = (request.DetInvoiceIds ?? new()).Where(x => x > 0).Distinct().ToList();
            if (detInvoiceIds.Count == 0)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Please select the dues to adjust.", Code = 0 });

            if (request.AdvAmount <= 0)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Adjustment amount must be more than zero.", Code = 0 });

            var transType = (request.TransType ?? string.Empty).Trim().ToUpperInvariant();
            var isAdvance = transType == "ADVANCE" || transType == "ADJUSTMENT";
            var isWaiveOff = transType == "WAIVE OFF" || transType == "WAIVEOFF";
            if (!isAdvance && !isWaiveOff)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Invalid TransType. Use Advance, Adjustment or Waive Off.", Code = 0 });

            var label = isAdvance ? "advance" : "waive off";
            try
            {
                request.DetInvoiceIds = detInvoiceIds;
                AdjustAdvanceAmountResponse result;
                if (isAdvance)
                {
                    // ADVANCE / ADJUSTMENT -> V3M_FIN_UspAdjustAdvanceAmount
                    result = await _studentFeeRepository.AdjustAdvanceAmount(request);
                }
                else
                {
                    // WAIVE OFF -> V3M_FIN_UspAdjustWaiveOffAmount
                    result = await _studentFeeRepository.AdjustWaiveOffAmount(request);
                }

                return Ok(new ApiResponse<AdjustAdvanceAmountResponse>
                {
                    Success = true,
                    Message = isAdvance ? "Advance adjusted successfully." : "Waive off adjusted successfully.",
                    Code = 1,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"An error occurred while adjusting the {label}.",
                    Code = -1
                });
            }
        }
    }
}
