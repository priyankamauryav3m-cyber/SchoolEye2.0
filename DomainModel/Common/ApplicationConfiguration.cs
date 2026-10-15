using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.Common
{
    public class ApplicationConfiguration
    {
        public string DNSAPIUrl { get; set; } = default!;
        public string LocalAPIURL { get; set; } = default!;
        public string Version { get; set; } = default!;
        public string App { get; set; } = default!;
        public string AppName { get; set; } = default!;
        public string AppFlavor { get; set; } = default!;
        public string AppFlavorSubscript { get; set; } = default!;
        public string Company { get; set; } = default!;
        public string Copyright { get; set; } = default!;
        // UI: every date the API sends as a string is shown in this format
        public string DateFormat { get; set; } = "dd-MM-yyyy";
        // UI: order of a slash date from the API (10/01/2026) when both parts are 12 or less
        public string ApiSlashDateFormat { get; set; } = "MM/dd/yyyy";
    }
}
