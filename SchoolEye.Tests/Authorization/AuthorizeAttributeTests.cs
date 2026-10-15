using DomainModel.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using ServerWebAPI.Authorization;

namespace SchoolEye.Tests.Authorization;

/// <summary>
/// The custom [Authorize] filter protects RegistrationController and AdmissionDateController.
/// </summary>
public class AuthorizeAttributeTests
{
    private static AuthorizationFilterContext CreateContext(object? user, params object[] endpointMetadata)
    {
        var http = new DefaultHttpContext();
        if (user != null)
            http.Items["User"] = user;

        var actionContext = new ActionContext(http, new RouteData(), new ActionDescriptor
        {
            EndpointMetadata = endpointMetadata.ToList()
        });
        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }

    [Fact]
    public void OnAuthorization_NoUser_Returns401()
    {
        var context = CreateContext(user: null);

        new AuthorizeAttribute().OnAuthorization(context);

        var json = Assert.IsType<JsonResult>(context.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, json.StatusCode);
    }

    [Fact]
    public void OnAuthorization_AuthenticatedUser_AllowsRequest()
    {
        var context = CreateContext(new UserModels { Username = "test.user" });

        new AuthorizeAttribute().OnAuthorization(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void OnAuthorization_AllowAnonymousMetadata_SkipsCheck()
    {
        var context = CreateContext(user: null, new AllowAnonymousAttribute());

        new AuthorizeAttribute().OnAuthorization(context);

        Assert.Null(context.Result);
    }

    // CLAUDE.md §8A.9 / §14: an unexpected object in HttpContext.Items["User"] must be rejected with 401, not crash the request (hard cast throws InvalidCastException).
    [Fact]
    public void OnAuthorization_WrongUserType_Returns401WithoutThrowing()
    {
        var context = CreateContext(user: "not-a-user-model");

        var ex = Record.Exception(() => new AuthorizeAttribute().OnAuthorization(context));

        Assert.Null(ex);
        Assert.Equal(StatusCodes.Status401Unauthorized, Assert.IsType<JsonResult>(context.Result).StatusCode);
    }
}
