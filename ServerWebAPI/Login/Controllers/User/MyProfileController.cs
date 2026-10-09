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
    public class MyProfileController : ControllerBase
    {
        private readonly IUser _users;
        public MyProfileController(IUser users)
        {
            _users = users;
        }

        // Profile of the logged-in user only: the id comes from the verified JWT,
        // so one user can never read another user's profile.
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userSid))
                return Unauthorized(ApiResponse<string>.Fail("Unable to identify the current user."));

            try
            {
                var profile = await _users.GetMyProfileAsync(userSid);
                if (profile == null)
                    return NotFound(ApiResponse<string>.Fail("Profile was not found."));

                return Ok(ApiResponse<MyProfileResponse>.Ok(profile));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<string>.Fail("Something went wrong."));
            }
        }
    }
}
