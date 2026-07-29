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
    public class ApplicationStatusController : ControllerBase
    {
        private readonly IApplicationStatus _service;

        public ApplicationStatusController(IApplicationStatus service)
        {
            _service = service;
        }

        [HttpGet("registration-status")]
        public async Task<IActionResult> GetRegistrationStatus(
         [FromQuery] string groupCode,
         [FromQuery] string branchCode,
         [FromQuery] string sessionName,
         [FromQuery] string registrationNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(registrationNo))
                    return BadRequest(new { message = "Registration status required" });

                var data = await _service.GetRegistrationStatus(
                    groupCode, branchCode, sessionName, registrationNo);

                if (data == null)
                    return NotFound(new { message = "Record not found" });

                return Ok(data);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Something went wrong." });
            }
        }
    }
}
