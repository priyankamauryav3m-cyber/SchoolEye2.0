using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ServerWebAPI.Authorization
{
    //public class ValidTokenHandler
    //        : AuthorizationHandler<ValidTokenRequirement>
    //{
    //    //protected override Task HandleRequirementAsync(
    //    //    AuthorizationHandlerContext context,
    //    //    ValidTokenRequirement requirement)
    //    //{
    //    //    // ✅ Cookie se authenticated user check
    //    //    //if (context.User?.Identity?.IsAuthenticated == true &&
    //    //    //    context.User.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
    //    //    //{
    //    //    //    context.Succeed(requirement);
    //    //    //}

    //    //    //return Task.CompletedTask;
    //    //}
    //}
}
