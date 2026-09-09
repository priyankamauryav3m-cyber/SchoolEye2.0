using DomainModel.FinanceMNGT;

namespace ApplicationInterface.FinanceMNGT
{
    public interface IAdjustmentReceiptRepository
    {
        Task<string> AddAdjustmentReceipt(AdjustmentReceiptModel model);
        Task<string> ApproveAdjustmentReceipt(long adjustmentId, string groupCode, string branchCode, long sessionId, string approvedBy);
        Task<IEnumerable<AdjustmentReceiptModel>> GetAdjustmentReceiptList(string groupCode, string branchCode, long sessionId, long? studentId);
    }
}
