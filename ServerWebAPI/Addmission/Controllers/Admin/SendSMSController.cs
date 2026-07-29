using ApplicationInterface.SuperAdmin;
using DomainModel.Admin;
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





    }
}
