using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DomainModel.Admin
{
    #region  -------------------------- Role Menu Order -------------
    // One row of the role's menu tree (V3M_RoleMenuOrder_GetByRole)
    public class RoleMenuOrderRow
    {
        public int ModuleId { get; set; }
        public string? MName { get; set; }
        public int ModuleOrder { get; set; }
        public bool IsModuleCustom { get; set; }
        public int FeatureId { get; set; }
        public string? FeaturesName { get; set; }
        public int FeatureOrder { get; set; }
        public bool IsFeatureCustom { get; set; }
        public int ActivityId { get; set; }
        public string? ActivityName { get; set; }
        public string? DisplayName { get; set; }
        public int ActivityOrder { get; set; }
        public bool IsActivityCustom { get; set; }
    }

    // LevelType: M = Module, F = Feature, A = Activity
    public class RoleMenuOrderItem
    {
        [Required]
        [RegularExpression("^[MFA]$", ErrorMessage = "LevelType must be M, F or A")]
        public string LevelType { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int RefId { get; set; }

        [Range(0, 9999)]
        public int DisplayOrder { get; set; }
    }

    public class RoleMenuOrderSaveRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please select a role")]
        public int RoleId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Nothing to save")]
        public List<RoleMenuOrderItem> Items { get; set; } = new();
    }
    #endregion
}
