using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DomainModel.SchoolAdmin
{
    // one field of the dynamic (summer activity) form: DynamicFormController
    // (USP_AddUpdateDynamicFormController2 / Usp_GetFieldNameList)
    public class DynamicFormControllerModel : IValidatableObject
    {
        // field types of the form; Dropdown / Radio need Option Values
        public static readonly string[] FieldTypes = { "Textbox", "Dropdown", "Radio", "Checkbox", "DateTime" };
        public static readonly string[] OptionFieldTypes = { "Dropdown", "Radio" };
        // data types known by USP_AddUpdateDynamicFormController2 (column type of OtherActivities)
        public static readonly string[] DataTypes = { "string", "int", "long", "decimal", "double", "datetime", "bit" };

        public int DFCId { get; set; }
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }

        // becomes a column of OtherActivities: letters, digits and _ only
        [Required(ErrorMessage = "Field name is required.")]
        [StringLength(100, ErrorMessage = "Field name can be up to 100 characters.")]
        [RegularExpression("^[A-Za-z][A-Za-z0-9_]*$", ErrorMessage = "Field name: start with a letter; only letters, digits and _ (no spaces).")]
        public string? FieldName { get; set; }

        [Required(ErrorMessage = "Display name is required.")]
        [StringLength(100, ErrorMessage = "Display name can be up to 100 characters.")]
        public string? DisplayName { get; set; }

        [Required(ErrorMessage = "Display order is required.")]
        [Range(1, 9999, ErrorMessage = "Display order must be between 1 and 9999.")]
        public int? DisplayOrder { get; set; }

        [Range(1, 4000, ErrorMessage = "Max length must be between 1 and 4000.")]
        public int? MaxLength { get; set; }

        [Required(ErrorMessage = "Please select a field type.")]
        public string? FieldType { get; set; }

        [Required(ErrorMessage = "Please select a data type.")]
        public string? DataType { get; set; }

        // comma separated (Dropdown / Radio)
        [StringLength(4000, ErrorMessage = "Option values can be up to 4000 characters.")]
        public string? OptionValues { get; set; }

        public bool IsMandatory { get; set; }
        public bool IsValid { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select an activity.")]
        public int SAId { get; set; }
        public string? ActivityName { get; set; }

        // session of the activity: checked by the API before saving
        public long SessionId { get; set; }

        public bool NeedsOptions => OptionFieldTypes.Contains(FieldType ?? "", StringComparer.OrdinalIgnoreCase);

        // Checkbox may be a list of options (e.g. Hobbies) or a single tick
        public bool AllowsOptions => NeedsOptions || string.Equals(FieldType, "Checkbox", StringComparison.OrdinalIgnoreCase);

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(FieldType) && !FieldTypes.Contains(FieldType, StringComparer.OrdinalIgnoreCase))
                yield return new ValidationResult("Invalid field type.", new[] { nameof(FieldType) });

            if (!string.IsNullOrWhiteSpace(DataType) && !DataTypes.Contains(DataType, StringComparer.OrdinalIgnoreCase))
                yield return new ValidationResult("Invalid data type.", new[] { nameof(DataType) });

            if (string.Equals(DataType, "string", StringComparison.OrdinalIgnoreCase) && !(MaxLength > 0))
                yield return new ValidationResult("Max length is required for the string data type.", new[] { nameof(MaxLength) });

            if (NeedsOptions)
            {
                var options = SplitOptions(OptionValues);
                if (options.Count < 2)
                    yield return new ValidationResult("Enter at least 2 option values (comma separated) for Dropdown / Radio.", new[] { nameof(OptionValues) });
                else if (options.Distinct(StringComparer.OrdinalIgnoreCase).Count() != options.Count)
                    yield return new ValidationResult("Option values must not repeat.", new[] { nameof(OptionValues) });
            }
        }

        public static List<string> SplitOptions(string? values) =>
            (values ?? "").Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .ToList();
    }

    // page heading filters: session, activity (0 = all), field name (part), status (1 / 0 / -1 all)
    public class DynamicFormControllerSearchModel
    {
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Please select a session.")]
        public long SessionId { get; set; }

        [Range(0, int.MaxValue)]
        public int ActivityId { get; set; }

        [StringLength(100)]
        public string? FieldName { get; set; }

        [Range(-1, 1, ErrorMessage = "Status must be 1 (active), 0 (deactive) or -1 (all).")]
        public int Valid { get; set; } = 1;
    }

    // active / deactive of one field
    public class DynamicFormControllerStatusModel
    {
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Invalid field.")]
        public int DFCId { get; set; }

        // true = activate, false = deactivate
        public bool IsValid { get; set; }
    }
}
