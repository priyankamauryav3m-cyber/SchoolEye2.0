using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DomainModel.SchoolAdmin
{
    // public summer activity registration page (opened from the tiny link, no login)

    // ?key=<token> of the link: GroupCode|BranchCode|SessionId|SAId encrypted
    public class SummerActivityFormRequest
    {
        [Required(ErrorMessage = "Invalid link.")]
        [StringLength(500, ErrorMessage = "Invalid link.")]
        public string? Key { get; set; }
    }

    // what the page needs to draw the form (no group / branch / session codes)
    public class SummerActivityFormInfo
    {
        public string? SchoolName { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }

        public string? ActivityName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public List<SummerActivitySlotOption> Slots { get; set; } = new();
        public List<SummerActivityFormField> Fields { get; set; } = new();
    }

    public class SummerActivitySlotOption
    {
        public int SASId { get; set; }
        public string? SlotName { get; set; }
        public decimal FeeAmount { get; set; }
    }

    public class SummerActivityFormField
    {
        public string FieldName { get; set; } = "";
        public string? DisplayName { get; set; }
        public int DisplayOrder { get; set; }
        public int? MaxLength { get; set; }
        public string? FieldType { get; set; }
        public string? DataType { get; set; }
        public List<string> Options { get; set; } = new();
        public bool IsMandatory { get; set; }
    }

    // filled form
    public class SummerActivityRegistrationRequest
    {
        [Required(ErrorMessage = "Invalid link.")]
        [StringLength(500, ErrorMessage = "Invalid link.")]
        public string? Key { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a slot.")]
        public int SASId { get; set; }

        // FieldName -> value (checkbox list: comma separated, date: yyyy-MM-dd, tick: 1 / 0)
        public Dictionary<string, string?> Values { get; set; } = new();
    }

    public class SummerActivityRegistrationResult
    {
        // 1 saved; -1 activity not found / not active, -2 registration closed, -3 slot not available,
        // -4 mandatory field missing, -5 invalid values, -6 invalid link, -99 not saved
        public int Code { get; set; }
        public int? RegistrationId { get; set; }
        public string? ActivityName { get; set; }
        public string? SlotName { get; set; }
        public decimal Amount { get; set; }
        // encrypted G|B|Session|OAId of the saved registration (payment step), "+" -> "V3M"
        public string? PaymentKey { get; set; }
        // field name -> message
        public Dictionary<string, string> Errors { get; set; } = new();
    }
}

namespace DomainModel.SchoolAdmin
{
    // payment review page after the registration (key = SummerActivityRegistrationResult.PaymentKey)
    public class SummerActivityPaymentInfo
    {
        public int RegistrationId { get; set; }
        public string? SchoolName { get; set; }
        public string? ActivityName { get; set; }
        public string? SlotName { get; set; }
        public decimal ActivityFee { get; set; }
        public decimal ConvenienceFee { get; set; }
        public decimal TotalAmount => ActivityFee + ConvenienceFee;
        public bool IsFeePaid { get; set; }
    }

    // checkout page (/summer-activity-checkout?key=PaymentKey): the order of Usp_InitiateSummerActivityPayment
    public class SummerActivityCheckoutInfo
    {
        public SummerActivityPaymentInfo Payment { get; set; } = new();
        // order id (RefNo) of the pending transaction
        public string? OrderId { get; set; }
        // seconds left to pay this order
        public int SecondsLeft { get; set; }
        // local test payment is on (no money is charged)
        public bool IsTestMode { get; set; }
        // filled when the fee is already paid
        public SummerActivityPaymentReceipt? Receipt { get; set; }
    }

    public class SummerActivityPayRequest
    {
        [Required(ErrorMessage = "This payment link is not valid.")]
        [StringLength(500, ErrorMessage = "This payment link is not valid.")]
        public string? Key { get; set; }

        [Required(ErrorMessage = "Order was not found.")]
        [StringLength(100, ErrorMessage = "Order was not found.")]
        public string? OrderId { get; set; }

        // UPI id: name@bank (checked only, not saved)
        [Required(ErrorMessage = "Please enter your UPI ID.")]
        [StringLength(100, ErrorMessage = "Please enter a valid UPI ID.")]
        [RegularExpression(@"^[A-Za-z0-9][A-Za-z0-9.\-_]{1,255}@[A-Za-z][A-Za-z0-9]{1,63}$", ErrorMessage = "Please enter a valid UPI ID (e.g. name@okbank).")]
        public string? UpiId { get; set; }
    }

    public class SummerActivityPaymentReceipt
    {
        public int RegistrationId { get; set; }
        public string? SchoolName { get; set; }
        public string? ActivityName { get; set; }
        public string? SlotName { get; set; }
        public decimal Amount { get; set; }
        public string? OrderId { get; set; }
        public string? TransactionId { get; set; }
        public string? UtrNo { get; set; }
        public string? PaymentMode { get; set; }
        public DateTime? PaidOn { get; set; }
    }
}
