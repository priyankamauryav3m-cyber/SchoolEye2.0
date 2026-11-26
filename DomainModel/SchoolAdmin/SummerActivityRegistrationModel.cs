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
        // field name -> message
        public Dictionary<string, string> Errors { get; set; } = new();
    }
}
