using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections;
using System.Reflection;

namespace ServerWebAPI.Dependency
{
    public class GlobalFormatFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result is not ObjectResult objectResult || objectResult.Value == null)
                return;

            objectResult.Value = ProcessValue(objectResult.Value);
        }

        private object? ProcessValue(object? value)
        {
            if (value == null)
                return null;

            var valueType = value.GetType();

            if (valueType == typeof(string))
                return value;

            if (value is byte[] byteArray)
                return Convert.ToBase64String(byteArray);

            if (value is IEnumerable enumerable)
            {
                var newList = new List<object?>();
                foreach (var item in enumerable)
                {
                    newList.Add(ProcessValue(item));
                }
                return newList;
            }

            if (valueType.IsPrimitive || valueType.IsEnum ||
                valueType == typeof(DateTime) || valueType == typeof(DateTime?) ||
                valueType == typeof(decimal) || valueType == typeof(decimal?) ||
                valueType == typeof(Guid) || valueType == typeof(Guid?) ||
                valueType == typeof(bool) || valueType == typeof(bool?))
            {
                return value;
            }

            if (valueType.IsClass)
            {
                var ns = valueType.Namespace ?? "";
                if (ns.StartsWith("System") || ns.StartsWith("Microsoft"))
                {
                    return value;
                }

                var result = new Dictionary<string, object?>();
                var properties = valueType.GetProperties();

                foreach (var prop in properties)
                {
                    if (!prop.CanRead) continue;

                    object? propValue;
                    try
                    {
                        propValue = prop.GetValue(value);
                    }
                    catch
                    {
                        continue;
                    }

                    var formatAttr = prop.GetCustomAttribute<GlobalFormatAttribute>();

                    if (formatAttr != null && propValue != null)
                    {
                        result[prop.Name] = FormatSingleValue(propValue, formatAttr.Type);
                    }
                    else
                    {
                        result[prop.Name] = ProcessValue(propValue);
                    }
                }

                return result;
            }

            return value;
        }

        private object? FormatSingleValue(object value, DomainModel.Enum.FormatType type)
        {
            var culture = System.Globalization.CultureInfo.CurrentCulture;

            return type switch
            {
                DomainModel.Enum.FormatType.Currency when value is decimal dec
                    => dec.ToString("C", culture),

                DomainModel.Enum.FormatType.Number when value is decimal num
                    => num.ToString("N2", culture),

                DomainModel.Enum.FormatType.Percentage when value is decimal pct
                    => pct.ToString("P", culture),

                DomainModel.Enum.FormatType.Date_ddMMyy when value is DateTime d1
                    => d1.ToString("dd/MM/yy", culture),

                DomainModel.Enum.FormatType.Date_ddMMyyyy_Dash when value is DateTime d2
                    => d2.ToString("dd-MM-yyyy", culture),

                DomainModel.Enum.FormatType.Date_ddMMMMyyyy when value is DateTime d3
                    => d3.ToString("dd MMMM yyyy", culture),

                DomainModel.Enum.FormatType.DateTime_Full when value is DateTime d4
                    => d4.ToString("dd/MM/yyyy HH:mm:ss", culture),

                _ => value
            };
        }

        public void OnActionExecuting(ActionExecutingContext context) { }
    }
}