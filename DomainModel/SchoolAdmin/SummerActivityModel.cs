using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DomainModel.SchoolAdmin
{
    // one row of SummerActivities (USP_AddUpdateSummerActivities / Usp_GetSummerActivitiesList)
    public class SummerActivityModel : IValidatableObject
    {
        public int SAId { get; set; }
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Please select a session.")]
        public long SessionId { get; set; }

        [Required(ErrorMessage = "Activity name is required.")]
        [StringLength(50, ErrorMessage = "Activity name can be up to 50 characters.")]
        public string? ActivityName { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        public DateTime? EndDate { get; set; }

        public bool IsValid { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? Link { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDate.HasValue && EndDate.HasValue && EndDate.Value.Date < StartDate.Value.Date)
                yield return new ValidationResult("End date cannot be before the start date.", new[] { nameof(EndDate) });
        }
    }

    // page heading filters: session, activity (0 = all), status (1 active, 0 deactive, -1 both)
    public class SummerActivitySearchModel
    {
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Please select a session.")]
        public long SessionId { get; set; }

        [Range(0, int.MaxValue)]
        public int ActivityId { get; set; }

        [Range(-1, 1, ErrorMessage = "Status must be 1 (active), 0 (deactive) or -1 (both).")]
        public int Valid { get; set; } = 1;
    }

    // generate the registration link (tiny url) of one activity
    public class SummerActivityLinkRequest
    {
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Please select a session.")]
        public long SessionId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Invalid activity.")]
        public int SAId { get; set; }
    }

    public class SummerActivityLinkResult
    {
        // 1 generated, 0 TinyUrl not set, 2 SummerActivity:RegistrationPageUrl not set,
        // -1 activity not found / not active, -99 not saved
        public int Code { get; set; }
        public string? Link { get; set; }
    }

    // active / deactive of one activity
    public class SummerActivityStatusModel
    {
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Invalid activity.")]
        public int SAId { get; set; }

        // true = activate, false = deactivate
        public bool IsValid { get; set; }
    }
}
