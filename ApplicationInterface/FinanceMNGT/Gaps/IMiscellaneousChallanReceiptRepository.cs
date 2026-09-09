using DomainModel.FinanceMNGT;

namespace ApplicationInterface.FinanceMNGT
{
    public interface IMiscellaneousChallanReceiptRepository
    {
        Task<string> AddUpdateMiscChallan(MiscellaneousChallanModel model);
        Task<IEnumerable<MiscellaneousChallanModel>> GetMiscChallanList(string groupCode, string branchCode, long sessionId, long? studentId);

        Task<string> AddMiscReceipt(MiscellaneousReceiptModel model);
        Task<IEnumerable<MiscellaneousReceiptModel>> GetMiscReceiptList(string groupCode, string branchCode, long sessionId, long? studentId);
    }
}
