using System.ComponentModel.DataAnnotations;

namespace DomainModel.Admin
{
    #region -------------------------- DPDP : Student Sensitive Details -------------

    // request for the page data (unmasked) / history
    public class StudentSensitiveDetailsRequest
    {
        [Required, StringLength(10)]
        public string GroupCode { get; set; } = "";

        [Required, StringLength(10)]
        public string BranchCode { get; set; } = "";

        [Range(1, long.MaxValue, ErrorMessage = "Select a session.")]
        public long SessionId { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Select a student.")]
        public long StudentId { get; set; }

        // history only: one field key, empty = all fields
        [StringLength(50)]
        public string? FieldKey { get; set; }
    }

    // permission check for the page (no student needed)
    public class StudentSensitiveAccessRequest
    {
        [Required, StringLength(10)]
        public string GroupCode { get; set; } = "";

        [Required, StringLength(10)]
        public string BranchCode { get; set; } = "";
    }

    public class StudentSensitiveHeader
    {
        public long StudentId { get; set; }
        public string? ControlNo { get; set; }
        public string? StudentName { get; set; }
        public string? ClassSection { get; set; }
        public string? Gender { get; set; }
        public string? FatherName { get; set; }
        public string? MotherName { get; set; }
        public string? GuardianName { get; set; }
        public string? GuardianRelationship { get; set; }
        public string? EmergencyPersonName { get; set; }
        public string? EmergencyPersonRelationship { get; set; }
    }

    // one sensitive field (FieldKey = DomainModel.Common.SensitiveFieldCatalog key)
    public class StudentSensitiveField
    {
        public string FieldKey { get; set; } = "";
        public string? FieldValue { get; set; }
        public int ChangeCount { get; set; }
        public DateTime? LastChangedOn { get; set; }
        public string? LastChangedBy { get; set; }
    }

    public class StudentSensitiveDetailsResponse
    {
        public StudentSensitiveHeader Header { get; set; } = new();
        public List<StudentSensitiveField> Fields { get; set; } = new();
    }

    public class StudentSensitiveFieldChange
    {
        [Required, StringLength(50)]
        public string FieldKey { get; set; } = "";

        [StringLength(400)]
        public string? NewValue { get; set; }
    }

    // only the changed fields are sent; the SP reads the old values itself
    public class StudentSensitiveUpdateRequest
    {
        [Required, StringLength(10)]
        public string GroupCode { get; set; } = "";

        [Required, StringLength(10)]
        public string BranchCode { get; set; } = "";

        [Range(1, long.MaxValue, ErrorMessage = "Select a session.")]
        public long SessionId { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Select a student.")]
        public long StudentId { get; set; }

        [Required(ErrorMessage = "Enter the reason for the change.")]
        [StringLength(300, MinimumLength = 5, ErrorMessage = "Reason must be 5 to 300 characters.")]
        public string Reason { get; set; } = "";

        [Required, MinLength(1, ErrorMessage = "No field is changed.")]
        public List<StudentSensitiveFieldChange> Changes { get; set; } = new();
    }

    public class StudentSensitiveUpdateResult
    {
        // 1 saved | 3 nothing changed | -1 not found | -2 unknown field | -3 reason missing | -4 invalid date/number | 0 error
        public int Result { get; set; }
        public int ChangedCount { get; set; }
    }

    public class StudentSensitiveHistoryRow
    {
        public long LogId { get; set; }
        public string? FieldKey { get; set; }
        public string? TableName { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? ChangedBy { get; set; }
        public string? IP { get; set; }
        public DateTime? ChangedOn { get; set; }
        public string? Reason { get; set; }
    }

    public class PiiAccessResult
    {
        public bool HasAccess { get; set; }
        public string? UserName { get; set; }
    }

    #endregion
}
