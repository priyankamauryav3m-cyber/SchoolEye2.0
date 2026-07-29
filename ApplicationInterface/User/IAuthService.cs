using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.User
{
    public interface IAuthService
    {
        Task SignInAsync(HttpContext context, string username, string role);
        Task SignOutAsync(HttpContext context);
    }
}
