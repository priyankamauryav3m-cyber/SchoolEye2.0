using DomainModel.FinanceMNGT;

namespace ApplicationInterface.FinanceMNGT
{
    public interface IJournalRepository
    {
        Task<string> AddUpdateAccountGroup(JournalAccountGroupModel model);
        Task<IEnumerable<JournalAccountGroupModel>> GetAccountGroupList(string groupCode, string branchCode);

        Task<string> AddUpdateAccount(JournalAccountModel model);
        Task<IEnumerable<JournalAccountModel>> GetAccountList(string groupCode, string branchCode);

        Task<(string ReturnValue, long VoucherId)> AddJournalVoucher(JournalVoucherModel model);
        Task<IEnumerable<JournalVoucherModel>> GetJournalVoucherList(string groupCode, string branchCode, long sessionId);
        Task<IEnumerable<JournalVoucherDetailModel>> GetJournalVoucherDetail(long voucherId);
    }
}
