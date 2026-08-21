//using DomainModel.Enum;
//using System.Globalization;
//using System.Reflection;

//namespace ServerWebAPI.Dependency
//{
//    public static class CultureFormatter
//    {
//        public static Dictionary<string, object?> Format<T>(T obj) where T : class
//        {
//            CultureInfo culture = CultureInfo.CurrentCulture;

//            var result = new Dictionary<string, object?>();
//            var properties = typeof(T).GetProperties();

//            foreach (var prop in properties)
//            {
//                object? value = prop.GetValue(obj);
//                var attr = prop.GetCustomAttribute<GlobalFormatAttribute>();

//                if (attr == null)
//                {
//                    result[prop.Name] = value;
//                    continue;
//                }

//                if (value == null)
//                {
//                    result[prop.Name] = null;
//                    continue;
//                }

//                result[prop.Name] = attr.Type switch
//                {
//                    FormatType.Currency when value is decimal dec
//                        => dec.ToString("C", culture),

//                    FormatType.Number when value is decimal num
//                        => num.ToString("N2", culture),

//                    FormatType.Percentage when value is decimal pct
//                        => pct.ToString("P", culture),

//                    FormatType.Date_ddMMyy when value is DateTime d1
//                        => d1.ToString("dd/MM/yy", culture),

//                    FormatType.Date_ddMMyyyy_Dash when value is DateTime d2
//                        => d2.ToString("dd-MM-yyyy", culture),

//                    FormatType.Date_ddMMMMyyyy when value is DateTime d3
//                        => d3.ToString("dd MMMM yyyy", culture),

//                    FormatType.DateTime_Full when value is DateTime d4
//                        => d4.ToString("dd/MM/yyyy HH:mm:ss", culture),

//                    _ => value
//                };
//            }

//            return result;
//        }
//    }
//}