using ApplicationInterface.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.User
{
    public class AuthService : IAuthService
    {
        public async Task SignInAsync(HttpContext context, string username, string role)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };


            var identity = new ClaimsIdentity(claims, "LoginV3M");
            var principal = new ClaimsPrincipal(identity);

            await context.SignInAsync("LoginV3M", principal);
        }

        public async Task SignOutAsync(HttpContext context)
        {
            await context.SignOutAsync("LoginV3M");
        }
    }
}
