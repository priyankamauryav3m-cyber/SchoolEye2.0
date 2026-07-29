using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ServerWebAPI.Login.Controllers.Login
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthGoogleFbController : ControllerBase
    {
        [HttpGet("google")]
        public IActionResult GoogleLogin()
        {
            try
            {
                var redirectUrl = Url.Action("GoogleResponse", "AuthGoogleFb", null, Request.Scheme);

                var properties = new AuthenticationProperties
                {
                    RedirectUri = redirectUrl
                };

                return Challenge(properties, GoogleDefaults.AuthenticationScheme);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("facebook")]
        public IActionResult FacebookLogin()
        {
            try
            {
                var redirectUrl = Url.Action("FacebookResponse", "AuthGoogleFb", null, Request.Scheme);

                var properties = new AuthenticationProperties
                {
                    RedirectUri = redirectUrl
                };

                return Challenge(properties, FacebookDefaults.AuthenticationScheme);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            try
            {
                var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

                if (result?.Principal == null)
                    return BadRequest("Authentication failed");

                var email = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                var name = result.Principal.Identity?.Name;

                return Redirect($"http://localhost:5219/googel-facebook-login?email={email}&name={name}");
               
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("facebook-response")]
        public async Task<IActionResult> FacebookResponse()
        {
            try
            {
                var result = await HttpContext.AuthenticateAsync();

                var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
                var name = result.Principal.Identity?.Name;

                return Redirect($"https://localhost:5001/external-login?email={email}&name={name}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
