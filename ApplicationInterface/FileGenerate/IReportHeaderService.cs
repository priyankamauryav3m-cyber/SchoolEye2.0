using DomainModel.Common;
using System.Threading.Tasks;

namespace ApplicationInterface.GenerateFile
{
    // PDF / Excel header from V3M_MstReportHeader (SP V3M_GetReportHeader)
    public interface IReportHeaderService
    {
        Task<ReportHeaderInfo> GetReportHeaderAsync(string? groupCode, string? branchCode);
    }
}
