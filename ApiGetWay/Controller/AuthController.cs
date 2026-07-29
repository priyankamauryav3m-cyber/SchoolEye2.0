using ApiGetWay.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiGetWay.Controller
{
    [Route("Users")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AuthenticateUser> _userManager;
        private readonly SignInManager<AuthenticateUser> _signInManager;
        private readonly JwtTokenIssuer _jwt;

        public AuthController(UserManager<AuthenticateUser> userManager, JwtTokenIssuer jwt, SignInManager<AuthenticateUser> signInManager)
        {
            _userManager = userManager;
            _jwt = jwt;
            _signInManager = signInManager;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName)
                ?? await _userManager.FindByEmailAsync(dto.UserName);

            if (user == null)
                return Unauthorized("Invalid credentials");

            if (!await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized("Invalid credentials");

            // 🔑 Identity cookie created here
            await _signInManager.SignInAsync(user, isPersistent: true);

            var roles = await _userManager.GetRolesAsync(user);

            var accessToken = await _jwt.CreateAccessTokenAsync(user);

            return Ok(new
            {
                success = true,
                userName = user.UserName,
                role = roles.FirstOrDefault(),
                accessToken
            });
        }

        //[HttpPost("login")]
        //public async Task<IActionResult> Login(LoginDto dto)
        //{
        //    var user = await _userManager.FindByNameAsync(dto.UserName)
        //               ?? await _userManager.FindByEmailAsync(dto.UserName);

        //    if (user == null)
        //        return Unauthorized("Invalid credentials");

        //    if (!await _userManager.CheckPasswordAsync(user, dto.Password))
        //        return Unauthorized("Invalid credentials");

        //    var accessToken = await _jwt.CreateAccessTokenAsync(user);

        //    var roles = await _userManager.GetRolesAsync(user);
        //    var claims = new List<Claim>
        //    {
        //        new Claim(ClaimTypes.NameIdentifier, user.Id),
        //        new Claim(ClaimTypes.Name, user.UserName!),
        //        new Claim("access_token", accessToken)
        //    };

        //    foreach (var role in roles)
        //        claims.Add(new Claim(ClaimTypes.Role, role));
        //    var identity = new ClaimsIdentity(claims, "Cookies");
        //    var principal = new ClaimsPrincipal(identity);
        //    await _signInManager.SignInAsync(user, isPersistent: true);
        //    //await HttpContext.SignInAsync("Cookies", principal);

        //    return Ok(new
        //    {
        //        accessToken,
        //        success = true,
        //        userName = user.UserName,
        //        role = roles.FirstOrDefault()
        //    });
        //}

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            Response.Cookies.Delete("Auth.Cookie", new CookieOptions
            {
                Path = "/",
                Secure = true,
                SameSite = SameSiteMode.None
            });

            return Ok(new { success = true });
        }
        public class LoginDto
        {
            public string UserName { get; set; } = "";
            public string Password { get; set; } = "";
        }
    }
}
