using DomainModel.SchoolAdmin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolAdmin
{
    // Summer Activity page, Registrations tab (login only)
    public interface ISummerActivityRegistrationView
    {
        // Usp_GetSummerActivityRegistrationList
        Task<IEnumerable<SummerActivityRegistrationListModel>> GetRegistrationsAsync(SummerActivityRegistrationSearchModel search);

        // Usp_GetSummerActivityRegistrationDetail: filled form of one registration
        Task<IEnumerable<SummerActivityRegistrationFieldValue>> GetRegistrationDetailAsync(SummerActivityRegistrationKeyModel model);

        // Usp_GetSummerActivityPaymentModes
        Task<IEnumerable<string>> GetPaymentModesAsync(string groupCode, string branchCode);

        // Usp_ActiveDeactiveSummerActivityRegistration: 1 changed, -1 not found
        Task<int> ActiveDeactiveAsync(SummerActivityRegistrationStatusModel model);
    }
}
