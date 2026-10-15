using DomainModel.Common;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ServerWebUI.Shared
{
    // A date the API sends as a string (2026-10-01, 2026-10-01T00:00:00, 2026/10/01, 10/01/2026 12:00:00 AM ...)
    // is shown in AppConfigurationSettings:DateFormat, so pages need no ToString for it.
    // Such a date going back to the API is sent as yyyy-MM-dd, which SQL Server reads the same way on every server.
    // Only the date is kept, a time part is dropped.
    public class DateStringConverter : JsonConverter<string>
    {
        // year first: 2026-10-01, 2026/10/01, with optional time and zone
        private static readonly Regex YearFirst = new(
            @"^(\d{4})[-/](\d{1,2})[-/](\d{1,2})([T ]\d{1,2}:\d{2}(:\d{2}(\.\d+)?)?\s*([AaPp][Mm])?(Z|[+-]\d{2}:?\d{2})?)?$",
            RegexOptions.Compiled);

        // year last with slash: 10/01/2026, 1/10/2026 12:00:00 AM
        private static readonly Regex SlashYearLast = new(
            @"^(\d{1,2})/(\d{1,2})/(\d{4})(\s+\d{1,2}:\d{2}(:\d{2}(\.\d+)?)?\s*([AaPp][Mm])?)?$",
            RegexOptions.Compiled);

        private readonly string _dateFormat;
        private readonly bool _slashDayFirst;

        public DateStringConverter(ApplicationConfiguration settings)
        {
            _dateFormat = string.IsNullOrWhiteSpace(settings?.DateFormat) ? "dd-MM-yyyy" : settings.DateFormat;
            _slashDayFirst = settings?.ApiSlashDateFormat?.TrimStart().StartsWith("d", StringComparison.OrdinalIgnoreCase) == true;
        }

        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return FormatForDisplay(value);
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            if (value != null
                && DateTime.TryParseExact(value, _dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                writer.WriteStringValue(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                return;
            }
            writer.WriteStringValue(value);
        }

        public string? FormatForDisplay(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > 40)
                return value;

            var text = value.Trim();
            int year, month, day;

            var match = YearFirst.Match(text);
            if (match.Success)
            {
                year = int.Parse(match.Groups[1].Value);
                month = int.Parse(match.Groups[2].Value);
                day = int.Parse(match.Groups[3].Value);
            }
            else
            {
                match = SlashYearLast.Match(text);
                if (!match.Success)
                    return value;

                var first = int.Parse(match.Groups[1].Value);
                var second = int.Parse(match.Groups[2].Value);
                year = int.Parse(match.Groups[3].Value);
                // a part above 12 can only be the day; otherwise the configured order decides
                bool dayFirst = first > 12 || (second <= 12 && _slashDayFirst);
                day = dayFirst ? first : second;
                month = dayFirst ? second : first;
            }

            if (year < 1 || month < 1 || month > 12 || day < 1 || day > DateTime.DaysInMonth(year, month))
                return value;

            return new DateTime(year, month, day).ToString(_dateFormat, CultureInfo.InvariantCulture);
        }
    }
}
