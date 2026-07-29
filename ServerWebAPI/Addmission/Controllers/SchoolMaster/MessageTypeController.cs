using ApplicationInterface.SchoolMaster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ServerWebAPI.Addmission.Controllers.SchoolMaster
{
    [ApiExplorerSettings(GroupName = "Admission")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MessageTypeController : ControllerBase
    {
        private readonly IMessageTypeRepository _service;

        public MessageTypeController(IMessageTypeRepository service)
        {
            _service = service;
        }

        [HttpGet("GetMessageType")]
        public async Task<IActionResult> GetMessage()
        {
            try
            {
                var data = await _service.GetMessageType();
                return Ok(data);
            }
            catch (Exception)
            {
                return StatusCode(500, "Something went wrong.");
            }
        }

        [HttpGet("GetSMSEmailText")]
        public async Task<IActionResult> GetEmailTextMessage([FromQuery] int messageTypeId)
        {
            try
            {
                if (messageTypeId <= 0)
                {
                    return BadRequest(new { message = "MessageTypeId is required." });
                }
                var data = await _service.GetMessageType(messageTypeId);

                if (data == null || !data.Any())
                {
                    return NotFound(new { message = "No records found." });
                }
                return Ok(data);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Something went wrong." });
            }
        }
    }
}
