using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Configuration
{
    public class BugTracker : IExceptionHandler
    {
        private readonly ILogger<BugTracker> logger;

        public BugTracker(ILogger<BugTracker> logger)
        {
            this.logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var exceptionMessage = exception.Message;
            //var methodName = ExtractMethodNameFromPageName(exceptionMessage);
            var pageName = httpContext.Request.Path;

            // Determine the status code to use
            int statusCode = httpContext.Response.StatusCode;

            // Default to 500 if statusCode is still zero or not set
            if (statusCode == 0)
            {
                statusCode = 500;
            }

            //logger.LogError(
            //    "Error Message: {exceptionMessage}, Method: {methodName}, Page: {pageName}, Status Code: {statusCode}, Time of occurrence: {time}",
            //    exceptionMessage, pageName, statusCode, DateTime.UtcNow);

            var traceId = httpContext.TraceIdentifier;

            var response = new DomainModel.Common.DMBugTracker
            {
                //Type = "https://httpstatuses.com/" + statusCode, // Example: URL or identifier for the status code
                BugModule = "Error", // Set a general title or customize based on status code
                BugMethod = "",
                BugPage = traceId,
                ControlerRouteName = pageName,
                BugMessage = exceptionMessage,
                BugDateTime = DateTime.Now,
                BugUserId = "",
                GroupId = "",
                HospitalId = "",
                VCId = ""
            };

            //httpContext.Response.StatusCode = statusCode;
            //httpContext.Response.ContentType = "application/problem+json";
            //await httpContext.Response.WriteAsJsonAsync(response);

            // Return true to indicate that this exception is handled


            return true;
        }

    }
}
