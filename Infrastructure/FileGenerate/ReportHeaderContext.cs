using DomainModel.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Infrastructure.FileGenerate
{
    // Header (V3M_MstReportHeader) of the PDF / Excel being made in the current API request.
    // Set once per request by the API filter [ReportHeader] (ServerWebAPI/Common/ReportHeaderAttribute.cs),
    // read by every PDF document (CreateDocument.cs) and Excel sheet (StudentDocumentService.cs).
    public static class ReportHeaderContext
    {
        private static readonly AsyncLocal<ReportHeaderInfo?> CurrentHeader = new();

        public static ReportHeaderInfo Current
        {
            get => CurrentHeader.Value ?? new ReportHeaderInfo();
            set => CurrentHeader.Value = value;
        }

        public static string SchoolName => Current.SchoolName;
        public static string Address => Current.Address;
        public static string Phone => Current.Phone;
        public static string Email => Current.Email;
        public static string Website => Current.Website;

        // school logo used on all prints (API wwwroot)
        public static string LogoPath =>
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "Logos", "logo (1).png");

        // address split in two lines (used where the old layout had two address lines)
        public static (string Line1, string Line2) AddressLines
        {
            get
            {
                // cut at the comma nearest to the middle
                var address = Address.Trim();
                var cut = Enumerable.Range(0, address.Length)
                    .Where(i => address[i] == ',')
                    .OrderBy(i => System.Math.Abs(i - address.Length / 2))
                    .DefaultIfEmpty(-1)
                    .First();
                return cut < 0
                    ? (address, string.Empty)
                    : (address[..cut].Trim(), address[(cut + 1)..].Trim());
            }
        }

        // non-empty contact parts in one line, e.g. "Phone: 0837... | Email: ..."
        public static string ContactLine
        {
            get
            {
                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(Phone)) parts.Add($"Phone: {Phone}");
                if (!string.IsNullOrWhiteSpace(Email)) parts.Add($"Email: {Email}");
                if (!string.IsNullOrWhiteSpace(Website)) parts.Add(Website);
                return string.Join(" | ", parts);
            }
        }

        public static bool HasWebsite => !string.IsNullOrWhiteSpace(Website);
        public static bool HasPhone => !string.IsNullOrWhiteSpace(Phone);
    }
}
