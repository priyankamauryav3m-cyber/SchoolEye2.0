using System.Globalization;
using System.Text.RegularExpressions;

namespace DomainModel.Common
{
    // DPDP: how a student's / parent's personal data is shown on every normal screen.
    // The API sends the real value; the Blazor page shows only the masked text (Blazor Server renders
    // on the server, so the real value never reaches the browser). The model value itself is never
    // changed, so saving, SMS, search and other logic keep working with the real data.
    // The real value is visible and editable only on the Student Sensitive Details page.
    public static class PiiMask
    {
        public const string HiddenText = "••••••";

        // 9876543221 -> 98******21
        public static string Mobile(string? value)
        {
            var v = Clean(value);
            if (v.Length == 0) return "";
            if (v.Length < 5) return new string('*', v.Length);
            return v[..2] + new string('*', v.Length - 4) + v[^2..];
        }

        // rahul.sharma@gmail.com -> ra****@gmail.com
        public static string Email(string? value)
        {
            var v = Clean(value);
            if (v.Length == 0) return "";
            var at = v.IndexOf('@');
            if (at <= 0) return "****";
            var name = v[..at];
            return (name.Length <= 2 ? name[..1] : name[..2]) + "****" + v[at..];
        }

        // 123456781234 -> XXXX XXXX 1234
        public static string Aadhaar(string? value)
        {
            var v = Clean(value).Replace(" ", "").Replace("-", "");
            if (v.Length == 0) return "";
            if (v.Length < 5) return "XXXX";
            return "XXXX XXXX " + v[^4..];
        }

        // 2015-04-12 -> **/**/2015
        public static string Dob(DateTime? value) => value.HasValue ? "**/**/" + value.Value.Year.ToString(CultureInfo.InvariantCulture) : "";

        public static string Dob(DateOnly? value) => value.HasValue ? "**/**/" + value.Value.Year.ToString(CultureInfo.InvariantCulture) : "";

        public static string Dob(string? value)
        {
            var v = Clean(value);
            if (v.Length == 0) return "";
            var formats = new[] { "yyyy-MM-dd", "dd/MM/yyyy", "dd-MM-yyyy", "MM/dd/yyyy", "dd MMM yyyy", "yyyy-MM-ddTHH:mm:ss" };
            if (DateTime.TryParseExact(v, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                || DateTime.TryParse(v, CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
                return Dob(d);
            return "**/**/****";
        }

        // birthday lists need the day, not the year: 12 Apr ****
        public static string BirthdayOnly(DateTime? value) => value.HasValue ? value.Value.ToString("dd MMM", CultureInfo.InvariantCulture) + " ****" : "";

        // H.No 12, Lajpat Nagar, Delhi 110024 -> ****, ****, Delhi 110***
        public static string Address(string? value)
        {
            var v = Clean(value);
            if (v.Length == 0) return "";
            var parts = v.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length <= 1) return "****";
            var last = Regex.Replace(parts[^1], @"(\d{3})\d{3}", "$1***");
            return string.Join(", ", parts[..^1].Select(_ => "****")) + ", " + last;
        }

        // 110024 -> 110***
        public static string PinCode(string? value)
        {
            var v = Clean(value);
            if (v.Length == 0) return "";
            return v.Length <= 3 ? "***" : v[..3] + new string('*', v.Length - 3);
        }

        // 00112233444321 -> XXXXXXXXXX4321
        public static string BankAccount(string? value)
        {
            var v = Clean(value);
            if (v.Length == 0) return "";
            if (v.Length < 5) return "XXXX";
            return new string('X', v.Length - 4) + v[^4..];
        }

        // SBIN0001234 -> SBIN0******
        public static string Ifsc(string? value)
        {
            var v = Clean(value);
            if (v.Length == 0) return "";
            return v.Length <= 5 ? "*****" : v[..5] + new string('*', v.Length - 5);
        }

        // health notes, disability, blood group, income: never shown
        public static string Hidden(string? value) => string.IsNullOrWhiteSpace(value) ? "" : HiddenText;

        public static string Hidden(decimal? value) => value.HasValue && value.Value != 0 ? HiddenText : "";

        public static string Mask(string? value, SensitiveFieldType type) => type switch
        {
            SensitiveFieldType.Mobile or SensitiveFieldType.Phone => Mobile(value),
            SensitiveFieldType.Email => Email(value),
            SensitiveFieldType.Aadhaar => Aadhaar(value),
            SensitiveFieldType.Date => Dob(value),
            SensitiveFieldType.Address => Address(value),
            SensitiveFieldType.PinCode => PinCode(value),
            SensitiveFieldType.BankAccount => BankAccount(value),
            SensitiveFieldType.Ifsc => Ifsc(value),
            _ => Hidden(value)
        };

        private static string Clean(string? value) => (value ?? "").Trim();
    }
}
