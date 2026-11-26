using DomainModel.SchoolAdmin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolAdmin
{
    public interface ISummerActivity
    {
        // USP_AddUpdateSummerActivities: 1 inserted, 2 updated, -1 not found, -99 error
        Task<int> AddUpdateSummerActivityAsync(SummerActivityModel model);

        // Usp_GetSummerActivitiesList
        Task<IEnumerable<SummerActivityModel>> GetSummerActivitiesAsync(SummerActivitySearchModel search);

        // Usp_ActiveDeactiveSummerActivity: 1 changed, -1 not found
        Task<int> ActiveDeactiveSummerActivityAsync(SummerActivityStatusModel model);

        // tiny registration link of an active activity (DefaultSettings TinyUrl -> Blazor page SummerActivity:RegistrationPageUrl?key=,
        // Alert_Usp_GenerateSummerActiviryLink); saved in SummerActivities.Link
        Task<SummerActivityLinkResult> GenerateLinkAsync(SummerActivityLinkRequest request, string? createdBy);

        // Excel of the searched activities
        Task<byte[]> ExportSummerActivitiesAsync(SummerActivitySearchModel search);
    }
}
