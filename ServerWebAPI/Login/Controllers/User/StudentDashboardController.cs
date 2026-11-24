using ApplicationInterface.User;
using DomainModel.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using System.Security.Claims;

namespace ServerWebAPI.Login.Controllers.User
{
    [ApiExplorerSettings(GroupName = "Login")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentDashboardController : ControllerBase
    {
        private readonly IUser _users;
        public StudentDashboardController(IUser users)
        {
            _users = users;
        }

        // Dashboard of the logged-in student. The UI sends the UserId saved in session at login;
        // it must match the user in the verified JWT, so a student can never read another
        // student's record by changing the UserId in the request.
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] StudentDashboardRequest request)
        {
            if (request == null || request.UserId <= 0)
                return BadRequest(ApiResponse<string>.Fail("A valid user id is required."));

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var tokenUserId))
                return Unauthorized(ApiResponse<string>.Fail("Unable to identify the current user."));

            if (tokenUserId != request.UserId)
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail("You are not allowed to view this record."));

            try
            {
                var profile = await _users.GetStudentDashboardProfileAsync(request.UserId);
                if (profile == null)
                    return NotFound(ApiResponse<string>.Fail("Student record was not found."));

                return Ok(ApiResponse<StudentDashboardProfile>.Ok(profile));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail("Something went wrong."));
            }
        }
    }
}
