using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using DomainModel.Resources;
using DomainModel.Resources.Resource;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DomainModel.SchoolMaster
{
    #region  -------------------------- Class Master -------------
    public class ClassModel : CommonBaseModel
    {
        public int ClassId { get; set; }

        [Display(Name = "Class code")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(100, ErrorMessage = "ClassCode Name cannot exceed 100 characters")]
        public string ClassCode { get; set; } = string.Empty;
        [Display(Name = "Class name")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "StringLengthExceeded")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Class Name can contain only letters and spaces")]
        public string ClassName { get; set; } = string.Empty;
        public int ClassOrder { get; set; }
    }
    #endregion

    #region  -------------------------- Class Code Master -------------
    public class ClassCodeModel : CommonBaseModel
    {
        public int ClassId { get; set; }

        [Display(Name = "Class code")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string ClassCode { get; set; } = string.Empty;
    }
    #endregion

    #region  -------------------------- Class Schedule Master -------------
    public class ClassSchedule : CommonBaseModel
    {
        public int Sid { get; set; }
        public string? ClassCode { get; set; }
        public string? OffDayName { get; set; }

        public int? OffDayValue { get; set; }


        public string? HolidayType { get; set; }

        public DateTime? OffClassDate { get; set; }
    }
    #endregion

    #region  -------------------------- Class Code Master -------------
    public class ClassSubjectModel : CommonBaseModel
    {
        public int MapId { get; set; }
        public int SemesterId { get; set; }
        [Display(Name = "Class code")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "StringLengthExceeded")]
        public string? ClassCode { get; set; }

        [Display(Name = "Subject code")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "StringLengthExceeded")]

        public string? SubjectCode { get; set; }


        public bool IsPracticalSubject { get; set; }

        public bool IsOptionalSubject { get; set; }

        public bool IsScholasticSubject { get; set; }

        public bool IsReportCardSubject { get; set; }

        public bool IsCalculatedSubject { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsLanguage { get; set; }
    }
    #endregion

    #region  -------------------------- Country Master -------------
    public class CountryModel : CommonBaseModel
    {
        public int CountryId { get; set; }

        public int CountryCode { get; set; }
        [Display(Name = "Country name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Country Name can contain only letters and spaces")]
        public string CountryName { get; set; } = string.Empty;


        public string? Language { get; set; }

        public string? Nationality { get; set; }

        public bool IsDefaultCountry { get; set; }


    }
    #endregion

    #region  -------------------------- State Master -------------
    public class StateModel : CommonBaseModel
    {
        public int StateId { get; set; }


        [Display(Name = "State name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]

        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "StringLengthExceeded")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "State Name can contain only letters and spaces")]
        public string? StateName { get; set; }
        [Required(ErrorMessage = "Please select Country")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Country")]

        public int CountryId { get; set; }

    }
    #endregion

    #region  -------------------------- District Master -------------
    public class DistrictModel : CommonBaseModel
    {
        public int DistrictId { get; set; }

        [Display(Name = "District name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "District Name can contain only letters and spaces")]
        public string? DistrictName { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid State")]
        public int StateId { get; set; }
    }
    #endregion

    #region  -------------------------- Department Master -------------
    public class DepartmentModel : CommonBaseModel
    {
        public int DepartmentId { get; set; }


        [Required(ErrorMessage = "Department Code required")]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Department Code can contain only uppercase letters and numbers")]
        public string? DepartmentCode { get; set; }

        [Required(ErrorMessage = "Department name required")]
        [RegularExpression(@"^[a-zA-Z\s]+$",
            ErrorMessage = "Department Name can contain only letters and spaces")]
        public string? DepartmentName { get; set; }


    }
    #endregion

    #region  -------------------------- Designation Master -------------
    public class DesignationModel : CommonBaseModel
    {
        public int DesignationId { get; set; }
        [Display(Name = "Designation name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]

        [RegularExpression(@"^[a-zA-Z\s]+$",
            ErrorMessage = "Designation Name can contain only letters and spaces")]
        public string? DesignationName { get; set; }

    }
    #endregion

    #region  -------------------------- Allowed IP Master -------------
    public class AllowedIPModel : CommonBaseModel

    {

        public int AllowedIPId { get; set; }



        public Guid UserId { get; set; }

        public int ModuleId { get; set; }

        public string Ip { get; set; } = string.Empty;

        public DateTime ValidUpto { get; set; }



    }
    #endregion

    #region  -------------------------- Religion Master -------------
    public class ReligionMaster : CommonBaseModel
    {
        public int ReligionId { get; set; }

        [Display(Name = "Religion name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "StringLengthExceeded")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Only letters are allowed")]
        public string ReligionName { get; set; } = string.Empty;

    }
    #endregion

    #region  -------------------------- Subject Master -------------
    public class SubjectModel : CommonBaseModel
    {

        public int SubjectId { get; set; }




        [Display(Name = "Subject name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "StringLengthExceeded")]
        public string SubjectName { get; set; } = string.Empty;
        [Range(1, int.MaxValue, ErrorMessageResourceName = "Please select a department")]
        public int DepartmentId { get; set; }
        [Display(Name = "Subject code")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "StringLengthExceeded")]
        public string SubjectCode { get; set; } = string.Empty;

        [Display(Name = "UGC code")]
        [StringLength(20, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "StringLengthExceeded")]
        public string? UGCCode { get; set; }

        [Display(Name = "Subject Abbreviation")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField1")]
        public string? Abbreviation { get; set; }

        public int DisplayOrder { get; set; }

        public int Credit { get; set; }


    }
    #endregion

    #region  -------------------------- Subject Code Master -------------
    public class SubjectCodeMaster : CommonBaseModel
    {

        public int Sid { get; set; }
        [Display(Name = "Subject code")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "StringLengthExceeded")]
        public string SubjectCode { get; set; } = string.Empty;


    }
    #endregion

    #region  -------------------------- Group Master -------------
    public class GroupMaster : CommonBaseModel
    {
        public int GroupId { get; set; } = 0;
        [Display(Name = "Group Name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "StringLengthExceeded")]
        public string GroupName { get; set; } = string.Empty;
        public IFormFile? Logo { get; set; }

        public string? LogoPath { get; set; }

        [Display(Name = "Contact person")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "StringLengthExceeded")]
        public string? ContactPerson { get; set; }


        [Display(Name = "Contact number")]
        [StringLength(25, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "StringLengthExceeded")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Enter a valid 10-digit contact number")]
        public string? ContactNo { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]


        [Display(Name = "Contact email adress")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "StringLengthExceeded")]
        public string? ContactEmailId { get; set; }

        [Display(Name = "Contact persion imagepath")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "StringLengthExceeded")]
        public string? ContactPersonImagePath { get; set; }


        [Display(Name = "Web site")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "StringLengthExceeded")]
        public string? WebSite { get; set; }

        [EmailAddress(ErrorMessage = "Please enter a valid EmailId")]

        public string? EmailId { get; set; }
        [Display(Name = "Address line1")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "StringLengthExceeded")]
        public string? AddressLine1 { get; set; }


        [Display(Name = "Address line2")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "StringLengthExceeded")]

        public string? AddressLine2 { get; set; }

        public int DistrictId { get; set; }

        public int StateId { get; set; }

        public int CountryId { get; set; }


        [Display(Name = "Pincode")]
        [StringLength(20, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "StringLengthExceeded")]

        public string? PinCode { get; set; }


        public int ResellerId { get; set; }

        [RegularExpression(@"^[A-Za-z ]+$", ErrorMessage = "LoginName must contain only letters and spaces.")]

        public string LoginName { get; set; } = string.Empty;

    }
    #endregion

    #region  -------------------------- Occupation Master -------------
    public class OccupationModal : CommonBaseModel
    {
        public int OccupationId { get; set; }
        [Display(Name = "Occupation name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? OccupationName { get; set; }
        [Display(Name = "Occupation type")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? Type { get; set; }

    }
    #endregion

    #region  -------------------------- UserRight Master -------------
    public class UserRightModal : CommonBaseModel
    {
        public int URSID { get; set; }
        [Display(Name = "User name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "StringLengthExceeded")]
        public string UserName { get; set; } = string.Empty;
        [Range(1, int.MaxValue, ErrorMessage = "Please select a  role")]
        public int RoleId { get; set; }

    }
    #endregion

    #region  -------------------------- Qualification Master -------------
    public class Qualification : CommonBaseModel
    {
        public int QualificationId { get; set; }
        [Display(Name = "Qualification name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? QualificationName { get; set; }
        public string? QualificationTypeId { get; set; }
    }
    #endregion

    #region  -------------------------- Gender Master -------------
    public class GenderModal : CommonBaseModel
    {
        public int Sid { get; set; }
        [Display(Name = "Gender name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]

        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Gender Name can contain only letters and spaces")]
        public string? GenderName { get; set; }
        public int DisplayOrder { get; set; }
    }
    #endregion

    #region  -------------------------- HoliDay Master -------------
    public class HolidayModal : CommonBaseModel
    {
        public int HolidayId { get; set; }


        [Display(Name = "Holiday name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? HolidayName { get; set; }
        public DateTime HolidayDate { get; set; }
        public DateTime HolidayEndDate { get; set; }

        [RegularExpression("^[YN]$", ErrorMessage = "AppliedOn must be Y or N")]
        public string? AppliedOn { get; set; }
        [Display(Name = "Holiday type")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? HolidayType { get; set; }
    }
    #endregion

    #region  -------------------------- Commom Property -------------
    public abstract class CommonBaseModel
    {
        public bool IsValid { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Today;

        [StringLength(50)]
        public string? CreatedBy { get; set; }
        public long SessionId { get; set; }

        [StringLength(200)]
        public string? Remarks { get; set; }

        [RegularExpression(@"^[A-Z0-9]+$",
            ErrorMessage = "Group Code can contain only uppercase letters and numbers")]
        public string? GroupCode { get; set; }

        [RegularExpression(@"^[A-Z0-9]+$",
            ErrorMessage = "Branch Code can contain only uppercase letters and numbers")]
        public string? BranchCode { get; set; }

        public string? SessionName { get; set; }
    }
    #endregion

    #region  -------------------------- Message Model -------------
    public class MessageTypeModal
    {
        public int MessageTypeId { get; set; }
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }
        public string? MessageType { get; set; }
        public bool IsValid { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
    public class SmsEmailTextModel
    {
        public int SmsEmailTextId { get; set; }


        [Display(Name = "Group code")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string GroupCode { get; set; } = string.Empty;

        [Display(Name = "Branch code")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string BranchCode { get; set; } = string.Empty;


        [Display(Name = "Message type id ")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public int MessageTypeId { get; set; }

        [Display(Name = "Text type")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public int TextType { get; set; }

        [Display(Name = "SMS email text")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string SMSEmailText { get; set; } = string.Empty;

        public bool IsValid { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        public string WhatsAppTemplateName { get; set; } = string.Empty;
    }
    #endregion

    #region  -------------------------- Book Marks Model -------------
    public class BookMarkModel
    {
        public int BookMarkId { get; set; }
        //[StringLength(50, ErrorMessage = "Bookmark caption cannot exceed 50 characters")]
        public string BookMarkCaption { get; set; } = string.Empty;


        // [Url(ErrorMessage = "Please enter a valid URL")]
        public string Url { get; set; } = string.Empty;

        // [StringLength(200, ErrorMessage = "Icon path cannot exceed 200 characters")]
        public string? Icon { get; set; } = string.Empty;
        public bool IsValid { get; set; }

        // [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // [StringLength(50)]
        public string CreateBy { get; set; } = string.Empty;
    }
    #endregion

    #region ---------Class Section----

    public class ClassSectionModal
    {

        public int SectionId { get; set; }
        public string? SectionName { get; set; }
    }

    #endregion

    #region ---------Class Category----

    public class CategoryModal
    {

        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }

    }

    #endregion

    #region ---------Class Distance ----

    public class MstDistance
    {
        public int DistanceId { get; set; }
        public string? DistanceName { get; set; }

    }

    #endregion

    #region ---------Class Mother Details ----
    public class MstMotherTongue
    {
        public int TongueId { get; set; }
        public string? TongueName { get; set; }
    }

    public class MstVisaType
    {
        public int VisaTypeId { get; set; }
        public string? VisaTypeName { get; set; }
    }
    public class MstPassportTypeName
    {
        public int PassportTypeID { get; set; }
        public string? PassportTypeName { get; set; }
    }

    #endregion

    public class MotherTongue
    {
        public int TongueId { get; set; }
        public string? TongueName { get; set; }
    }

    public class VisaType
    {
        public int VisaTypeId { get; set; }
        public string? VisaTypeName { get; set; }
    }
    public class PassportName
    {
        public int PassportTypeID { get; set; }
        public string? PassportTypeName { get; set; }
    }
    public class BranchNameMst
    {
        public int BranchId { get; set; }
        public string? BranchName { get; set; }

        public string? BranchCode { get; set; }
    }
    public class DetStream : CommonBaseModel
    {
        public string? StreamCode { get; set; }
        public string? SubjectCode { get; set; }
        public string? SubjectName { get; set; }
    }

    public class ShareDomain
    {
        public string? ValueField { get; set; }
        public string? TextField { get; set; }
    }

    public class ElectiveSubjects
    {
        public string SubjectName { get; set; } = "";
        public string GroupId { get; set; } = "";
    }
    public class SectionModel : MNGTCommon
    {
        public int SectionId { get; set; }
        public string? SectionName { get; set; }
    }
    public class RTECategoryModel : CommonBaseModel
    {
        public int CategoryId { get; set; }

        [Display(Name = "Category Name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(100, ErrorMessage = "Category Name cannot exceed 100 characters.")]
        public string CategoryName { get; set; } = string.Empty;

        [Display(Name = "Display Order")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [Range(1, int.MaxValue, ErrorMessage = "Display Order must be greater than 0.")]
        public int DisplayOrder { get; set; }
    }
    public class TongueModel
    {
        public int TongueId { get; set; }

        [Display(Name = "Tongue Name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Tongue Name can contain only letters and spaces.")]
        [StringLength(100, ErrorMessage = "Tongue Name cannot exceed 100 characters.")]
        public string TongueName { get; set; } = string.Empty;

        [Display(Name = "Remarks")]
        [StringLength(250, ErrorMessage = "Remarks cannot exceed 250 characters.")]
        public string? Remarks { get; set; }

        public bool IsValid { get; set; } = true;

        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Display Order")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [Range(1, int.MaxValue, ErrorMessage = "Display Order must be greater than 0.")]
        public int DisplayOrder { get; set; }
    }
    public class StudentCategoryModel
    {
        public int CategoryId { get; set; }

        public string GroupCode { get; set; } = string.Empty;

        public string BranchCode { get; set; } = string.Empty;

        [Display(Name = "Category Name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Category Name can contain only letters and spaces.")]
        [StringLength(100, ErrorMessage = "Category Name cannot exceed 100 characters.")]
        public string CategoryName { get; set; } = string.Empty;

        [Display(Name = "EWS Category")]
        public bool IsEWS { get; set; }

        public bool IsValid { get; set; } = true;

        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
    public class SocialCategoryModel
    {
        public int CategoryId { get; set; }

        [Display(Name = "Category Name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Category Name can contain only letters and spaces.")]
        [StringLength(100, ErrorMessage = "Category Name cannot exceed 100 characters.")]
        public string CategoryName { get; set; } = string.Empty;

        [Display(Name = "Remarks")]
        [StringLength(250, ErrorMessage = "Remarks cannot exceed 250 characters.")]
        public string? Remarks { get; set; }

        public bool IsValid { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string CreatedBy { get; set; } = string.Empty;
    }
    public class DisciplineModel
    {
        public int DisciplineId { get; set; }

        //[Display(Name = "Level")]
        //[Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        //[Range(1, int.MaxValue, ErrorMessage = "Please select a valid Level.")]
        public int LevelId { get; set; }

        public string GroupCode { get; set; } = string.Empty;

        public string BranchCode { get; set; } = string.Empty;

        [Display(Name = "Discipline Name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Discipline Name can contain only letters and spaces.")]
        [StringLength(100, ErrorMessage = "Discipline Name cannot exceed 100 characters.")]
        public string DisciplineName { get; set; } = string.Empty;

        [Display(Name = "Remarks")]
        [StringLength(250, ErrorMessage = "Remarks cannot exceed 250 characters.")]
        public string? Remarks { get; set; }

        public bool IsValid { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string CreatedBy { get; set; } = string.Empty;
    }

    public class DocumentModel
    {
        public int DocId { get; set; }

        public string GroupCode { get; set; } = string.Empty;

        public string BranchCode { get; set; } = string.Empty;

        [Display(Name = "Document Name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        //[RegularExpression(@"^[a-zA-Z0-9\s\-_().]+$", ErrorMessage = "Document Name contains invalid characters.")]
        [StringLength(100, ErrorMessage = "Document Name cannot exceed 100 characters.")]
        public string DocumentName { get; set; } = string.Empty;

        [Display(Name = "Remarks")]
        [StringLength(250, ErrorMessage = "Remarks cannot exceed 250 characters.")]
        public string? Remarks { get; set; }

        [Display(Name = "Document Type")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(50, ErrorMessage = "Document Type cannot exceed 50 characters.")]
        public string DocumentType { get; set; } = string.Empty;

        [Display(Name = "Display Order")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [Range(1, int.MaxValue, ErrorMessage = "Display Order must be greater than 0.")]
        public int DisplayOrder { get; set; }

        public bool IsValid { get; set; } = true;

        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    #region  -------------------------- Disability Type Master -------------

    public class DisabilityTypeModel : CommonBaseModel
    {

        public int SeedId { get; set; }

        [Display(Name = "Disability type name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "StringLengthExceeded")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Only letters are allowed")]
        public string DisabilityType { get; set; } = string.Empty;
        [Display(Name = "Display Order")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [Range(1, int.MaxValue, ErrorMessage = "Display Order must be greater than 0.")]
        public int DisplayOrder { get; set; }


    }

    #endregion


    public class BranchModel
    {
        public int BranchId { get; set; }
        [Display(Name = "Group Code")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string GroupCode { get; set; } = string.Empty;
        [Display(Name = "Branch Code")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string BranchCode { get; set; } = string.Empty;

        [Display(Name = "Branch Name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(150, ErrorMessage = "Branch Name cannot exceed 150 characters.")]
        public string BranchName { get; set; } = string.Empty;

        [Display(Name = "Remarks")]
        [StringLength(250, ErrorMessage = "Remarks cannot exceed 250 characters.")]
        public string? Remarks { get; set; }

        [Display(Name = "Contact Person")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(100, ErrorMessage = "Contact Person cannot exceed 100 characters.")]
        public string ContactPerson { get; set; } = string.Empty;

        [Display(Name = "Contact Number")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [Phone(ErrorMessage = "Please enter a valid contact number.")]
        [StringLength(15, ErrorMessage = "Contact Number cannot exceed 15 characters.")]
        public string ContactNo { get; set; } = string.Empty;

        [Display(Name = "Contact Email")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(100, ErrorMessage = "Contact Email cannot exceed 100 characters.")]
        public string? ContactEmailId { get; set; }

        public string? LogoPath { get; set; }

        public string? ContactPersonImagePath { get; set; }

        [Display(Name = "Website")]
        [StringLength(200, ErrorMessage = "Website cannot exceed 200 characters.")]
        public string? WebSite { get; set; }

        [Display(Name = "Branch Email")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(100, ErrorMessage = "Branch Email cannot exceed 100 characters.")]
        public string? BranchEmailId { get; set; }

        [Display(Name = "Affiliation Details")]
        [StringLength(200, ErrorMessage = "Affiliation Details cannot exceed 200 characters.")]
        public string? AffiliationDetails { get; set; }

        [Display(Name = "School Number")]
        [StringLength(50, ErrorMessage = "School Number cannot exceed 50 characters.")]
        public string? SchoolNo { get; set; }

        [Display(Name = "Address Line 1")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(250, ErrorMessage = "Address Line 1 cannot exceed 250 characters.")]
        public string AddressLine1 { get; set; } = string.Empty;

        [Display(Name = "Address Line 2")]
        [StringLength(250, ErrorMessage = "Address Line 2 cannot exceed 250 characters.")]
        public string? AddressLine2 { get; set; }

        [Display(Name = "District")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [Range(1, int.MaxValue, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]

        public int DistrictId { get; set; }

        [Display(Name = "State")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [Range(1, int.MaxValue, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]

        public int StateId { get; set; }

        [Display(Name = "Country")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [Range(1, int.MaxValue, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]

        public int CountryId { get; set; }

        [Display(Name = "Pin Code")]
        [StringLength(10, ErrorMessage = "Pin Code cannot exceed 10 characters.")]
        public string? PinCode { get; set; }

        [Display(Name = "Start Time")]
        public TimeSpan? StartTime { get; set; }

        [Display(Name = "End Time")]
        public TimeSpan? EndTime { get; set; }

        [Display(Name = "Head Office")]
        public bool IsHO { get; set; }

        public bool IsValid { get; set; } = true;

        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string? LogoPathForPrint { get; set; }

        [Display(Name = "Branch Category")]
        [StringLength(100, ErrorMessage = "Branch Category cannot exceed 100 characters.")]
        public string? BranchCategory { get; set; }

        [Display(Name = "Affiliation Upto")]
        public DateTime? AffiliationUpto { get; set; }

        [Display(Name = "Status of School")]
        [StringLength(100, ErrorMessage = "Status of School cannot exceed 100 characters.")]
        public string? StatusOfSchool { get; set; }

        [Display(Name = "UDISE No")]
        [StringLength(20, ErrorMessage = "UDISE No cannot exceed 20 characters.")]
        public string? UDISENo { get; set; }
    }
    public class SourceOfEnquiryModel
    {
        public int SourceId { get; set; }
        [Display(Name = "Source name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string SourceName { get; set; } = string.Empty;

        public bool IsValid { get; set; }

        public DateTime CreatedDate { get; set; }

        public string? CreatedBy { get; set; }
    }
    public class StudentModel
    {
        public int StudentId { get; set; } = 0;

        [Required(ErrorMessage = "Student Name is required")]
        [StringLength(200, ErrorMessage = "Student Name cannot exceed 200 characters")]
        public string StudentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Email Address is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string EmailAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Country is required")]
        public int CountryId { get; set; }

        [Required(ErrorMessage = "State is required")]
        [Range(1, int.MaxValue, ErrorMessage = "State is required")]
        public int StateId { get; set; }

        [Required(ErrorMessage = "City is required")]
        [Range(1, int.MaxValue, ErrorMessage = "City is required")]
        public int CityId { get; set; }

        [Required(ErrorMessage = "Display Order is required")]
        public int DisplayOrder { get; set; }

        // Optional - JPG/PNG up to 2MB, set after calling UploadPhoto
        public string? PhotoPath { get; set; }
        public string? StudentPhoto { get; set; }

        public bool IsActive { get; set; } = true;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
    public class InteractionPanelModel
    {
        public int PID { get; set; }

        public string GroupCode { get; set; } = string.Empty;

        public string BranchCode { get; set; } = string.Empty;

        [Display(Name = "Session")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [Range(1, long.MaxValue, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public long SessionId { get; set; }

        [Display(Name = "Panel Name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(100, ErrorMessage = "Panel Name cannot exceed 100 characters.")]
        public string PanelName { get; set; } = string.Empty;

        [Display(Name = "Remarks")]
        [StringLength(250, ErrorMessage = "Remarks cannot exceed 250 characters.")]
        public string? Remarks { get; set; }

        public bool IsValid { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string CreatedBy { get; set; } = string.Empty;
    }
    public class ClassDocumentModel
    {
        public int DDocId { get; set; }

        public string GroupCode { get; set; } = string.Empty;

        public string BranchCode { get; set; } = string.Empty;

        [Display(Name = "Document")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Document.")]
        public int DocumentId { get; set; }
        public string? DocumentName{ get; set; }
        public string?  DocumentType { get; set; }
        public string?  ClassName { get; set; }

        [Display(Name = "Class")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string ClassCode { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Mandatory")]
        public bool IsMandatory { get; set; }  
    }
    public class UpdateClassDocumentRequest
    {
        public string GroupCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public long DocumentId { get; set; }
        public string ClassCode { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }
        public int TransType { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}

