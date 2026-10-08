using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MyApp.Common;

namespace ServerWebAPI.Dependency
{
    // [ApiController] normally answers an invalid model with ProblemDetails, which the
    // Blazor HttpService cannot read. Put this on an action to get the validation
    // messages back as ApiResponse instead. Runs just before ModelStateInvalidFilter (-2000).
    public class ApiResponseValidationAttribute : ActionFilterAttribute
    {
        public ApiResponseValidationAttribute()
        {
            Order = -2001;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid)
                return;

            // only our own validation messages; JSON/binding errors ("$..." keys) stay generic
            var messages = context.ModelState
                .Where(kv => !kv.Key.StartsWith("$"))
                .SelectMany(kv => kv.Value!.Errors)
                .Where(e => e.Exception == null && !string.IsNullOrWhiteSpace(e.ErrorMessage))
                .Select(e => e.ErrorMessage)
                .Distinct()
                .ToArray();

            var message = messages.FirstOrDefault() ?? "Invalid request data.";
            context.Result = new BadRequestObjectResult(ApiResponse<string>.Fail(message, messages));
        }
    }
}
