using DomainModel.SchoolAdmin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolAdmin
{
    public interface IOptionalFeeGroupMaster
    {
        // FIN_USp_OptionalFeeGroup (@Status): 1 inserted, 2 updated, 3 name already exists; -1 not found, -99 error
        Task<int> AddUpdateOptionalFeeGroupAsync(OptionalFeeGroupMasterModel model);

        // FIN_Usp_GetOptionalFeeGroupList
        Task<IEnumerable<OptionalFeeGroupMasterModel>> GetOptionalFeeGroupsAsync(OptionalFeeGroupMasterSearchModel search);

        // FIN_Usp_ActiveDeactiveOptionalFeeGroup: 1 changed, -1 not found
        Task<int> ActiveDeactiveOptionalFeeGroupAsync(OptionalFeeGroupMasterStatusModel model);

        // USp_FIN_GetOptionalGroupFeeHeadMapping: active class / fee head rows of one group
        Task<IEnumerable<OptionalGroupFeeHeadMapRow>> GetMappingAsync(OptionalGroupMapSearchModel search);

        // one transaction: USp_FIN_DetOptionalGroupFeeHeadMapping (class with heads, TVP)
        // or USp_FIN_UnmapOptionalGroupClass (class without heads) for every class
        Task<OptionalGroupMapSaveResult> SaveMappingAsync(OptionalGroupMapSaveModel model);
    }
}
