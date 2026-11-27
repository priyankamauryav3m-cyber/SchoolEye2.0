using System;
using System.ComponentModel.DataAnnotations;

namespace DomainModel.SchoolAdmin
{
    // Summer Activity page, Registrations tab: submitted registrations (OtherActivities)

    // Usp_GetSummerActivityRegistrationList
    public class SummerActivityRegistrationListModel
    {
        public int RegistrationId { get; set; }
        public int SAId { get; set; }
        public string? ActivityName { get; set; }
        public string? SlotName { get; set; }
        public decimal Amount { get; set; }
        public bool IsFeePaid { get; set; }
        public string? PaymentMode { get; set; }
        public string? OrderId { get; set; }
        public string? TransactionId { get; set; }
        public string? UtrNo { get; set; }
        public DateTime? PaidOn { get; set; }
        public DateTime? RegisteredOn { get; set; }
        public bool IsValid { get; set; }
    }

    public class SummerActivityRegistrationSearchModel
    {
        public const string NotPaid = "NOTPAID";

        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Please select a session.")]
        public long SessionId { get; set; }

        // 0 = all activities
        [Range(0, int.MaxValue)]
        public int ActivityId { get; set; }

        [Range(-1, 1, ErrorMessage = "Status must be 1 (active), 0 (deactive) or -1 (all).")]
        public int Valid { get; set; } = 1;

        // "" = all, NOTPAID = fee not paid, otherwise the saved payment mode (UPI, ...)
        [StringLength(50)]
        public string? PaymentMode { get; set; }
    }

    // one filled field of a registration (Usp_GetSummerActivityRegistrationDetail)
    public class SummerActivityRegistrationFieldValue
    {
        public string? FieldName { get; set; }
        public string? DisplayName { get; set; }
        public string? FieldType { get; set; }
        public string? Value { get; set; }
    }

    public class SummerActivityRegistrationKeyModel
    {
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Invalid registration.")]
        public int RegistrationId { get; set; }
    }

    public class SummerActivityRegistrationStatusModel : SummerActivityRegistrationKeyModel
    {
        // true = activate, false = deactivate
        public bool IsValid { get; set; }
    }
}
