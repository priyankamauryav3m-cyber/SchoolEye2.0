using DomainModel.Enum;
using DomainModel.Resources;
using DomainModel.Resources.Resource;
using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DomainModel.FinanceMNGT;

namespace DomainModel.Admin
{
    #region  -------------------------- Super Admin Role Master -------------
    public class SuperAdminDomain : BaseEntity
    {
        public bool IsDirty { get; set; } = false;
        public int RoleId { get; set; }

        [Display(Name = "Role name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? RoleName { get; set; }
        [Display(Name = "Role descripation")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]

        public string? RoleDescripation { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Please select a dashboard")]
        public int DashBoardId { get; set; }
    }
    #endregion

    #region  -------------------------- Common Base Entity  -------------
    public class BaseEntity
    {
        [Range(0, 100, ErrorMessage = "Display Order must be between 0 and 100")]
        public int DisplayOrder { get; set; }
        //public int DisplayOrder { get; set; }
        public string? Icon { get; set; }
        public bool IsValid { get; set; } = true;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int FeatureId { get; set; }
        public bool IsSelected { get; set; }
        public bool IsSaved { get; set; }
        public int? MinAge { get; set; }   // 👈 add
        public int? MaxAge { get; set; }

    }
    #endregion

    #region  -------------------------- Super Admin Module  -------------
    public class SuperAdminModule : BaseEntity
    {
        public int ModuleId { get; set; }

        //[Required(ErrorMessage = "MName is required.")]

        [Display(Name = "Module name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Module name  can contain only letters and spaces")]
        public string? MName { get; set; }
        [Display(Name = "Display name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Display Name  can contain only letters and spaces")]
        public string? DisplayName { get; set; }
        //[Required(ErrorMessage = "Description is required.")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Description name must not contain numbers.")]
        public string? Description { get; set; }

        public bool IsExpanded { get; set; }
        public bool IsRestricted { get; set; }


        public List<SuperAdminFeatures> Features { get; set; } = new();
    }
    #endregion

    #region  -------------------------- Super Admin Feature -------------
    public class SuperAdminFeatures : BaseEntity
    {


        [Display(Name = "Display name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? FeaturesName { get; set; }
        public int ModuleId { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public bool IsExpanded { get; set; }

        public List<SuperAdminActivity> Activites { get; set; } = new();
    }
    #endregion

    #region  -------------------------- Super Admin Activity -------------
    public class SuperAdminActivity : BaseEntity
    {
        [Key]
        public int ActivityId { get; set; }

        [Display(Name = "Features name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? ActivityName { get; set; }
        public string? DisplayName { get; set; }
        public bool? IsAdd { get; set; }
        public bool? IsModifiy { get; set; }
        public bool? IsPrint { get; set; }
        public bool? IsExportToExcel { get; set; }
        public bool? IsPII { get; set; }
        public bool? Action1 { get; set; }
        [Display(Name = "Action1Desc")]
        [MaxLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? Action1Desc { get; set; }
        public bool? Action2 { get; set; }

        [Display(Name = "Action2Desc")]
        [MaxLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? Action2Desc { get; set; }
        public bool? Action3 { get; set; }

        [Display(Name = "Action3Desc")]
        [MaxLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? Action3Desc { get; set; }

        [Display(Name = "URL")]
        [MaxLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? URL { get; set; }
    }
    #endregion

    #region  -------------------------- Super Admin Access Details -------------
    public class ControlAccess
    {
        public int FeatureId { get; set; }
        public int AccessId { get; set; }
        public int ModuleId { get; set; }
        public int ActivityId { get; set; }
        public int RoleId { get; set; }
        public bool? IsAdd { get; set; }
        public bool IsSelected { get; set; }
        public bool IsActive { get; set; } = true;
        public string? CreatedBy { get; set; }
        public bool? IsModifiy { get; set; }
        public bool? IsPrint { get; set; }
        public bool? IsExportToExcel { get; set; }
        public bool? IsPII { get; set; }
        public bool? Action1 { get; set; }


        [Display(Name = "Action1Desc")]
        [MaxLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? Action1Desc { get; set; }
        public bool? Action2 { get; set; }

        [Display(Name = "Action2Desc")]
        [MaxLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? Action2Desc { get; set; }
        public bool? Action3 { get; set; }

        [Display(Name = "Action3Desc")]
        [MaxLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? Action3Desc { get; set; }
    }
    #endregion

    #region  -------------------------- Role Base Details -------------
    public class RoleaBase
    {
        public int UserSid { get; set; }
        public string? UserName { get; set; }
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public int ModuleId { get; set; }
        public string? MName { get; set; }
        public int IsRestricted { get; set; }
        public int FeatureId { get; set; }
        public string? FeaturesName { get; set; }
        public int ActivityId { get; set; }
        public string? ActivityName { get; set; }
        public string? URL { get; set; }
        public string? DisplayName { get; set; }
        public int DisplayOrder { get; set; }
        public string? Icon { get; set; }
        public string? LabelIcon { get; set; }
    }

    public class RolebaseActivity : RoleaBase
    {
        public bool IsAdd { get; set; }
        public bool IsModifiy { get; set; }
        public bool IsPrint { get; set; }
        public bool IsExportToExcel { get; set; }
        public bool IsPII { get; set; }
        public bool Action1 { get; set; }
        public bool Action2 { get; set; }
        public bool Action3 { get; set; }
    }
    #endregion

    #region  -------------------------- Branch Class Master -------------
    public class BranchClassModel : BaseEntity
    {
        public int ClassId { get; set; }

        public string? ClassCode { get; set; }
        public string? ClassName { get; set; }

        public int? ClassOrder { get; set; }

        public string? TCDisplayName { get; set; }



        public DateTime? CreatedDate { get; set; }


        public string? TallyGroupName { get; set; }

        public bool IsShowOnlineReg { get; set; }
    }
    #endregion

    #region ------------------------Registration Model-------------- 
    public class RegistrationModal
    {

        [Display(Name = "Group code")]
        [StringLength(10, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? GroupCode { get; set; }

        [Display(Name = "Branch code")]
        [StringLength(10, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? BranchCode { get; set; }

        //[Display(Name = "Session name")]
        //[StringLength(10, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public long SessionId { get; set; }
        [Display(Name = "First name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? FirstName { get; set; }
        [Display(Name = "Middle name")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? MiddleName { get; set; }
        [Display(Name = "Last name")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        //[Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? LastName { get; set; }
        [Display(Name = "Gender")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(10, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? Gender { get; set; }

        [Display(Name = "Child name")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? ChildName { get; set; }
        [Display(Name = "Date of birth ")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]


        public DateTime? DateOfBirth { get; set; }
        [Display(Name = "Class")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(10, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? ClassCode { get; set; }

        [Display(Name = "Stream code ")]
        [StringLength(20, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? StreamCode { get; set; }
        [Display(Name = "Mother title")]
        [StringLength(10, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? MotherTitle { get; set; }

        [Display(Name = "Mother Name")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? MotherName { get; set; }
        [StringLength(100)]
        public string? MotherLName { get; set; }

        [Display(Name = "Mother contect number")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? MotherContactNo { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.com$", ErrorMessage = "Please enter a valid email ")]
        public string? MotherEmailId { get; set; }

        [Display(Name = "Father title")]
        [StringLength(10, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? FatherTitle { get; set; }

        [Display(Name = "Father name")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? FatherName { get; set; }
        [StringLength(100)]
        public string? FatherLName { get; set; }

        [Display(Name = "Father contect number")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? FatherContactNo { get; set; }

        [Display(Name = "Father email address")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.com$", ErrorMessage = "Please enter a valid email ")]
        public string? FatherEmailId { get; set; }
        [Display(Name = "SMS mobile")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]

        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "SMS Mobile No must be 10 digits")]
        [StringLength(13, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? SMSMobileNo { get; set; }
        public int StudentCategory { get; set; }

        [Display(Name = "Address To Whome")]
        [StringLength(10, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? AddressToWhome { get; set; }

        [Display(Name = "Address line1")]
        [StringLength(150, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? AddressLine1 { get; set; }

        [Display(Name = "Address line2")]
        [StringLength(150, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? AddressLine2 { get; set; }

        [Display(Name = "Pincode")]
        [StringLength(10, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? Pincode { get; set; }

        [Display(Name = "Contect number")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? ContactNo { get; set; }
        public int EWS { get; set; }
        public int Sibling { get; set; }
        [Range(typeof(decimal), "0", "99999", ErrorMessage = "Registration Fee cannot exceed 6 digits.")]
        public decimal RegistrationFee { get; set; }
        [StringLength(50)]
        public string? CreatedBy { get; set; }
        [Display(Name = "Payment mode")]
        [StringLength(30, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? PaymentMode { get; set; }

        [Display(Name = "Remarks")]
        [StringLength(300, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? Remarks { get; set; }

        [Display(Name = "CBSE roll no")]
        [StringLength(20, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? CBSERollNo { get; set; }
        [StringLength(20)]
        public string? RegistrationNo { get; set; }
        public int EnquiryId { get; set; }
    }
    public record RegistrationDto
    {
        public bool IsSelected { get; set; }
        public string? RegistrationNo { get; set; }
        public long RegistrationId { get; set; }
        public string? StudentName { get; set; }
        public long SessionId { get; set; }
        public string? Gender { get; set; }
        public string? DateOfBirth { get; set; }
        public string? AppliedDate { get; set; } = "";
        public string? ApplicationStatus { get; set; } = "";
        public string? FatherName { get; set; } = "";
        public string? ClassName { get; set; }
        public string? FatherContactNo { get; set; } = "";
        public int Sibling { get; set; }
        public decimal? RegistrationFee { get; set; }
        public string? PaymentMode { get; set; } = "";
        public string? FatherEMail { get; set; } 
        public int Points { get; set; } = 0;
        public int IsReservedSeat { get; set; } = 0;
    }
    public class RegistrationSearchDto:MNGTCommon
    {
        public string? ClassCode { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateFrom { get; set; } = DateTime.Today;
        public DateTime? DateTo { get; set; } = DateTime.Today;
        public string? RegistrationNo { get; set; }
        public string? EWS { get; set; }
        public string? EWSSrc { get; set; }
        public int? Sibling { get; set; }
        public int? IsTransport { get; set; }
        public int? DocumentSelected { get; set; }
        public int? TransportDistance { get; set; }
        public string? RegistrationFrom { get; set; }
        public string? RegistrationTo { get; set; }
        public string? PointsFrom { get; set; }
        public string? PointsTo { get; set; }
        public string? StatusSrc { get; set; } = "0,1,2";
        public int AppStatus { get; set; } = 3;
        public string? StudentName { get; set; }
        public string? FatherName { get; set; }
        public string? MotherName { get; set; }
        public string? PaymentMode { get; set; }
        public int StudentCategory { get; set; }
    }
    public class RegistrationStatusModel
    {
        public int SeedId { get; set; }
        public string? StatusName { get; set; }
        public string? StatusDate { get; set; }
        public int ApplicationStatus { get; set; }
        public bool IsDone { get; set; }
    }
    public class GetRegistrationModel
    {
        // -------- Registration --------
        public int RegistrationId { get; set; }
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }
        public string? RegistrationNo { get; set; }
        public string? SessionName { get; set; }

        public string? ChildFirstName { get; set; }
        public string? ChildMiddleName { get; set; }
        public string? ChildLastName { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public string? ClassCode { get; set; }
        public string? StreamCode { get; set; }
        public string? BloodGroup { get; set; }

        public bool? IsDisability { get; set; }
        public string? MedicalInformation { get; set; }
        public string? OtherHelthProblem { get; set; }

        public string? BirthPlace { get; set; }
        public int Nationality { get; set; }
        public int SocialCategory { get; set; }
        public int Religion { get; set; }
        public string? MotherTongue { get; set; }

        public string? PreviousClass { get; set; }
        public string? PreviousSchool { get; set; }
        public string? PreviousSchoolAddress { get; set; }

        public bool? IsTransportRequired { get; set; }
        public string? TransportDetails { get; set; }
        public bool? IsHostelRequired { get; set; }
        public bool? IsSMSRequired { get; set; }
        public bool? IsEmailRequired { get; set; }
        public bool? IsNRI { get; set; }

        public string? EmergencyPersonName { get; set; }
        public string? EmergencyPersonRelationShip { get; set; }
        public string? EmergencyPersonContactNo { get; set; }

        public DateTime? AppliedDate { get; set; }
        public DateTime? AdmissionDate { get; set; } = DateTime.Now;
        public decimal? RegistrationFee { get; set; }

        public string? StudentImage { get; set; }
        public string? FatherImage { get; set; }
        public string? MotherImage { get; set; }

        public string? AdmissionNo { get; set; }
        public string? ChildAadhaarNo { get; set; }

        public string? StudentMobileNo { get; set; }
        public string? ReferenceName { get; set; }
        public string? ReferencePhone { get; set; }

        public string? SpecialNeeds { get; set; }
        public string? SpecialNeedDetails { get; set; }

        public string? Remarks { get; set; }
        public string? RegNo { get; set; }
        public string? RegisteredThrough { get; set; }
        public int SectionId { get; set; }
        public string? PaymentMode { get; set; }
        public int StudentCategory { get; set; }

        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }


        // -------- Parent --------
        public int? ParentID { get; set; }

        public string? FatherTitle { get; set; }
        public string? FatherName { get; set; }
        public string? FatherMName { get; set; }
        public string? FatherLName { get; set; }
        public DateTime? FatherDOB { get; set; }

        public string? FatherNationality { get; set; }
        public string? FatherQualification { get; set; }
        public int FatherOccupation { get; set; }
        public string? FatherDesignation { get; set; }
        public string? SMSMobileNo { get; set; }
        public string? FatherEMail { get; set; }
        public decimal? FatherAnnualIncome { get; set; }
        public string? FatherContactNo { get; set; }
        public string? FatherOfficeContactNo { get; set; }
        public string? FatherOfficeAddress { get; set; }

        public string? FatherAadharNo { get; set; }
        public string? FatherWhatsAppNo { get; set; }

        public string? MotherTitle { get; set; }
        public string? MotherName { get; set; }
        public string? MotherMName { get; set; }
        public string? MotherLName { get; set; }
        public DateTime? MotherDOB { get; set; }

        public string? MotherNationality { get; set; }
        public string? MotherQualification { get; set; }
        public int MotherOccupation { get; set; }

        public string? MotherEMail { get; set; }
        public decimal? MotherAnnualIncome { get; set; }
        public string? MotherContactNo { get; set; }
        public string? MotherOfficeContactNo { get; set; }
        public string? MotherOfficeAddress { get; set; }

        public string? MotherAadharNo { get; set; }
        public string? MotherWhatsAppNo { get; set; }

        public string? GuardianName { get; set; }
        public string? GuardianContactNo { get; set; }
        public string? GuardianRelationWithChild { get; set; }
        public string? GuardianAddress { get; set; }

        public string? ContactEmailid { get; set; }
        public bool? Parentlivingtogether { get; set; }

        public decimal? FamilyAnnualInCome { get; set; }


        // -------- Address --------
        public int? AddressID { get; set; }

        public string? Line1 { get; set; }
        public string? Line2 { get; set; }

        public int? DistrictId { get; set; }
        public int? StateId { get; set; }
        public int? CountryId { get; set; }

        public string? PinCode { get; set; }
        public string? ContactNo { get; set; }

        public string? AddressTo { get; set; }
        public string? AddressType { get; set; }

        public string? Area { get; set; }
    }
    #endregion

    #region     -----------Child Details  ------------------------
    public class ChildDetails : GbsrCommonmdl
    {
        [Display(Name = "Child first name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string ChildFirstName { get; set; }
        [Display(Name = "Child middle name")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]

        public string? ChildMiddleName { get; set; }


        [Display(Name = "Child last name")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? ChildLastName { get; set; }
        [Display(Name = "Gender")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Class")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string ClassCode { get; set; }


        public string? StreamCode { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Nationality is required")]
        [Display(Name = "Nationality")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]

        public int? Nationality { get; set; }
        [Display(Name = "Category")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]

        [Range(1, int.MaxValue, ErrorMessage = "Category is required")]
        public int? SocialCategory { get; set; }
        public int Religion { get; set; } = 0;
        public int MotherTongue { get; set; } = 0;

        //[ Range(0, 1)]
        public bool IsTransportRequired { get; set; } = false;

        public string? TransportDetails { get; set; } = "";
        //[Range(0, 1)]
        public bool IsHostelRequired { get; set; } = false;
        public int IsNRI { get; set; } = 0;


        public string EmergencyPersonName { get; set; } = "";
        public string? EmergencyPersonRelationShip { get; set; } = "";

        public string? EmergencyPersonContactNo { get; set; } = "";

        public string? PassportNo { get; set; } = "";
        public int PassportType { get; set; } = 0;

        //[Range(0, 1)]
        public bool IsPassportRegReq { get; set; } = false;
        public DateTime? PassportIssueDate { get; set; } = null;
        public DateTime? PassportExpiryDate { get; set; } = null;

        public string? VisaNo { get; set; } = "";
        public int VisaType { get; set; }

        //[Range(0, 1)]
        public bool IsVisaRegReq { get; set; }

        public DateTime? VisaIssueDate { get; set; } = null;
        public DateTime? VisaExpiryDate { get; set; } = null;
        public int IsDisability { get; set; } = 0;
        public string? CBSERollNo { get; set; } = "";
        public string? AadhaarNo { get; set; } = "";

        [Range(0, 1)]
        public int IsDayCare { get; set; } = 0;
        [Display(Name = "Blood group")]
        [StringLength(10, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? BloodGroup { get; set; } = "";


        [Display(Name = "Medical information")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? MedicalInformation { get; set; } = "";


        [Display(Name = "Othe health problem ")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? OtherHealthProblem { get; set; } = "";

        [Display(Name = "Previous class")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? PreviousClass { get; set; } = "";


        [Display(Name = "Previous school")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? PreviousSchool { get; set; } = "";


        [Display(Name = "Medium of instruction")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? MediumOfInstruction { get; set; } = "";

        [Display(Name = "Previous school address")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? PreviousSchoolAddress { get; set; } = "";


        [Display(Name = "Birth palace")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? BirthPalace { get; set; } = "";

        public string? Background { get; set; } = "";
        public string? AppStatus { get; set; } = "";

    }
    #endregion

    #region     -----------Parents Details  ------------------------
    public class ParentsDetails : GbsrCommonmdl
    {
        // ===== FATHER =====
        public string? FatherTitle { get; set; }
        public string? FatherName { get; set; }
        public string? FatherMiddleName { get; set; }
        public string? FatherLName { get; set; }
        public string? FatherAadharNo { get; set; }
        public int FatherQualification { get; set; } = 0;
        public int? FatherOccupation { get; set; }
        public string? FatherOtherOccupation { get; set; }
        public string? FatherDesignation { get; set; }
        public string? FatherOrganisation { get; set; }
        public string? FatherOfficeAddress { get; set; }
        public string? FatherOfficeContactNo { get; set; }
        public DateTime? FatherDOB { get; set; }
        public int? FatherNationality { get; set; }
        public string? FatherEMail { get; set; }

        public decimal FatherAnnualIncome { get; set; }
 
        public string? FatherContactNo { get; set; }
        public string? FatherAchievement { get; set; }
        public string? FatherMotherTongue { get; set; }
        public string? FatherPlaceOfBirth { get; set; }
        public string? FatherCollege { get; set; }
        public string? FatherSchool { get; set; }

        // ===== MOTHER =====
        public string? MotherTitle { get; set; }
        public string? MotherName { get; set; }


        public string? MotherMiddleName { get; set; }

        public string? MotherLName { get; set; }
        public string? MotherMaidenSurname { get; set; }
        public string? MotherAadharNo { get; set; }
        public int MotherQualification { get; set; } = 0;
        public int? MotherOccupation { get; set; }
        public string? MotherOtherOccupation { get; set; }
        public string? MotherDesignation { get; set; }
        public string? MotherOrganisation { get; set; }
        public string? MotherOfficeAddress { get; set; }
        public string? MotherOfficeContactNo { get; set; }
        public DateTime? MotherDOB { get; set; }
        public int? MotherNationality { get; set; }
        public string? MotherEMail { get; set; }
        public decimal MotherAnnualIncome { get; set; }
        public string? MotherContactNo { get; set; }
        public string? MotherAchievement { get; set; }
        public string? MotherMotherTongue { get; set; }
        public string? MotherPlaceOfBirth { get; set; }
        public string? MotherCollege { get; set; }
        public string? MotherSchool { get; set; }

        // ===== COMMON =====
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? ContactEmailId { get; set; }
        [Display(Name = "SMS mobile no")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? SMSMobileNo { get; set; }
    }
    #endregion

    #region ------------------------------- Points Criteria  -------------------------
    public class PointsCriteria : GbsrCommonmdl
    {
        [MaxLength(50)]
        public string? DistanceFromSchool { get; set; }
        public string? HearingFrom { get; set; }
        public bool IsFatherAlumni { get; set; } = false;
        public bool IsFirstBornChild { get; set; } = false;
        public bool IsGirlChildOnly { get; set; } = false;
        public bool IsReservedSeat { get; set; }
        public int CoreCategory { get; set; } = 1;
        public bool FatherTransferableJob { get; set; } = false;
        public bool MotherTransferableJob { get; set; } = false;
        public bool SingleParent { get; set; } = false;
        public bool IsLegalDocumentHave { get; set; }
        public bool IsStaffWard { get; set; } = false;
        public bool IsChildBelowAge { get; set; } = false;
        public bool IsTwinChild { get; set; } = false;
        public bool IsAdoptedChild { get; set; } = false;
        public bool Sibling { get; set; } = false;
        [MaxLength(50)]
        public string? TransferCity { get; set; }
        public bool IsMotherAlumni { get; set; } = false;
        public bool DefencePerson { get; set; } = false;
        [MaxLength(20)]
        public string? FatherAlumniClass { get; set; }
        [MaxLength(100)]
        public string? DefenceDetail { get; set; }
        [MaxLength(20)]
        public string? MotherAlumniClass { get; set; }
        [MaxLength(100)]
        public string? FatherAlumniBranch { get; set; }
        [MaxLength(100)]
        public string? MotherAlumniBranch { get; set; }
        [MaxLength(50)]
        public string? FatherPassingYear { get; set; }
        [MaxLength(50)]
        public string? MotherPassingYear { get; set; }
        [MaxLength(50)]
        public string? SingleFatherName { get; set; }
        [MaxLength(50)]
        public string? SingleMotherName { get; set; }
        [MaxLength(100)]
        public string? SingleParentComment { get; set; }
        [MaxLength(30)]
        public string? ChildCustody { get; set; }
        public List<GbsrCommonmdl> gbsrmdl { get; set; } = new();
    }
    #endregion

    #region  -------------------------- Address Details -------------
    public class AddressDetails : GbsrCommonmdl
    {

        [Display(Name = "Address Line1")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(150, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? Line1 { get; set; }

        [Display(Name = "Address Line1")]
       [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(150, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? PermanentLine1 { get; set; }


        [Display(Name = "Address Line2")]
        [StringLength(150, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? Line2 { get; set; }

        public string? PinCode { get; set; }

        [Display(Name = "Contact number")]
        [StringLength(10, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? ContactNo { get; set; }

        [Display(Name = "Address to")]
        [StringLength(10, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? AddressTo { get; set; } = "Student";

        [Display(Name = "Address type")]
        [StringLength(1, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? AddressType { get; set; }
        //[MaxLength(50)]
        //public string? Area { get; set; }
        public List<GbsrCommonmdl> gbsrmdl { get; set; } = new();
    }
    #endregion

    #region  -------------------------- Family Details -------------
    public class FamilyDetails : GbsrCommonmdl
    {

        [Display(Name = "Family child name ")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? FamilyChildName { get; set; }

        [Display(Name = "Family child school")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? FamilyChildSchool { get; set; }

        [Display(Name = "Family child class")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? FamilyChildclass { get; set; }


    }
    #endregion

    #region  -------------------------- Commom Property Details -------------
    public class GbsrCommonmdl
    {

        public long SessionId { get; set; }
        [MaxLength(20)]
        public string? SessionName { get; set; }
        public string? RegistrationNo { get; set; }
        [MaxLength(30)]
        public string? CreatedBy { get; set; }
        public long RegistrationId { get; set; }

        [Display(Name = "Group code")]
        [StringLength(10, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? GroupCode { get; set; }

        [Display(Name = "Branch code")]
        [StringLength(10, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? BranchCode { get; set; }
        public bool IsSelected { get; set; }
        public byte[]? Image { get; set; }


    }
    #endregion

    #region  -------------------------- SMS Sent Master -------------

    public class SMSSentModel
    {
        [Display(Name = "Group code")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? GroupCode { get; set; }
        [Display(Name = "Branch code")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? BranchCode { get; set; }
        [Display(Name = "Session name")]
        [StringLength(50)]
        public string? SMS_Or_Mail { get; set; }

        public int MessageType { get; set; }
        [Display(Name = "Message text")]
        [StringLength(1100, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? MessageText { get; set; }
        [Display(Name = "Sent date")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? SentDate { get; set; }
        [Display(Name = "Sent by")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? SentBy { get; set; }
        [Display(Name = "Total messages")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? TotalMsg { get; set; }
        [Display(Name = "Total delivered")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? TotalDelivered { get; set; }
        [Display(Name = "SMS vendor")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? SMSVendor { get; set; }
        public bool? IsValid { get; set; }
        public bool isAttachment { get; set; }

        [StringLength(50)]
        public string? SMS_EmailSentId { get; set; }

    }
    #endregion

    #region  -------------------------- Session Master -------------

    public class SessionModel:CommonClass
    {

        [Display(Name = "Session name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(150, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? Session { get; set; }
        public string? Remarks { get; set; }
        public bool CurrentSession { get; set; }
        //[Display(Name = "Admission session")]
        //[Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public bool AdmissionSession { get; set; }
        public bool IsValid { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Today;
        [Display(Name = "Start Date")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public DateTime StartDate { get; set; } = DateTime.Today;
        [Display(Name = "End date")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public DateTime EndDate { get; set; } = DateTime.Today;
        public int DisplayOrder { get; set; }
    }
    #endregion

    #region  -------------------------- Admission Details -------------
    public class StudentAdmissionModel
    {
        public string GroupCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string? RegistrationNo { get; set; }
        public string? StudentNo { get; set; }
        public string? ControlNo { get; set; }
        public string? SessionName { get; set; }

        public string? Gender { get; set; }
        public string? ImagePath { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? ClassCode { get; set; }
        public string? SectionId { get; set; }

        public string? ClassName { get; set; }
        public string? SectionName { get; set; }
        public string? ClassSection { get; set; }

        public string? StudentName { get; set; }
        public string? FatherName { get; set; }
        public string? MotherName { get; set; }

        public string? RollNo { get; set; }
        public string? SMSMobileNo { get; set; }

        public string? CurrentAddress { get; set; }

        public string? FatherEMail { get; set; }
        public string? MotherEMail { get; set; }

        public string? LoginName { get; set; }
        public string? UserPassword { get; set; }

        public string? BoardRollNo { get; set; }

        public string? FatherContactNo { get; set; }
        public string? MotherContactNo { get; set; }

        public string? ContactEmail { get; set; }
        public string? StudentMiddleName { get; set; }

        public string? StudentLastName { get; set; }

        public string? AadhaarNo { get; set; }
    }
    #endregion

    #region ------------------------------DirectAdmission-----------------------------

    public class StudentDirectAdmissionModel
    {
        public string? GroupCode { get; set; }
        public long StudentId { get; set; }

        public string? BranchCode { get; set; }
        public long SessionId { get; set; }

        [Display(Name = "First name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? FirstName { get; set; }

        public string? MiddleName { get; set; }

        public string? LastName { get; set; }

        [Display(Name = "Gender name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? Gender { get; set; }

        [Display(Name = "Class name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? ClassCode { get; set; }

        [Display(Name = "Date of birth")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public DateTime? DateOfBirth { get; set; }
        [Display(Name = "Admission date")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public DateTime? AdmissionDate { get; set; } = DateTime.Now;

        public int Religion { get; set; }

        public int SocialCategoryId { get; set; }

        public int NationalityId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Please select a section")]
        public int SectionId { get; set; }

        public int IsUsingTpt { get; set; }
        public int RouteDistance { get; set; }

        public string? FatherTitle { get; set; }

        [Display(Name = "Father name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? FatherFirstName { get; set; }

        public string? FatherMiddleName { get; set; }

        public string? FatherLastName { get; set; }

        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Invalid Father Contact No")]
        public string? FatherContactNo { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.com$", ErrorMessage = "Please enter a valid email ")]
        public string? FatherEmailId { get; set; }
        public int FatherOccupation { get; set; }
        public string? FatherOccupationOther { get; set; }
        public string? MotherTitle { get; set; }
        [Display(Name = "Mother name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? MotherFirstName { get; set; }
        public string? MotherMiddleName { get; set; }
        public string? MotherLastName { get; set; }

        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Invalid Mother Contact No")]
        public string? MotherContactNo { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.com$", ErrorMessage = "Please enter a valid email ")]
        public string? MotherEmailId { get; set; }
        public int MotherOccupation { get; set; }   
        public string? MotherOccupationOther { get; set; }

        [Display(Name = "SMS mobile no.")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Invalid SMS Contact")]
        public string? SMSContactNo { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }

        [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "Invalid PinCode")]
        public string? PinCode { get; set; }
        public string? CreatedBy { get; set; }
        public string? PreviousBranchCode { get; set; }
        public int SocietyId { get; set; }

        [RegularExpression(@"^[0-9]{12}$", ErrorMessage = "Student Aadhar must be 12 digits")]
        public string? StudentAadharNo { get; set; }

        [RegularExpression(@"^[0-9]{12}$", ErrorMessage = "Father Aadhar must be 12 digits")]
        public string? FatherAadharNo { get; set; }

        [RegularExpression(@"^[0-9]{12}$", ErrorMessage = "Mother Aadhar must be 12 digits")]
        public string? MotherAadharNo { get; set; }
        public long RegId { get; set; }
        public int FeeTemplateId { get; set; }
        public string? AddContactNo { get; set; }
        public string? AddressTo { get; set; } = "Student";
        [Display(Name = "Admission no.")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? stuManualAdmNo { get; set; }
        public string? ApaarId { get; set; }
        public string? PenNo { get; set; }
        public string? Caste { get; set; }
        public string? StudentControlStudentNo { get; set; }
        public int? MapConAndChallan { get; set; }
        public string ConcessionId { get; set; }
        public DateTime? ConcessionFromDate { get; set; } = DateTime.Now;
        public DateTime? ConcessionToDate { get; set; } = DateTime.Now;
        public string? ConcessionDetails { get; set; }
        public string? ConcessionRemarks { get; set; }
        public string? SiblingID { get; set; }
        public List<FeeHeadDto>? FeeHeadList { get; set; }

    }
    public class CommonDomain:MNGTCommon
    {
        public int ConcStudId { get; set; }
        public bool FieldBool1 { get; set; }
        public bool IsSelectedPersentage { get; set; }
        public string? FeeHeadId { get; set; }
        public string? FeeHeadName { get; set; }
        public decimal FeeHeadAmount { get; set; }
        public string? FeeHeadType { get; set; }
        public string? ConcessionId { get; set; }
        public string? ConcessionType { get; set; }

        public decimal? _concessionApplicable;
        public decimal? ConcessionApplicable
        {
            get => _concessionApplicable;
            set => _concessionApplicable = value;
        }
        public string? ClassCode { get; set; }
        public string? Remarks { get; set; }
        public DateTime FromDate { get; set; } = DateTime.Today;
        public DateTime ToDate { get; set; } = DateTime.Today;
    }
    public class StudentAdmissionResponse
    {
        public long StudentId { get; set; }
        public string? StudentNo { get; set; }
        public string? ControlNo { get; set; }
        public string? LoginId { get; set; }
        public string? StudentControlStudentNo { get; set; }
    }


    #endregion

    #region ------------------------------Search Student List-----------------------------

    public class StudentListRequest : GbsrCommonmdl
    {
        public string? ClassCode { get; set; }

        public string? SectionId { get; set; }

        public string? Gender { get; set; }

        public string? StudentNo { get; set; }

        public string? StudentName { get; set; }

        public bool IsSearchOnAdmDate { get; set; }

        public DateTime? AdmFromDate { get; set; }
        public DateTime? AdmToDate { get; set; }

        public string? ValidStatus { get; set; }

        public string? StudentStatus { get; set; }

        public string? OrderBy { get; set; }

    }

    public class StudentListResponse
    {

        public string? StudentNo { get; set; }

        public string? ControlNo { get; set; }

        public string? AdmissionNo { get; set; }

        public string? StudentName { get; set; }

        public string? RollNo { get; set; }

        public string? Gender { get; set; }

        public string? DateOfBirth { get; set; }

        public string? ClassCode { get; set; }

        public string? ClassName { get; set; }

        public string? SectionId { get; set; }

        public string? SectionName { get; set; }

        public string? ClassSection { get; set; }

        public int IsReservedSeat { get; set; }

        public string? SMSMobileNo { get; set; }

        public string? FatherName { get; set; }

        public string? MotherName { get; set; }

        public string? DistanceName { get; set; }

        public string? AdmissionDate { get; set; }
        public bool IsSelected { get; set; }

    }

    #endregion

    #region -----Sibling------
    public class SiblingDetailResponse
    {
        public string? StudentNo { get; set; }
        public string? StudentName { get; set; }
        public string? FatherName { get; set; }
        public string? MotherName { get; set; }
        public string? ClassName { get; set; }
        public string? SectionName { get; set; }
        public string? BranchName { get; set; }
        public string? SiblingID { get; set; }
    }
    #endregion


    #region ------------ Admission Slip--------
    public class AdmissionSlipModel
    {
        public string RegistrationNo { get; set; }
        public string AdmissionNo { get; set; }
        public string AdmissionDate { get; set; }
        public string StudentName { get; set; }
        public string Gender { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string DOB { get; set; }
        public string Address { get; set; }
        public string SmsMobile { get; set; }
        public string UserName { get; set; }
        public string DefaultPassword { get; set; }
    }
    #endregion
    public class OnlineRegistration : CommonClass
    {
        [Required(ErrorMessage = "Class is required")]
        public string? ClassCode { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        public string? ChildFirstName { get; set; }

        public string? ChildMiddleName { get; set; } = string.Empty;

        public string? ChildLastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of Birth is required")]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string? Gender { get; set; }

        public string? MotherTongue { get; set; } = string.Empty;

        public string? Nationality { get; set; } = string.Empty;

        [Required(ErrorMessage = "Previous school name is required")]
        public string? PreviousSchool { get; set; }

        [Required(ErrorMessage = "Stream is required")]
        public string? StreamCode { get; set; }

        //[Required(ErrorMessage = "Last promoted class is required")]
        public string? LastClass { get; set; } = string.Empty;

        //[Required(ErrorMessage = "Academic session is required")]
        public string? LastClassYear { get; set; } = string.Empty;

        public string? LastClassMonth { get; set; } = string.Empty;

        [Required]
        public bool SpecialNeeds { get; set; } = false;

        public string? SpecialneedsDetail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Residential Address is required")]
        public string? StudentResidentialAdd { get; set; }

        [Required(ErrorMessage = "Pin code is required")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Invalid Pin Code")]
        public string? StuResidentialAddPinCode { get; set; }

        public string? StudentPermanentAdd { get; set; } = string.Empty;

        public string? StuPermanentAddPinCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Father First Name is required")]
        public string? FatherFName { get; set; }

        public string? FatherMName { get; set; } = string.Empty;

        public string? FatherLName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Father Occupation is required")]
        public string? FatherOccupation { get; set; }

        [Required(ErrorMessage = "Father Qualification is required")]
        public string? FatherQualification { get; set; }

        [Required(ErrorMessage = "Father Mobile is required")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid Mobile Number")]
        public string? FatherMobile { get; set; }

        [Required(ErrorMessage = "Father Email is required")]
        [EmailAddress]
        public string? FatherEmailId { get; set; }

        [Required(ErrorMessage = "Mother First Name is required")]
        public string? MotherFName { get; set; }

        public string? MotherMName { get; set; } = string.Empty;

        public string? MotherLName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mother Occupation is required")]
        public string? MotherOccupation { get; set; }

        [Required(ErrorMessage = "Mother Qualification is required")]
        public string? MotherQualification { get; set; }

        [Required(ErrorMessage = "Mother Mobile is required")]
        [RegularExpression(@"^[6-9][0-9]{9}$", ErrorMessage = "Invalid Mobile Number")]
        public string? MotherMobile { get; set; }

        [Required(ErrorMessage = "Mother Email is required")]
        [EmailAddress]
        public string? MotherEmailId { get; set; }

        [Required(ErrorMessage = "Previous school board is required")]
        public bool IsCBSE { get; set; } = true;
        [Required(ErrorMessage = "Previous school board is required")]
        public string? IsCBSEDetails { get; set; } = "CBSE";

        public string? CompulsorySubject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subject 3 is required")]
        public string? Subject3 { get; set; }

        [Required(ErrorMessage = "Subject 4 is required")]
        public string? Subject4 { get; set; }

        [Required(ErrorMessage = "Subject 5 is required")]
        public string? Subject5 { get; set; }
        [Required(ErrorMessage = "Subject 6 is required")]
        public string? Subject6 { get; set; } = string.Empty;

        public decimal MathsMarks { get; set; }
        public decimal OptionalMarks { get; set; }
        public decimal SocialSciencemarks { get; set; }
        public decimal ScienceMarks { get; set; }
        public decimal EnglishMarks { get; set; }

        public decimal MathsT2Marks { get; set; }
        public decimal OptionalT2Marks { get; set; }
        public decimal SocialScienceT2marks { get; set; }
        public decimal ScienceT2Marks { get; set; }
        public decimal EnglishT2Marks { get; set; }

        public string? DocIds { get; set; } = string.Empty;

        [Required(ErrorMessage = "Previous school address is required")]
        public string? PreviousSchoolAddress { get; set; }
        //[Required(ErrorMessage = "Occupation is required")]
        public string? FatherOccupationOther { get; set; }
        [Required(ErrorMessage = "Annual Income is required")]
        public string? FatherAnnualIncome { get; set; } = string.Empty;
        //[Required(ErrorMessage = "Occupation is required")]
        public string? MotherOccupationOther { get; set; }

        [Required(ErrorMessage = "Annual Income is required")]
        public string? MotherAnnualIncome { get; set; }

        public string? MotherOtherQualification { get; set; } = string.Empty;

        public string? FatherOtherQualification { get; set; } = string.Empty;

        [Required(ErrorMessage = "Child Aadhar Number is required")]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "Invalid Aadhar Number")]
        public string? ChildAadharNo { get; set; }

        [Required(ErrorMessage = "Father Aadhar Number is required")]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "Invalid Aadhar Number")]
        public string? FatherAadharNo { get; set; }

        [Required(ErrorMessage = "Mother Aadhar Number is required")]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "Invalid Aadhar Number")]
        public string? MotherAadharNo { get; set; }
        //[Required(ErrorMessage = "Address is required")]
        public string? StudentAddress { get; set; } = string.Empty;

    }
    public class CommonClass
    {
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }
        public string? SessionName { get; set; }
        public long SessionId { get; set; }
        public bool IsSelected { get; set; }
        public string? CreatedBy { get; set; }
    }
    public class CancelRegistration: CommonClass
    {
        public string? RegistrationNo { get; set; }
        public string? AppStatus { get; set; }
    }
    public class FormateType : CommonClass
    {
        public string? formateType { get; set; }
        public int Mode { get; set; }
    }
    public class CommonDomainLarge: CommonClass
    {
        public bool isEmail { get; set; }
        public bool isSMS { get; set; }
        public bool isWhatsapp { get; set; }
        public int FormatTypeID { get; set; }
        public string? Name { get; set; }
        public string? ClassCode { get; set; }
        public string? EmailText { get; set; }
        public string? SMSText { get; set; }
        public string? WhatsappText { get; set; }
        public bool IsValid { get; set; }
        public string? Description { get; set; }
        public string? SMSTextOnPayment { get; set; }
        public string? WhatsappTextOnPayment { get; set; }
        public string? EmailTextOnPayment { get; set; }
        public string? RegFee { get; set; }
        public DateTime? StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; } = DateTime.Now;
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string MinAge { get; set; }
        public string MaxAge { get; set; }
    }

    public class UpdateRegistrationStatusModel
    {
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }
        public long SessionId { get; set; }
        public long RegistrationId { get; set; }

    }


    #region------------------- Dasboard Model And Collection -----------------
    public class DashboardModel
    {
        public int DashBoardId { get; set; }

        public string? DashBoard { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsValid { get; set; }

        public string? DisplayName { get; set; }
    }
    public class AdminDashboardModal
    {

        public int TotalStudent { get; set; }
        public int CurrentStudents { get; set; }
        public int PreviousStudent { get; set; }
        public int NewAdmissions { get; set; }
        public string? AdmissionPercentage { get; set; }
        public string? StudentPercentage { get; set; }
        public string? CollectionPercentage { get; set; }
        public string? OutstandingPercentage { get; set; }
        public string? OutstandingFeePoints { get; set; }
        public string? OutstandingMonthName { get; set; }
        public string? StudentMonthName { get; set; }
        public string? NewStudentMonthName { get; set; }
        public string? NewStudentPoints { get; set; }
        public string? StudentPoint { get; set; }
        public string? FeeCollecionPoints { get; set; }
        public string? FeeMonthName { get; set; }
        public decimal TodayFeeCollection { get; set; }
        public decimal OutstandingFees { get; set; }

    }

    public class FeeHeadCollectionDto
    {
        public int FeeHeadId { get; set; }
        public string? FeeHeadName { get; set; }
        public decimal CollectionAmount { get; set; }
        public string? Percentage { get; set; }
    }

    public class AdmissionDashboardModel
    {

        public int TotalEnquiry { get; set; }

        public int Application { get; set; }
        public int Registration { get; set; }


        public int TotalAdmission { get; set; }

        public string? EnquiryPercentage { get; set; }

        public string? ApplicationPercentage { get; set; }
        public string? RegistrationPercentage { get; set; }

        public string? AdmissionPercentage { get; set; }

    }

    #endregion

    public class BreadcrumbItem
    {
        public string Text { get; set; } = "";
        public string Url { get; set; } = "";
        public bool IsCurrent { get; set; } 
    }
    public class GenerationIdConfigurationModel
    {
        public int Sid { get; set; }

        public string GroupCode { get; set; } = string.Empty;

        public string BranchCode { get; set; } = string.Empty;

        [Display(Name = "BTC")]
        public int? BTCID { get; set; }

        [Display(Name = "Branch Code Required")]
        public bool BranchCodeRequired { get; set; }

        [Display(Name = "BTC Required")]
        public bool BTCRequired { get; set; }

        [Display(Name = "Session Required")]
        public bool SessionRequired { get; set; }

        [Display(Name = "Pattern For")]
        [StringLength(100, ErrorMessage = "Pattern For cannot exceed 100 characters.")]
        public string? PatternFor { get; set; }

        [Display(Name = "Keyword")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(100, ErrorMessage = "Keyword cannot exceed 100 characters.")]
        public string KeyWord { get; set; } = string.Empty;

        [Display(Name = "Prefix")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(20, ErrorMessage = "Prefix cannot exceed 20 characters.")]
        public string PreFix { get; set; } = string.Empty;

        [Display(Name = "Key Value")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [Range(1, int.MaxValue, ErrorMessage = "Key Value must be greater than 0.")]
        public int KeyValue { get; set; }

        [Display(Name = "Key Value Length")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [Range(1, 20, ErrorMessage = "Key Value Length must be between 1 and 20.")]
        public int KeyValueLength { get; set; }

        [Display(Name = "Reset Flag")]
        public bool ResetFlag { get; set; }

        [Display(Name = "Class Group")]
        [StringLength(100, ErrorMessage = "Class Group cannot exceed 100 characters.")]
        public string? ClassGroup { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsValid { get; set; } = true;

        [Display(Name = "Session")]
        public long? SessionId { get; set; }
    }

    public class KeyWordModel
    {
        public int Id { get; set; }
        public string? KeyWord { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class EnquiryDashboardModel
    {
        public int TotalEnquiry { get; set; }
        public int Application { get; set; }
        public int Registration { get; set; }
        public int TotalAdmission { get; set; }

        public int TodayEnquiry { get; set; }
        public int TodayFollowup { get; set; }
        public int TodayAdmission { get; set; }
        public int TotalConversionAdmission { get; set; }

        public string EnquiryPercentage { get; set; }
        public string ApplicationPercentage { get; set; }
        public string RegistrationPercentage { get; set; }
        public string AdmissionPercentage { get; set; }
    }
    public class SessionComparisonModel
    {
        public long SessionId { get; set; }

        public string SessionName { get; set; }

        public int TotalEnquiry { get; set; }

        public int TotalApplication { get; set; }

        public int TotalRegistration { get; set; }

        public int TotalAdmission { get; set; }
    }
    public class SourceReportModel
    {
        public string? SourceName { get; set; }

        public int TotalCount { get; set; }

        public string? Percentage { get; set; }
    }
    public class MonthWiseModel
    {
        public string MonthName { get; set; }
        public int MonthNo { get; set; }
        public int TotalEnquiry { get; set; }
        public int Registration { get; set; }
        public int PendingRegistration { get; set; }
    }
    public class ClassWiseModel
    {
        public string ClassCode { get; set; }
        public int TotalEnquiry { get; set; }
        public int Registration { get; set; }
        public int TotalAdmission { get; set; }
    }
    public class PipelineModel
    {
        public string? Stage { get; set; }
        public int TotalCount { get; set; }
        public int Percentage { get; set; }
    }
    public class RecentEnquiryModel
    {
        public long EnquiryId { get; set; }
        public string EnquiryNo { get; set; }
        public string StudentName { get; set; }
        public string ClassCode { get; set; }
        public string MobileNo { get; set; }
        public string SourceOfEnquiry { get; set; }
        public int EnquiryStatus { get; set; }
        public DateTime EnquiryDate { get; set; }
    }
    public class FollowupStatusModel
    {
        public int FollowupStatus { get; set; }
        public int TotalCount { get; set; }
    }
    public class RecentAdmissionModel
    {
        public long StudentId { get; set; }
        public string AdmissionNo { get; set; }
        public string StudentName { get; set; }
        public string ClassCode { get; set; }
        public DateTime AdmissionDate { get; set; }
    }
    public class TodayFollowupModel
    {
        public long FollowUpId { get; set; }
        public string EnquiryNo { get; set; }
        public string StudentName { get; set; }
        public DateTime FollowUpDate { get; set; }
        public DateTime? NextDate { get; set; }
        public int FollowupStatus { get; set; }
        public string FollowUpRemark { get; set; }
    }
    public class TodayAdmissionModel
    {
        public long StudentId { get; set; }
        public string AdmissionNo { get; set; }
        public string StudentName { get; set; }
        public string ClassCode { get; set; }
        public DateTime AdmissionDate { get; set; }
    }
    public class EnquiryDashboardResponse
    {
        public EnquiryDashboardModel Dashboard { get; set; }

        public List<SessionComparisonModel> SessionComparison { get; set; }

        public List<SourceReportModel> SourceReport { get; set; }
        public List<MonthWiseModel> MonthWise { get; set; }

        public List<ClassWiseModel> ClassWise { get; set; }

        public List<PipelineModel> Pipeline { get; set; }

        public List<RecentEnquiryModel> RecentEnquiries { get; set; }

        public List<RecentAdmissionModel> RecentAdmissions { get; set; }

        public List<FollowupStatusModel> FollowupStatus { get; set; }

        public List<TodayFollowupModel> TodayFollowups { get; set; }

        public List<TodayAdmissionModel> TodayAdmissions { get; set; }
        public FollowupDashboardModel FollowupDashboard { get; set; }
        public List<MonthWiseAdmissionModel> MonthWiseAdmission { get; set; }
        public EnquirySummaryModel EnquirySummary { get; set; }
        public List<ClassWiseAdmissionModel> ClassWiseAdmission { get; set; }
    }
    public class FollowupDashboardModel
    {
        public int TotalEnquiry { get; set; }
        public int TodayEnquiry { get; set; }
        public int TodayFollowup { get; set; }
        public int TodayConversion { get; set; }
        public int TotalConversionAdmission { get; set; }
        public int PendingFollowup { get; set; }
        public int CompletedFollowup { get; set; }
        public int NeverCall { get; set; }
        public int ReminderDue { get; set; }
    }
    public class MonthWiseAdmissionModel
    {
        public string MonthName { get; set; }
        public int MonthNo { get; set; }
        public int TotalAdmission { get; set; }
    }
    public class ClassWiseAdmissionModel
    {
        public string ClassCode { get; set; }
        public int TotalAdmission { get; set; }
    }
    public class EnquirySummaryModel
    {
        public int TotalEnquiry { get; set; }
        public int TodayEnquiry { get; set; }
        public int TotalApplication { get; set; }
        public int TotalRegistration { get; set; }
        public int TotalAdmission { get; set; }
        public int TodayAdmission { get; set; }
        public int TodayFollowup { get; set; }
        public int PendingRegistration { get; set; }
        public int PendingAdmission { get; set; }
    }
    public class EnquiryDashboardSearchRequest
    {
        public string GroupCode { get; set; } = string.Empty;

        public string BranchCode { get; set; } = string.Empty;

        public long SessionId { get; set; }

        public string? ClassCode { get; set; }

        public string? Source { get; set; }

        public string? Month { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
    public class PipelineStageDto
    {
        public string Stage { get; set; }
        public int TotalCount { get; set; }
        public string Percentage { get; set; }
    }

    public class AdminDashboardResponse
    {
        public AdminDashboardModal Dashboard { get; set; } = new();
        public List<PipelineStageDto> Pipeline { get; set; } = new();
        public List<FeeHeadCollectionDto> FeeHeadCollection { get; set; } = new();
    }
}
