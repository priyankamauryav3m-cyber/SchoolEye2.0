using DomainModel.FinanceMNGT;

namespace ApplicationInterface.FinanceMNGT
{
    public interface ILedgerRepository
    {
        Task<IEnumerable<StudentLedgerEntryModel>> GetStudentLedger(StudentLedgerRequestModel request);
        Task<IEnumerable<FeeHeadLedgerEntryModel>> GetFeeHeadLedger(FeeHeadLedgerRequestModel request);
    }
}
