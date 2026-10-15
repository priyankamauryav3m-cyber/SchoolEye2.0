using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using ServerWebAPI.Authorization;

namespace SchoolEye.Tests.TestHelpers;

/// <summary>Reflection checks for SchoolEye's custom authorization attributes.</summary>
public static class SecurityAssert
{
    public static IEnumerable<MethodInfo> Actions(Type controller) =>
        controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                  .Where(m => m.GetCustomAttributes<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>().Any());

    public static bool HasAuthorize(Type controller) =>
        controller.GetCustomAttribute<AuthorizeAttribute>() != null;

    public static string[] AnonymousActions(Type controller) =>
        Actions(controller).Where(m => m.GetCustomAttribute<AllowAnonymousAttribute>() != null)
                           .Select(m => m.Name).OrderBy(n => n).ToArray();

    public static bool IsApiController(Type controller) =>
        controller.GetCustomAttribute<ApiControllerAttribute>() != null;
}
