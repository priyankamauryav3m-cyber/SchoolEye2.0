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

        // payment review of a saved registration (PaymentKey: G|B|Session|OAId); null = invalid key / not found
        Task<SummerActivityPaymentInfo?> GetPaymentAsync(string key);

        // Usp_InitiateSummerActivityPayment: 1 order ready, 2 already paid (Receipt set),
        // -1 not found / invalid key, -3 no fee, -99 error
        Task<(int Code, SummerActivityCheckoutInfo? Info)> InitiatePaymentAsync(string key);

        // local test payment (SummerActivity:TestPayment = true) -> Usp_CompleteSummerActivityPayment:
        // 1 paid (Receipt set), -1 order not found, -2 already paid, -3 order expired, -4 test payment off, -99 error
        Task<(int Code, SummerActivityPaymentReceipt? Receipt)> CompleteTestPaymentAsync(SummerActivityPayRequest request);
    }
}
