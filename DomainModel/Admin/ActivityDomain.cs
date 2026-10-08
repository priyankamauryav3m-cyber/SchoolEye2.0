using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DomainModel.Admin
{
    #region  -------------------------- Activity (menu page) add -------------
    // Request for SuperAdminModule/InsertMstActivityListNew -> Usp_InsertMstActivityList.
    // Lengths follow the MstActivityList columns. Validated on the client (DataAnnotationsValidator)
    // and again on the server (ModelState).
    public class ActivityCreateRequest : IValidatableObject
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please select a module.")]
        public int ModuleId { get; set; }   // filters the feature dropdown

        [Range(1, int.MaxValue, ErrorMessage = "Please select a feature.")]
        public int FeatureId { get; set; }

        [Required(ErrorMessage = "Activity name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Activity name must be 2 to 50 characters.")]
        [RegularExpression(@"^[A-Za-z0-9][A-Za-z0-9 &()/.\-]*$", ErrorMessage = "Activity name can contain letters, numbers, spaces and & ( ) / . -")]
        public string? ActivityName { get; set; }

        [Required(ErrorMessage = "Display name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Display name must be 2 to 50 characters.")]
        public string? DisplayName { get; set; }

        [Range(1, 9999, ErrorMessage = "Display order must be between 1 and 9999.")]
        public int DisplayOrder { get; set; } = 1;

        public bool IsValid { get; set; } = true;

        [Required(ErrorMessage = "URL is required.")]
        [StringLength(200, ErrorMessage = "URL can be at most 200 characters.")]
        [RegularExpression(@"^/[A-Za-z0-9][A-Za-z0-9/_\-]*$", ErrorMessage = "URL must start with / and contain only letters, numbers, / _ - (e.g. /ViewStudentReceipt).")]
        public string? URL { get; set; }

        [StringLength(30, ErrorMessage = "Module label can be at most 30 characters.")]
        public string? ModuleLebal { get; set; }

        [StringLength(100, ErrorMessage = "Icon can be at most 100 characters.")]
        [RegularExpression(@"^bi bi-[a-z0-9-]+$", ErrorMessage = "Please pick an icon from the icon list.")]
        public string? LabelIcon { get; set; }

        // standard permissions
        public bool IsAdd { get; set; }
        public bool IsModifiy { get; set; }
        public bool IsPrint { get; set; }
        public bool IsExportToExcel { get; set; }
        public bool IsPII { get; set; }

        // custom actions
        public bool Action1 { get; set; }
        [StringLength(50, ErrorMessage = "Action 1 description can be at most 50 characters.")]
        public string? Action1Desc { get; set; }

        public bool Action2 { get; set; }
        [StringLength(50, ErrorMessage = "Action 2 description can be at most 50 characters.")]
        public string? Action2Desc { get; set; }

        public bool Action3 { get; set; }
        [StringLength(50, ErrorMessage = "Action 3 description can be at most 50 characters.")]
        public string? Action3Desc { get; set; }

        // an enabled custom action needs a description
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Action1 && string.IsNullOrWhiteSpace(Action1Desc))
                yield return new ValidationResult("Action 1 description is required when Action 1 is enabled.", new[] { nameof(Action1Desc) });
            if (Action2 && string.IsNullOrWhiteSpace(Action2Desc))
                yield return new ValidationResult("Action 2 description is required when Action 2 is enabled.", new[] { nameof(Action2Desc) });
            if (Action3 && string.IsNullOrWhiteSpace(Action3Desc))
                yield return new ValidationResult("Action 3 description is required when Action 3 is enabled.", new[] { nameof(Action3Desc) });
        }
    }
    #endregion
}
