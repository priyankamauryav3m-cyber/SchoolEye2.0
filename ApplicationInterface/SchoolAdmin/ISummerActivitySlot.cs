using DomainModel.SchoolAdmin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolAdmin
{
    public interface ISummerActivitySlot
    {
        // USP_AddUpdateSummerActivitySlots: 1 inserted, 2 updated, -1 not found, -99 error
        // -2 = the activity / slot is not of this group / branch / session
        Task<int> AddUpdateSlotAsync(SummerActivitySlotModel model);

        // Usp_GetSummerActivitySlotList
        Task<IEnumerable<SummerActivitySlotModel>> GetSlotsAsync(SummerActivitySlotSearchModel search);

        // Usp_ActiveDeactiveSummerActivitySlot: 1 changed, -1 not found
        Task<int> ActiveDeactiveSlotAsync(SummerActivitySlotStatusModel model);
    }
}
