using ApplicationInterface.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MyApp.Common;
using System.Security.Claims;

namespace ServerWebAPI.Authorization;

// Allows the action only for users whose role is V3MAdmin.
// The role is read from the database for the user in the verified JWT, so it
// cannot be faked from the browser (session storage, URL, request body).
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class V3MAdminOnlyAttribute : Attribute, IAsyncAuthorizationFilter
{
    public const string RoleName = "V3MAdmin";

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var userSid = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userSid))
        {
            context.Result = new UnauthorizedObjectResult(ApiResponse<string>.Fail("Unable to identify the current user."));
            return;
        }

        var users = context.HttpContext.RequestServices.GetRequiredService<IUser>();
        bool allowed;
        try
        {
            allowed = await users.IsUserInRoleAsync(userSid, RoleName);
        }
        catch (Exception)
        {
            context.Result = new ObjectResult(ApiResponse<string>.Fail("Something went wrong.")) { StatusCode = StatusCodes.Status500InternalServerError };
            return;
        }

        if (!allowed)
        {
            context.Result = new ObjectResult(ApiResponse<string>.Fail("Access denied. Only V3M Admin can use this feature."))
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
