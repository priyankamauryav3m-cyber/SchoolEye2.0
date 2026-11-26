using DomainModel.SchoolAdmin;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolAdmin
{
    // public registration page of a summer activity (tiny link ?key=)
    public interface ISummerActivityRegistration
    {
        // Code: 1 ok (Form set), -6 invalid link, -1 activity not found / not active, -2 registration closed
        Task<(int Code, SummerActivityFormInfo? Form)> GetFormAsync(string key);

        // validates every value against the activity's active fields and saves one row in OtherActivities
        Task<SummerActivityRegistrationResult> RegisterAsync(SummerActivityRegistrationRequest request);
    }
}
