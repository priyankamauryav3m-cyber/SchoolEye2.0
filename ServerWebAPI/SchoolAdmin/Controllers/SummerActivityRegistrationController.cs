using ApplicationInterface.SchoolAdmin;
using DomainModel.SchoolAdmin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MyApp.Common;

namespace ServerWebAPI.SchoolAdmin.Controllers
{
    // public registration page of a summer activity (opened from the tiny link, no login).
    // The link key is the only input that selects the school / activity; it is checked on every call.
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "Admission")]
    [AllowAnonymous]
    public class SummerActivityRegistrationController : ControllerBase
    {
        private readonly ISummerActivityRegistration _service;
        private readonly ILogger<SummerActivityRegistrationController> _logger;

        public SummerActivityRegistrationController(ISummerActivityRegistration service, ILogger<SummerActivityRegistrationController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private static string Message(int code) => code switch
        {
            -6 => "This link is not valid. Please check the link sent by the school.",
            -1 => "This activity is not available.",
            -2 => "Registration for this activity is closed.",
            -3 => "The selected slot is not available. Please select another slot.",
            -4 => "Please fill all the required fields.",
            -5 => "Please correct the highlighted fields.",
            _ => "Registration could not be saved. Please try again."
        };

        // school header, activity, slots and form fields of the link
        [HttpPost("GetForm")]
        public async Task<IActionResult> GetForm([FromBody] SummerActivityFormRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Key))
                return BadRequest(ApiResponse<SummerActivityFormInfo>.Fail(Message(-6)));

            try
            {
                var (code, form) = await _service.GetFormAsync(request.Key);
                if (code == 1 && form != null)
                    return Ok(ApiResponse<SummerActivityFormInfo>.Ok(form, "Form loaded.", code: 1));

                return Ok(new ApiResponse<SummerActivityFormInfo>
                {
                    Success = false,
                    Code = code,
                    Status = code == -6 ? 400 : code == -2 ? 410 : 404,
                    Message = Message(code)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SummerActivityRegistration GetForm failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<SummerActivityFormInfo>.Fail("Unable to load the form right now. Please try again."));
            }
        }

        // payment review of a saved registration (key = PaymentKey of Register)
        [HttpPost("GetPayment")]
        public async Task<IActionResult> GetPayment([FromBody] SummerActivityFormRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Key))
                return BadRequest(ApiResponse<SummerActivityPaymentInfo>.Fail("This payment link is not valid."));

            try
            {
                var info = await _service.GetPaymentAsync(request.Key);
                if (info == null)
                    return Ok(new ApiResponse<SummerActivityPaymentInfo> { Success = false, Code = -1, Status = 404, Message = "Registration was not found for this payment link." });

                return Ok(ApiResponse<SummerActivityPaymentInfo>.Ok(info, "Payment details loaded.", code: 1));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SummerActivityRegistration GetPayment failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<SummerActivityPaymentInfo>.Fail("Unable to load the payment details right now. Please try again."));
            }
        }

        // checkout: makes (or reuses) the pending order of the registration; Code 2 = already paid (receipt)
        [EnableRateLimiting("V3MAPI_Call_Limit")]
        [HttpPost("InitiatePayment")]
        public async Task<IActionResult> InitiatePayment([FromBody] SummerActivityFormRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Key))
                return BadRequest(ApiResponse<SummerActivityCheckoutInfo>.Fail("This payment link is not valid."));

            try
            {
                var (code, info) = await _service.InitiatePaymentAsync(request.Key);
                if ((code == 1 || code == 2) && info != null)
                    return Ok(ApiResponse<SummerActivityCheckoutInfo>.Ok(info, code == 2 ? "Fee is already paid." : "Order created.", code: code));

                return Ok(new ApiResponse<SummerActivityCheckoutInfo>
                {
                    Success = false,
                    Code = code,
                    Status = code == -1 ? 404 : code == -3 ? 422 : 500,
                    Message = code switch
                    {
                        -1 => "Registration was not found for this payment link.",
                        -3 => "There is no fee to pay for this registration.",
                        _ => "Unable to start the payment right now. Please try again."
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SummerActivityRegistration InitiatePayment failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<SummerActivityCheckoutInfo>.Fail("Unable to start the payment right now. Please try again."));
            }
        }

        // local TEST payment (no money is charged): works only when SummerActivity:TestPayment is true.
        // The real gateway will confirm the payment server-side instead of this call.
        [EnableRateLimiting("V3MAPI_Call_Limit")]
        [HttpPost("PayTest")]
        public async Task<IActionResult> PayTest([FromBody] SummerActivityPayRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse<SummerActivityPaymentReceipt>.Fail("This payment link is not valid."));

            try
            {
                var (code, receipt) = await _service.CompleteTestPaymentAsync(request);
                if (code == 1 && receipt != null)
                    return Ok(ApiResponse<SummerActivityPaymentReceipt>.Ok(receipt, "Payment successful.", code: 1));

                return Ok(new ApiResponse<SummerActivityPaymentReceipt>
                {
                    Success = false,
                    Code = code,
                    Status = code switch { -1 => 404, -2 => 409, -3 => 410, -4 => 403, _ => 500 },
                    Message = code switch
                    {
                        -1 => "Order was not found. Please start the payment again.",
                        -2 => "Fee is already paid for this registration.",
                        -3 => "This payment session has expired. Please start the payment again.",
                        -4 => "Online payment is not available yet. Please contact the school.",
                        _ => "Payment could not be completed. Please try again."
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SummerActivityRegistration PayTest failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<SummerActivityPaymentReceipt>.Fail("Payment could not be completed. Please try again."));
            }
        }

        // saving is throttled (same limit as the online registration form)
        [EnableRateLimiting("V3MAPI_Call_Limit")]
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] SummerActivityRegistrationRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Key))
                return BadRequest(ApiResponse<SummerActivityRegistrationResult>.Fail(Message(-6)));

            try
            {
                var result = await _service.RegisterAsync(request);
                return Ok(new ApiResponse<SummerActivityRegistrationResult>
                {
                    Success = result.Code == 1,
                    Code = result.Code,
                    Status = result.Code == 1 ? 200 : result.Code is -4 or -5 or -3 ? 422 : result.Code == -6 ? 400 : result.Code == -2 ? 410 : result.Code == -1 ? 404 : 500,
                    Message = result.Code == 1 ? "Registration submitted successfully." : Message(result.Code),
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SummerActivityRegistration Register failed");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<SummerActivityRegistrationResult>.Fail("Registration could not be saved. Please try again."));
            }
        }
    }
}
