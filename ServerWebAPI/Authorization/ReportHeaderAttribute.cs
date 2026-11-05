using ApplicationInterface.GenerateFile;
using Infrastructure.FileGenerate;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections;
using System.Reflection;

namespace ServerWebAPI.Authorization
{
    // On controllers that make PDF / Excel: loads the school header (V3M_MstReportHeader) for the
    // request's GroupCode / BranchCode and keeps it in ReportHeaderContext for the documents.
    // GroupCode / BranchCode are taken from the request body (object or first item of a list);
    // when not found, V3M_GetReportHeader returns the first valid header.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ReportHeaderAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var headerService = context.HttpContext.RequestServices.GetService<IReportHeaderService>();
            if (headerService != null)
            {
                var (groupCode, branchCode) = FindCodes(context.ActionArguments.Values);
                ReportHeaderContext.Current = await headerService.GetReportHeaderAsync(groupCode, branchCode);
            }

            await next();
        }

        private static (string? GroupCode, string? BranchCode) FindCodes(IEnumerable<object?> arguments)
        {
            foreach (var argument in arguments)
            {
                var item = argument is IEnumerable list and not string
                    ? list.Cast<object?>().FirstOrDefault(x => x != null)
                    : argument;
                if (item == null)
                    continue;

                var groupCode = ReadString(item, "GroupCode");
                if (!string.IsNullOrWhiteSpace(groupCode))
                    return (groupCode, ReadString(item, "BranchCode"));
            }
            return (null, null);
        }

        private static string? ReadString(object item, string propertyName) =>
            item.GetType()
                .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)?
                .GetValue(item)?
                .ToString();
    }
}
