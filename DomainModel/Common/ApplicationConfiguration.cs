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
    }
}
