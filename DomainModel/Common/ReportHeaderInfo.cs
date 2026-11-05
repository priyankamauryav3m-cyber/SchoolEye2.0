namespace DomainModel.Common
{
    // Header of every PDF / Excel (school name, address, phone ...).
    // Source: V3M_MstReportHeader (SP V3M_GetReportHeader) -> HeaderRow1..HeaderRow5.
    public class ReportHeaderInfo
    {
        public string GroupCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string SchoolName { get; set; } = string.Empty;   // HeaderRow1
        public string Address { get; set; } = string.Empty;      // HeaderRow2
        public string Phone { get; set; } = string.Empty;        // HeaderRow3
        public string Email { get; set; } = string.Empty;        // HeaderRow4
        public string Website { get; set; } = string.Empty;      // HeaderRow5
        public bool LogoInHeader { get; set; } = true;
    }
}
