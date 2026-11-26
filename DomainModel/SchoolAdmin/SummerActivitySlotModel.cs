using System;
using System.ComponentModel.DataAnnotations;

namespace DomainModel.SchoolAdmin
{
    // one row of SummerActivitySlots (USP_AddUpdateSummerActivitySlots / Usp_GetSummerActivitySlotList)
    public class SummerActivitySlotModel
    {
        public int SASId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select an activity.")]
        public int SAId { get; set; }

        public string? ActivityName { get; set; }

        [Required(ErrorMessage = "Slot name is required.")]
        [StringLength(50, ErrorMessage = "Slot name can be up to 50 characters.")]
        public string? SlotName { get; set; }

        [Required(ErrorMessage = "Fee amount is required.")]
        [Range(typeof(decimal), "0.01", "99999999.99", ErrorMessage = "Fee amount must be greater than 0 (up to 99999999.99).")]
        public decimal? FeeAmount { get; set; }

        public bool IsValid { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }

        // group / branch / session of the activity: checked by the API before saving
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }
        public long SessionId { get; set; }
    }

    // page heading filters: session, activity (0 = all), slot name (part), status (1 / 0 / -1 both)
    public class SummerActivitySlotSearchModel
    {
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Please select a session.")]
        public long SessionId { get; set; }

        [Range(0, int.MaxValue)]
        public int ActivityId { get; set; }

        [StringLength(50)]
        public string? SlotName { get; set; }

        [Range(-1, 1, ErrorMessage = "Status must be 1 (active), 0 (deactive) or -1 (both).")]
        public int Valid { get; set; } = 1;
    }

    // active / deactive of one slot
    public class SummerActivitySlotStatusModel
    {
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Invalid slot.")]
        public int SASId { get; set; }

        // true = activate, false = deactivate
        public bool IsValid { get; set; }
    }
}
