using DomainModel.FinanceMNGT;

namespace ApplicationInterface.FinanceMNGT
{
    public interface IOptionalFeeGroupRepository
    {
        Task<string> AddUpdateOptionalFeeGroup(OptionalFeeGroupModel model);
        Task<IEnumerable<OptionalFeeGroupModel>> GetOptionalFeeGroupList(string groupCode, string branchCode, long sessionId);
        Task ToggleOptionalFeeGroup(OptionalFeeGroupModel model);
        Task MapClass(OptionalFeeGroupClassMapModel model);
        Task MapFeeHead(OptionalFeeGroupHeadModel model);
    }
}
