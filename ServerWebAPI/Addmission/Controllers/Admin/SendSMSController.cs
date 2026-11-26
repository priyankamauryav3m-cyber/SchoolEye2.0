using ApplicationInterface.SuperAdmin;
using DomainModel.Admin;
using DomainModel.SchoolMaster;
using DomainModel.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace ServerWebAPI.Addmission.Controllers.Admin
{
    [ApiExplorerSettings(GroupName = "Admission")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SendSMSController : ControllerBase
    {
        private readonly ISendSMS _service;

        public SendSMSController(ISendSMS service)
        {
            _service = service;
        }
        [HttpPost("send-sms-log")]
        public async Task<IActionResult> SendSMSLog(SMSSentModel model)
        {
            try
            {
                var result = await _service.SaveSMSSentDetails(model);

                if (result != null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = true,
                        Message = "SMS saved successfully",
                        Data = result
                    });
                }
                return Ok(new ApiResponse<string>
                {
                    Success = false,
                    Message = result
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Something went wrong."
                });
            }
        }

        // WhatsApp (Pinnacle): message to the father's WhatsApp number of a registration.
        // Success = true only when Pinnacle accepted the message (provider message id returned).
        [HttpPost("/api/WhatsApp/SendRegistrationMessage")]
        public async Task<IActionResult> SendRegistrationMessage([FromBody] WhatsAppRegistrationMessageRequest request, CancellationToken cancellationToken)
        {
            if (request == null || request.RegistrationId <= 0
                || string.IsNullOrWhiteSpace(request.GroupCode) || string.IsNullOrWhiteSpace(request.BranchCode))
            {
                return BadRequest(ApiResponse<WhatsAppSendResult>.Fail("RegistrationId, GroupCode and BranchCode are required."));
            }
            // a template (e.g. fee_reminder5) or a free-text message
            if (string.IsNullOrWhiteSpace(request.TemplateName) && string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(ApiResponse<WhatsAppSendResult>.Fail("Give TemplateName (approved template) or Message."));
            }
            if (request.TemplateParameters != null && request.TemplateParameters.Any(p => string.IsNullOrWhiteSpace(p)))
            {
                return BadRequest(ApiResponse<WhatsAppSendResult>.Fail("Template parameters cannot be empty."));
            }

            try
            {
                var user = HttpContext.Items["User"] as UserModels;
                var sentBy = !string.IsNullOrWhiteSpace(user?.LoginName) ? user.LoginName : user?.Username;

                var result = await _service.SendRegistrationWhatsAppAsync(request, sentBy, cancellationToken);

                if (result.Success)
                    return Ok(ApiResponse<WhatsAppSendResult>.Ok(result, result.Message, code: 1));

                return Ok(new ApiResponse<WhatsAppSendResult>
                {
                    Success = false,
                    Message = result.Message,
                    Data = result,
                    Status = result.Failure switch
                    {
                        WhatsAppFailure.RegistrationNotFound => StatusCodes.Status404NotFound,
                        WhatsAppFailure.MobileNotAvailable or WhatsAppFailure.InvalidMobile => StatusCodes.Status422UnprocessableEntity,
                        WhatsAppFailure.SettingsMissing or WhatsAppFailure.InvalidCredentials => StatusCodes.Status503ServiceUnavailable,
                        WhatsAppFailure.Timeout => StatusCodes.Status504GatewayTimeout,
                        _ => StatusCodes.Status502BadGateway
                    }
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<WhatsAppSendResult>.Fail("Unable to send the WhatsApp message right now. Please try again."));
            }
        }

        [HttpPost("/api/SMS/SendRegistrationMessage")]
        public async Task<IActionResult> SendRegistrationSms([FromBody] WhatsAppRegistrationMessageRequest request, CancellationToken cancellationToken)
        {
            if (request == null || request.RegistrationId <= 0 || string.IsNullOrWhiteSpace(request.GroupCode) || string.IsNullOrWhiteSpace(request.BranchCode))
            {
                return BadRequest(ApiResponse<WhatsAppSendResult>.Fail("RegistrationId, GroupCode and BranchCode are required."));
            }
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(ApiResponse<WhatsAppSendResult>.Fail("Message is required."));
            }

            try
            {
                var user = HttpContext.Items["User"] as UserModels;
                var sentBy = !string.IsNullOrWhiteSpace(user?.LoginName) ? user.LoginName : user?.Username;

                var result = await _service.SendRegistrationSmsAsync(request, sentBy, cancellationToken);

                if (result.Success)
                    return Ok(ApiResponse<WhatsAppSendResult>.Ok(result, result.Message, code: 1));

                return Ok(new ApiResponse<WhatsAppSendResult>
                {
                    Success = false,
                    Message = result.Message,
                    Data = result,
                    Status = result.Failure switch
                    {
                        WhatsAppFailure.RegistrationNotFound => StatusCodes.Status404NotFound,
                        WhatsAppFailure.MobileNotAvailable or WhatsAppFailure.InvalidMobile => StatusCodes.Status422UnprocessableEntity,
                        WhatsAppFailure.SettingsMissing or WhatsAppFailure.InvalidCredentials => StatusCodes.Status503ServiceUnavailable,
                        WhatsAppFailure.Timeout => StatusCodes.Status504GatewayTimeout,
                        WhatsAppFailure.TemplateMismatch => StatusCodes.Status422UnprocessableEntity,
                        _ => StatusCodes.Status502BadGateway
                    }
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<WhatsAppSendResult>.Fail("Unable to send the SMS right now. Please try again."));
            }
        }

        [HttpPost("GetMessageHistory")] 
        public async Task<IActionResult> GetMessageHistory([FromBody] MessageHistoryRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.GroupCode) || string.IsNullOrWhiteSpace(request.BranchCode)
                || request.RecipientIds == null || !request.RecipientIds.Any(id => id > 0))
            {
                return BadRequest(ApiResponse<List<MessageHistoryRow>>.Fail("GroupCode, BranchCode and RecipientIds are required."));
            }

            try
            {
                var rows = await _service.GetMessageHistoryAsync(request);
                return Ok(ApiResponse<List<MessageHistoryRow>>.Ok(rows, rows.Count == 0 ? "No message sent yet." : null, code: 1));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<List<MessageHistoryRow>>.Fail("Unable to load the message history right now. Please try again."));
            }
        }

        // SMS balance of the Pinnacle account (credentials from appsettings SmsSettings)
        [HttpGet("GetSmsBalance")]
        public async Task<IActionResult> GetSmsBalance(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _service.GetSmsBalanceAsync(cancellationToken);
                // not available: HTTP 200 + Success=false so the UI can show the reason
                return Ok(new ApiResponse<SmsBalanceResult>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result,
                    Code = result.Success ? 1 : 0
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<SmsBalanceResult>.Fail("Unable to load the SMS balance right now. Please try again."));
            }
        }
    }
}
