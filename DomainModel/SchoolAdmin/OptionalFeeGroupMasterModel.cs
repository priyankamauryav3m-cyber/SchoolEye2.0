using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DomainModel.SchoolAdmin
{
    // one row of V3M_FIN_MstOptionalFeeGroup (FIN_USp_OptionalFeeGroup / FIN_Usp_GetOptionalFeeGroupList)
    public class OptionalFeeGroupMasterModel
    {
        public int SeedId { get; set; }

        [Required(ErrorMessage = "Activity group name is required.")]
        [StringLength(50, ErrorMessage = "Activity group name can be up to 50 characters.")]
        public string? ActivityGroupName { get; set; }

        public bool IsValid { get; set; }
        public string? CreatedBy { get; set; }

        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }
    }

    // page heading filters: activity group name (part), status (1 / 0 / -1 all)
    public class OptionalFeeGroupMasterSearchModel
    {
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }

        [StringLength(50)]
        public string? ActivityGroupName { get; set; }

        [Range(-1, 1, ErrorMessage = "Status must be 1 (active), 0 (deactive) or -1 (all).")]
        public int Valid { get; set; } = 1;
    }

    // ---------------- Map Group (V3M_FIN_DetOptionalGroupFeeHeadMapping) ----------------

    // one mapped fee head row of a group (USp_FIN_GetOptionalGroupFeeHeadMapping)
    public class OptionalGroupFeeHeadMapRow
    {
        public string? ClassCode { get; set; }
        public int FeeHeadId { get; set; }
        public decimal Amount { get; set; }
        public DateTime? TillDate { get; set; }
    }

    // load the mapping of one group
    public class OptionalGroupMapSearchModel
    {
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Invalid optional fee group.")]
        public int OptFeeGroupId { get; set; }
    }

    public class OptionalGroupFeeHeadAmount
    {
        [Range(1, int.MaxValue, ErrorMessage = "Invalid fee head.")]
        public int FeeHeadId { get; set; }

        [Range(typeof(decimal), "0", "9999999999999999.99", ErrorMessage = "Amount cannot be negative.")]
        public decimal Amount { get; set; }
    }

    // one class of the popup: FeeHeads empty = unmap the class
    public class OptionalGroupClassMapModel
    {
        [Required(ErrorMessage = "Class is required.")]
        [StringLength(20)]
        public string? ClassCode { get; set; }

        public string? ClassName { get; set; }

        public DateTime? TillDate { get; set; }

        public List<OptionalGroupFeeHeadAmount> FeeHeads { get; set; } = new();
    }

    // Save of the popup: every changed class of one group (one transaction)
    public class OptionalGroupMapSaveModel
    {
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Invalid optional fee group.")]
        public int OptFeeGroupId { get; set; }

        public string? CreatedBy { get; set; }

        [MinLength(1, ErrorMessage = "No class to save.")]
        public List<OptionalGroupClassMapModel> Classes { get; set; } = new();
    }

    // result of the Save: Code 1 ok, -1 group not found / not active, -2/-3/-4 invalid class row (ClassName set)
    public class OptionalGroupMapSaveResult
    {
        public int Code { get; set; }
        public string? ClassName { get; set; }
        public int Mapped { get; set; }
        public int Unmapped { get; set; }
    }

    // active / deactive of one group
    public class OptionalFeeGroupMasterStatusModel
    {
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Invalid optional fee group.")]
        public int SeedId { get; set; }

        // true = activate, false = deactivate
        public bool IsValid { get; set; }
    }
}
