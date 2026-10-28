using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainModel.FinanceMNGT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using SchoolEye.ApplicationInterface.Admin;


namespace SchoolEye.ServerWebAPI.Addmission.Controllers.Admin
{
    [ApiExplorerSettings(GroupName = "FinanceManagement")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentMiscellaneousController : ControllerBase
    {
        private readonly IStudentMiscellaneousRepository _service;

        public StudentMiscellaneousController(IStudentMiscellaneousRepository service)
        {
            _service = service;
        }

        [HttpPost("GetStudentMiscellaneousData")]
        public async Task<IActionResult> GetStudentMiscellaneous([FromBody] MiscellaneousRequest request)
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
                var data = await _service.GetStudentMiscellaneous(request);
                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<MiscellaneousResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<MiscellaneousResponse>()
                    });
                }
                return Ok(new ApiResponse<IEnumerable<MiscellaneousResponse>>
                {
                    Success = true,
                    Message = "Students retrieved successfully.",
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
                    Message = "An error occurred while fetching the students.",
                    Code = -1
                });
            }
        }

        [HttpPost("GetStudentByFeeTemplate")]
        public async Task<IActionResult> GetStudentByFeeTemplate([FromBody] StudentByFeeTemplateRequest request)
        {
            if (request == null || request.SessionId <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Please select session.",
                    Code = 0
                });
            }
            if (request.FeeTemplateId <= 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Please select template.",
                    Code = 0
                });
            }
            try
            {
                var data = await _service.GetStudentByFeeTemplate(request);
                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<StudentByFeeTemplateResponse>>
                    {
                        Success = true,
                        Message = "No records found.",
                        Code = 0,
                        Data = Enumerable.Empty<StudentByFeeTemplateResponse>()
                    });
                }
                return Ok(new ApiResponse<IEnumerable<StudentByFeeTemplateResponse>>
                {
                    Success = true,
                    Message = "Students retrieved successfully.",
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
                    Message = "An error occurred while fetching the students.",
                    Code = -1
                });
            }
        }

        [HttpPost("GenerateMiscellaneousChallan")]
        public async Task<IActionResult> GenerateMiscellaneousChallan([FromBody] GenerateMiscChallanRequest request)
        {
            if (request == null || request.Students == null || request.Students.Count == 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Please select at least one student.",
                    Code = 0
                });
            }
            if (request.FeeHeads == null || request.FeeHeads.Count == 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Please add at least one fee head.",
                    Code = 0
                });
            }
            if (string.IsNullOrWhiteSpace(request.Narration))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Narration is required.",
                    Code = 0
                });
            }
            if (request.FeeHeads.Any(x => x.Amount <= 0))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Fee head amount must be greater than 0.",
                    Code = 0
                });
            }
            try
            {
                var data = await _service.GenerateMiscellaneousChallan(request);
                if (data == null || !data.Any())
                {
                    return Ok(new ApiResponse<IEnumerable<GenerateMiscChallanResponse>>
                    {
                        Success = false,
                        Message = "Challan not generated.",
                        Code = 0,
                        Data = Enumerable.Empty<GenerateMiscChallanResponse>()
                    });
                }
                return Ok(new ApiResponse<IEnumerable<GenerateMiscChallanResponse>>
                {
                    Success = true,
                    Message = "Challan generated successfully.",
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
                    Message = "An error occurred while generating the challan.",
                    Code = -1
                });
            }
        }

        [HttpPost("GenerateMiscChallanWithMonths")]
        public async Task<IActionResult> GenerateMiscChallanWithMonths([FromBody] MiscChallanWithMonthsRequest request)
        {
            if (request == null || request.SessionId <= 0)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Please select session.", Code = 0 });
            }
            if (request.FeeTemplateId <= 0)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Please select template.", Code = 0 });
            }
            if (request.StudentIds == null || !request.StudentIds.Any(x => x > 0))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Please select at least one student.", Code = 0 });
            }
            if (request.FeeHeads == null || request.FeeHeads.Count == 0)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Please select fee head and month.", Code = 0 });
            }
            if (request.FeeHeads.Any(x => x.FeeHeadId <= 0 || x.MonthNo < 1 || x.MonthNo > 12))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Invalid fee head or month.", Code = 0 });
            }
            if (request.FeeHeads.Any(x => x.Amount <= 0))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Fee head amount must be greater than 0.", Code = 0 });
            }
            if (string.IsNullOrWhiteSpace(request.Narration))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Narration is required.", Code = 0 });
            }
            if (request.Narration.Trim().Length > 150)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Narration can be maximum 150 characters.", Code = 0 });
            }
            if (request.DueDate == default)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Please select due date.", Code = 0 });
            }
            try
            {
                request.StudentIds = request.StudentIds.Where(x => x > 0).Distinct().ToList();
                request.Narration = request.Narration.Trim();

                var result = await _service.GenerateMiscChallanWithMonths(request);
                if (result != 1)
                {
                    return Ok(new ApiResponse<object> { Success = false, Message = "Due not generated.", Code = 0 });
                }
                return Ok(new ApiResponse<object> { Success = true, Message = "Due generated successfully.", Code = 1 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while generating the due.",
                    Code = -1
                });
            }
        }

        [HttpPost("GenerateMiscellaneousReceipt")]
        public async Task<IActionResult> GenerateMiscellaneousReceipt([FromBody] GenerateMiscReceiptRequest request)
        {
            if (request == null || request.SessionId <= 0)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Please select session.", Code = 0 });
            }
            if (request.StudentId <= 0)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Please select student.", Code = 0 });
            }
            if (request.ReceiptDate == default)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Please select receipt date.", Code = 0 });
            }
            if (request.FeeHeads == null || request.FeeHeads.Count == 0)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Please add at least one fee head.", Code = 0 });
            }
            if (request.FeeHeads.Any(x => x.FeeHeadId <= 0))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Invalid fee head.", Code = 0 });
            }
            if (request.FeeHeads.GroupBy(x => x.FeeHeadId).Any(g => g.Count() > 1))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Fee head cannot be added more than once.", Code = 0 });
            }
            if (request.FeeHeads.Any(x => x.Amount <= 0 || x.Concession < 0 || x.Concession > x.Amount))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Fee head amount must be greater than 0.", Code = 0 });
            }
            var payable = request.FeeHeads.Sum(x => x.Amount - x.Concession);
            if (request.PaidAmount <= 0 || request.PaidAmount != payable)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Paid amount does not match the fee head total.", Code = 0 });
            }
            var paymentModes = new[] { "Cash", "Cheque", "Online" };
            if (string.IsNullOrWhiteSpace(request.PaymentMode) || !paymentModes.Contains(request.PaymentMode))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Please select payment mode.", Code = 0 });
            }
            if (request.PaymentMode != "Cash")
            {
                var noLabel = request.PaymentMode == "Online" ? "transaction no." : "cheque no.";
                var dateLabel = request.PaymentMode == "Online" ? "transaction date" : "cheque date";
                if (string.IsNullOrWhiteSpace(request.ChequeNo) || request.ChequeNo.Trim().Length > 50)
                {
                    return BadRequest(new ApiResponse<object> { Success = false, Message = $"Please enter valid {noLabel}", Code = 0 });
                }
                if (request.ChequeDate == null)
                {
                    return BadRequest(new ApiResponse<object> { Success = false, Message = $"Please select {dateLabel}.", Code = 0 });
                }
                if (string.IsNullOrWhiteSpace(request.ChequeBank) || request.ChequeBank.Trim().Length > 100)
                {
                    return BadRequest(new ApiResponse<object> { Success = false, Message = "Please enter valid bank.", Code = 0 });
                }
            }
            if (!string.IsNullOrWhiteSpace(request.Remark) && request.Remark.Trim().Length > 200)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Remark can be maximum 200 characters.", Code = 0 });
            }
            try
            {
                // cash receipts never carry cheque / transaction details
                if (request.PaymentMode == "Cash")
                {
                    request.ChequeNo = string.Empty;
                    request.ChequeDate = null;
                    request.ChequeBank = string.Empty;
                }
                request.ChequeNo = request.ChequeNo?.Trim();
                request.ChequeBank = request.ChequeBank?.Trim();
                request.Remark = request.Remark?.Trim();

                var result = await _service.GenerateMiscellaneousReceipt(request);
                if (result == null || string.IsNullOrWhiteSpace(result.StudentReceiptNo))
                {
                    return Ok(new ApiResponse<object> { Success = false, Message = "Receipt could not be generated.", Code = 0 });
                }
                return Ok(new ApiResponse<GenerateMiscReceiptResponse>
                {
                    Success = true,
                    Message = "Fee collected successfully.",
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
                    Message = "An error occurred while generating the receipt.",
                    Code = -1
                });
            }
        }

        //[HttpPost("GetFeeHeadForCombo")]
        //public async Task<IActionResult> GetFeeHeadForCombo([FromBody] FeeHeadForComboRequest request)
        //{
        //    if (request == null)
        //    {
        //        return BadRequest(new ApiResponse<object>
        //        {
        //            Success = false,
        //            Message = "Data not found.",
        //            Code = 0
        //        });
        //    }

        //    try
        //    {
        //        var data = await _service.GetFeeHeadForCombo(request);

        //        if (data == null || !data.Any())
        //        {
        //            return Ok(new ApiResponse<IEnumerable<FeeHeadForComboResponse>>
        //            {
        //                Success = true,
        //                Message = "No records found.",
        //                Code = 0,
        //                Data = Enumerable.Empty<FeeHeadForComboResponse>()
        //            });
        //        }

        //        return Ok(new ApiResponse<IEnumerable<FeeHeadForComboResponse>>
        //        {
        //            Success = true,
        //            Message = "Fee heads retrieved successfully.",
        //            Code = 1,
        //            Data = data
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Exception: {ex.Message}");
        //        return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
        //        {
        //            Success = false,
        //            Message = "An error occurred while fetching the fee heads.",
        //            Code = -1
        //        });
        //    }
        //}
    }
}
