using DomainModel.Admin;
using DomainModel.Resources.Resource;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DomainModel.FinanceMNGT
{
    #region ------------------------Finance Common Model-------------- 
    public class MNGTCommon
    {
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }
        public string? SessionName { get; set; }
        public long SessionId { get; set; }
        public long StudentId { get; set; }
        public bool IsDisabled { get; set; }
        public int MonthIndex { get; set; }
        public bool IsValid { get; set; } = true;
        public bool IsSelected { get; set; }
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Today;
        public byte[]? Image { get; set; }
    }

    #endregion

    #region----------- Search Any Model--------------
    public class SearchAnyRequestModel
    {
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }
        public string? SessionName { get; set; }
        public string? RequestName { get; set; }
        public string? RequestName2 { get; set; }
        public int RequestId { get; set; }
        public long SessionId { get; set; }
        public long StudentId { get; set; }
        public bool IsActive { get; set; } = false;

    }
    public class StudentTransportDetails : MNGTCommon
    {
        public string? ControlNo { get; set; }
        public string? StudentName { get; set; }
        public string? StudentNo { get; set; }
        public string? PermanentAddress { get; set; }
        public string? FatherContactNo { get; set; }
        public int? BoardingPointId { get; set; }
        public int? RouteId { get; set; }
        public string? RouteName { get; set; }
        public string? PointName { get; set; }
        public int? DropRouteId { get; set; }
        public int? DropPointId { get; set; }
        public string? DropRouteName { get; set; }
        public string? DropPointName { get; set; }
        // public string? SessionName { get; set; }
        public string? ClassCode { get; set; }
    }
    #endregion

    #region ------------------------Bank Model-------------- 
    public class BankModel : MNGTCommon
    {
        [Key]
        public int BankId { get; set; }
        [Display(Name = "Bank name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Bank name can contain only letters and spaces")]
        public string BankName { get; set; } = string.Empty;

        [Display(Name = "Branch name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Bank name can contain only letters and spaces")]
        [StringLength(100)]
        public string BranchName { get; set; } = string.Empty;

        [Display(Name = "Bank address")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(250)]
        public string BankAddress { get; set; } = string.Empty;
    }

    #endregion

    #region ------------------------Cheque Book Model-------------- 
    public class ChequeBookModel : MNGTCommon
    {
        [Key]
        public int CheqBookId { get; set; }

        [Display(Name = "Cheque title")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Cheque title can contain only letters and spaces")]
        public string? CheqTitle { get; set; }
        [Display(Name = "Bank Name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public int BankId { get; set; }
        public int AccountId { get; set; }
        public string? FirstLeafNo { get; set; } = string.Empty;
        public int TotalLeaf { get; set; }
        public byte BookStatus { get; set; }
    }

    #endregion

    #region ------------------------Cheque Type Model-------------- 
    public class ChequeTypeModel : MNGTCommon
    {
        public int Sid { get; set; }
        [Display(Name = "Cheq type")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Cheq type can contain only letters and spaces")]
        public string? ChequeTypeName { get; set; }
        public int DisplayOrder { get; set; }
    }

    #endregion

    #region ------------------------ Bank Account Model-------------- 
    public class BankAccountModel : MNGTCommon
    {
        public int DetBankAcId { get; set; }
        [Display(Name = "Bank Name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public int BankId { get; set; }
        public int SocietyId { get; set; }
        [Display(Name = "Account Number")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression("^[0-9]{5,20}$", ErrorMessage = "Account no numeric. or between 5 and 20 characters.")]

        public string? AccountNo { get; set; }
        public string? DisplayName { get; set; }
        public string? AccountType { get; set; }
        public bool Freeze { get; set; }
        public bool ForFee { get; set; }
        public bool ForInventory { get; set; }
        public bool ForSalary { get; set; }
        public bool ForOthers { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal CommitedAmount { get; set; }
        public string? Signatory1 { get; set; }
        public bool IsSig1Mandatory { get; set; }
        public string? Signatory2 { get; set; }
        public bool IsSig2Mandatory { get; set; }
        public string? Signatory3 { get; set; }
        public bool IsSig3Mandatory { get; set; }
        public string? Signatory4 { get; set; }
        public bool IsSig4Mandatory { get; set; }
        public string? Signatory5 { get; set; }
        public bool IsSig5Mandatory { get; set; }
        public DateTime? LastTransDate { get; set; }
        public int LedgerId { get; set; }
        public string? ReturnValue { get; set; }
    }
    #endregion

    #region ------------------------  Account Model-------------- 
    public class AccountNoModel : MNGTCommon
    {
        public int AccountId { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]{9,18}$",
         ErrorMessage = "Account No  must be between 9 and 18 digits.")]
        public string? AccountNo { get; set; }

        [Required]
        [StringLength(50)]
        public string? AccountDescription { get; set; }

        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }

    }
    public class PaymentModel : MNGTCommon
    {
        public int PMId { get; set; }
        [Display(Name = "Payment mode name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Payment mode name can contain only letters")]
        public string? ModeName { get; set; }
        [Display(Name = "Mode Abbreviation")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField1")]
        public string? ModeAbbr { get; set; }
        [Display(Name = "Display Order")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [Range(1, int.MaxValue, ErrorMessage = "Distance order must be greater than 0.")]
        public int? DisplayOrder { get; set; }
    }
    #endregion

    #region ------------------------ Month Fee Head And Socity Model-------------- 
    public class MonthModel : MNGTCommon
    {
        public int Sid { get; set; }

        [Display(Name = "Month No")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [Range(1, 12, ErrorMessage = "Month No must be between 1 and 12.")]
        public int MonthNo { get; set; }
        [Display(Name = "Month Name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Month Name can contain only letters and spaces")]
        public string? MonthName { get; set; }
        [Display(Name = "Display Order")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]

        [Range(1, 12, ErrorMessage = "DisplayOrder must be between 1 and 12")]
        public int? DisplayOrder { get; set; }
    }
    public class FeeHeadModel : MNGTCommon
    {
        [StringLength(50)]
        [Display(Name = "Fee Head name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField1")]
        public string? FeeHeadName { get; set; }
        public int FeeHeadId { get; set; }
        [StringLength(50)]
        [Display(Name = "Head Abbreviation ")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField1")]
        public string? FeeHeadAbbr { get; set; }
        [StringLength(50)]
        [Display(Name = "Head type ")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? FeeHeadType { get; set; }
        [StringLength(20)]
        [Display(Name = "Applicable type ")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? FeeApplicableType { get; set; }
        public int DisplayOrder { get; set; }
    }
    public class FeeCollectionPeriodConfig : MNGTCommon
    {
        public int PeriodId { get; set; }
        [StringLength(50)]
        public string? QuarterName { get; set; }
        //[StringLength(50)]
        [Display(Name = "Month Name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[0-9]$", ErrorMessage = "Month No can contain only numbric.")]
        public int NoOfMonth { get; set; }
        [StringLength(50)]
        [Display(Name = "Period Name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? PeriodName { get; set; }
        public DateTime? DueFrom { get; set; } = DateTime.Today;
        public DateTime? DueTo { get; set; } = DateTime.Today;
        [Display(Name = "Due Date")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public DateTime? FeeDueDate { get; set; }
        public string? PeriodType { get; set; } = "Quarterly";
        public string? MonthNos { get; set; }


    }

    public class FeeHeadForAdmision : MNGTCommon
    {


    }
    public class SocietyModel : MNGTCommon
    {
        public int SocietyId { get; set; }
        [Range(0, 255)]
        public int IsTuitionFeeEditable { get; set; }
        [StringLength(50)]

        [Display(Name = "Society name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? SocietyName { get; set; }
        [StringLength(50)]
        [Display(Name = "Society code")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? SocietyCode { get; set; }
        [StringLength(50)]
        [Display(Name = "Society description")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? SocietyDesc { get; set; }
        [StringLength(150)]
        public string? AddressLine1 { get; set; }
        [StringLength(150)]
        public string? AddressLine2 { get; set; }
        [StringLength(10)]
        public string? BankClientCode { get; set; }
        [StringLength(50)]
        public string? OnlineGateway { get; set; }
        [StringLength(500)]
        public string? hashSequence { get; set; }
        [StringLength(100)]
        public string? URL { get; set; }
        [StringLength(50)]
        public string? SALT { get; set; }
        [StringLength(20)]
        public string? MerchantID { get; set; }
        [StringLength(50)]
        public string? EncryptKey { get; set; }
        [StringLength(100)]
        public string? VerifyURL { get; set; }
        [StringLength(500)]
        public string? SuccessURL { get; set; }
        [StringLength(100)]
        public string? TallyURL { get; set; }
        [StringLength(500)]
        public string? FailureURL { get; set; }
        [StringLength(500)]
        public string? CancelURL { get; set; }
        [Range(0, 9999999999999999.99)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal RegFee { get; set; }
        [StringLength(50)]
        public string? TallyCompany { get; set; }
        [StringLength(50)]
        public string? GatewayType { get; set; }
        [StringLength(500)]
        public string? GatewayTypeURL { get; set; }
        [StringLength(200)]
        public string? SettlementURL { get; set; }
        public bool IsSettlementProcess { get; set; }
        public bool BankProcessApl { get; set; }

    }



    #endregion

    #region ---------fee collection ---------

    public class FeeCollectionModel : MNGTCommon
    {
        public int Sid { get; set; }



        [Range(1, 12, ErrorMessage = "Please select a valid month.")]
        public int MonthNo { get; set; }
        [Display(Name = "Class name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? ClassCode { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a  fee head")]
        public int FeeHeadId { get; set; }

        [Display(Name = "Amount")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [Range(typeof(decimal), "1", "10000", ErrorMessage = "Please enter a valid amount.")]
        public decimal? Amount { get; set; }

        //[Display(Name = "FeeTemplateId")]
        //[Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        //[RegularExpression(@"^[0-9]$", ErrorMessage = "Fee Template Id can contain only numbric.")]
        public int FeeTemplateId { get; set; }
    }
    #endregion

    #region -------------Distsnce------

    public class DistanceModel : MNGTCommon
    {
        public int DistanceId { get; set; }

        [Display(Name = "Distance Name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? DistanceName { get; set; }

        [Display(Name = "Distance Order")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [Range(1, int.MaxValue, ErrorMessage = "Distance order must be greater than 0.")]
        public int? DistanceOrder { get; set; }

    }
    #endregion

    #region ----- class wise fee Head Mapped------------------------

    public class ClassFeeHeadMappedModel : MNGTCommon
    {
        public int ClassFeeId { get; set; }

        [Required]
        [StringLength(50)]
        public string? SessionName { get; set; }

        [Required]
        [StringLength(20)]
        public string? ClassCode { get; set; }

        [Required]
        public int FeeHeadId { get; set; }

        public bool IsStudentSpecific { get; set; }

        public bool IsEditable { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Invalid amount.")]
        public decimal Amount { get; set; }
        public decimal AmountForOld { get; set; }

        public bool IsClubForTutionCertificate { get; set; }

        public int FeeTemplateId { get; set; }




    }

    #endregion

    #region   ------FeeTemplate------
    public class FeeTemplateModel : MNGTCommon
    {
        public int FeeTemplateId { get; set; }

        [Display(Name = "Template Name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        //   [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Template Name can contain only letters")]
        public string? TemplateName { get; set; }

        [StringLength(150, ErrorMessage = "Description cannot exceed 150 characters.")]
        public string? Description { get; set; }
        [Display(Name = "Display Order")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [Range(1, int.MaxValue, ErrorMessage = "Display Order must be greater than 0")]
        public int? DisplayOrder { get; set; }
    }

    #endregion

    #region -----DetSessionModel-----------------

    public class DetSessionModel : MNGTCommon
    {
        public int Sid { get; set; }
        public string? Session { get; set; }
        [Display(Name = "Fee Head Name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select Fee Head")]
        public int FeeHeadId { get; set; }
        public string? FeeHeadName { get; set; }
        public string? FeeHeadType { get; set; }
    }

    #endregion

    #region-------------Fee Taking Method-----
    public class FeeHeadsOfTemplateModel : MNGTCommon
    {
        public int Id { get; set; }
        public int FeeTemplateID { get; set; }
        public int FeeHeadId { get; set; }

    }

    #endregion

    #region--------Maped Template-----------------------
    public class ClassFeeHeadsModel : MNGTCommon
    {
        public int ClassFeeId { get; set; }
        public int TemplateFeeId { get; set; }
        public int FeeHeadId { get; set; }
        public string? FeeHeadName { get; set; }
        public string? FeeHeadType { get; set; }
        public string? FeeApplicableType { get; set; }
        public decimal? Amount { get; set; }
        public string? DisplayOrder { get; set; }
        //  public bool IsValid { get; set; }
        public string? FeeTemplateID { get; set; }
        public string? TemplateName { get; set; }
        public string? TemplateDisplayOrder { get; set; }
        public string? Status { get; set; }
        //public string? CreatedBy { get; set; }   
    }
    public class FeeHeadTemplateRequest : MNGTCommon
    {
        public string? FeeHeadType { get; set; }
        public int FeeHeadId { get; set; }
        public int TemplateId { get; set; }
        public string? Status { get; set; } = "-1";
    }

    public class FeeHeadTemplatesListModel
    {
        public string? FeeHeadType { get; set; }
        public string? FeeTemplateID { get; set; }
        public string? TemplateName { get; set; }
        public string? DisplayOrder { get; set; }
        public decimal FeeHeadAmount { get; set; }
        public string? IsMapped { get; set; }
        public bool IsSelected { get; set; }
    }

    #endregion

    #region---------------Set Fee Taking Method---------------
    public class FeeTakingMethod : MNGTCommon
    {
        public int Sid { get; set; }
        public string? SessionName { get; set; }
        public string? MonthNo { get; set; }
        public int FeeTemplateID { get; set; }
        public List<JsonElement>? GridData { get; set; }

    }
    #endregion

    #region---------Map Fee Template With Class---------------
    public class ClassWiseFeeTemplateModel : MNGTCommon
    {
        public int SeedId { get; set; }
        [Display(Name = "Class Name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? ClassCode { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Please select Fee Template")]
        public int DefaultFeeTemplateId { get; set; }
        public string? ClassName { get; set; }
        public string? TemplateName { get; set; }

    }
    #endregion

    #region--------------Map Fee Head With Student----------------------------
    public class StudenmapheadModal : MNGTCommon
    {
        public int SId { get; set; }
        public string? Result { get; set; }

    }
    public class MapwithFeehead : MNGTCommon
    {
        public int FeeHeadId { get; set; }
        public string? ClassCode { get; set; }
        public int SectionId { get; set; }
        public string? Mode { get; set; } = "-1";
        public string? StudentStatus { get; set; } = "-1";
        public string? EWSCategory { get; set; } = "-1";

        public int Sid { get; set; }
        public string? StudentNo { get; set; }
        public string? StudentName { get; set; }
        public string? FeeHeadName { get; set; }
        public string? RollNo { get; set; }
        public string? ClassSection { get; set; }
        public string? ApplicableFrom { get; set; }
        public string? ControlNo { get; set; }
        public string? Gender { get; set; }
        public string? IsEWS { get; set; }
        public string? JoinType { get; set; }
    }
    public class UnMapFeeHead : MNGTCommon
    {
        public int FeeHeadId { get; set; }
        public string? StudentId { get; set; }

        public bool Status { get; set; }
        public string? Message { get; set; }
    }

    #endregion

    #region------------------Map Period With Student-----------------
    public class IMSWFTPeriodType : MNGTCommon
    {
        public int RollNo { get; set; }
        public string? ClassCode { get; set; }
        public string? ClassSection { get; set; }
        public string? SectionId { get; set; }
        public string? StudentFeeTemplateName { get; set; }
        public string? StudentName { get; set; }
        public string? StudentNo { get; set; }
        public string? PeriodType { get; set; }
        public string? TemplateId { get; set; }
        public string? ControlNo { get; set; }
        // public bool IsSelected { get; set; }

    }
    public class StudentFeePeriodTypeExportModel : MNGTCommon
    {
        public int SrNo { get; set; }
        public int RollNo { get; set; }
        public string? ClassCode { get; set; }
        public int Section { get; set; }
        public string? StudentName { get; set; }
        public string? StudentNo { get; set; }
        public string? PeriodType { get; set; }
    }
    public class ReportHeaderModel : MNGTCommon
    {
        public string? SocietyId { get; set; }
        public string? ReportName { get; set; }
        // Output Fields
        public string? FieldOne { get; set; }
        public string? FieldTwo { get; set; }
        public string? FieldThree { get; set; }
        public string? FieldFour { get; set; }
        public string? FieldFive { get; set; }
        public string? FieldSix { get; set; }
        public string? FieldSeven { get; set; }
        public string? FieldEight { get; set; }
        public string? FieldNine { get; set; }
        public string? FieldTen { get; set; }
        public string? FieldEleven { get; set; }
        public string? FieldTwelve { get; set; }
        public string? FieldThirteen { get; set; }
    }
    public class MapFeePeriodWithStudentModel : MNGTCommon
    {
        public string? PeriodType { get; set; }
        public string? StudentNo { get; set; }
        public string? TemplateId { get; set; }
    }
    public class PeriodMaster
    {
        public int PeriodTypeId { get; set; }
        public string? PeriodTypeName { get; set; } = "Quarterly";
    }

    #endregion

    #region---------- Transport Model ------------------------------------
    public class TransportSearchModel : MNGTCommon
    {
        public string? ClassCode { get; set; }
        public int SectionId { get; set; }
        public string? Gender { get; set; }
        public string? StudentNo { get; set; }
        public string? FirstName { get; set; }

        public int RouteDistanceId { get; set; } = 0;
        public int IsTransportSelected { get; set; } = 0;
    }
    public class TransportStudentDataModel : MNGTCommon
    {
        public string? StudentNo { get; set; }
        public string? ControlNo { get; set; }
        public string? RollNo { get; set; }
        public string? StudentName { get; set; }
        public string? Gender { get; set; }
        public string? SMSMobileNo { get; set; }
        public string? MotherContactNo { get; set; }
        public string? FatherContactNo { get; set; }
        public string? FatherName { get; set; }
        public decimal Amount { get; set; }
        public string? DOB { get; set; }
        public string? ClassCode { get; set; }
        public string? SectionId { get; set; }
        public string? CurrentAddress { get; set; }
        public string? ClassSection { get; set; }
        public string? SectionName { get; set; }
        public decimal? RouteDistance { get; set; }
        public string? DistanceName { get; set; }
        public int IsTransportRequired { get; set; }
        public string? TptStatus { get; set; }
        public DateTime TransportAppliedFrom { get; set; } = DateTime.Today;
        public string? RouteName { get; set; }
        public int? PickupRouteId { get; set; }
        public int? DropRouteId { get; set; }
        public string? DistanceId { get; set; }
        public int IsTransportSelected { get; set; }
        public DateTime TransportAppliedDate { get; set; } = DateTime.Today;
        public string? PeriodIds { get; set; }
        public string? PassengerId { get; set; }
        public int? RouteId { get; set; }
        public int BoardingPointId { get; set; }
        public int? DropPointId { get; set; }
        public int? BoardingPointNo { get; set; }
        public string? PassengerType { get; set; }
        public string? SelectedMonthNo { get; set; }

    }
    public class StudentTransportMappedModel : MNGTCommon
    {
        public int ItemNo { get; set; }
        public string? StudentNo { get; set; }
        public string? AdmissionNo { get; set; }
        public string? ClassCode { get; set; }
        public string? ClassName { get; set; }
        public string? FirstName { get; set; }
        public string? StudentName { get; set; }
        public string? DistanceName { get; set; }
        public int IsTransportRequired { get; set; }
        public string? TptStatus { get; set; }
        public DateTime? TransportAppliedFrom { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ControlNo { get; set; }
        public int MonthNo { get; set; }
    }
    public class TransportRoute
    {
        public int RouteId { get; set; }
        public string? RouteName { get; set; }
    }
    public class TransportRoutePoint
    {

        public int BordingPointId { get; set; }
        public string? PointName { get; set; }
    }

    public class TransportFeeConfig : MNGTCommon
    {

        public int Tid { get; set; }
        [Display(Name = "Amount")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [Range(typeof(decimal), "1", "10000", ErrorMessage = "Enter an amount between 1 and 10,000.")]
        public decimal? Amount { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a  distance")]
        public int DistanceId { get; set; }
        public string? DistanceName { get; set; }


    }

    public class TransportRequestModel : MNGTCommon
    {

        public int IsTrasportReq { get; set; }
        public string? StudentName { get; set; }
        public string? FatherName { get; set; }
        public string? ControlNo { get; set; }
        public string? ClassSection { get; set; }
        public string? DistanceName { get; set; }
        public string? CurrentAddress { get; set; }
        public string? UpdatedBy { get; set; }
        public int DistanceId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransportAppliedFrom { get; set; } = DateTime.Today;
        public string SelectedMonthNo { get; set; } = string.Empty;
        public string RouteId { get; set; } = string.Empty;
        public string BoardingPointId { get; set; } = string.Empty;
        public string PassengerType { get; set; } = string.Empty;
        public string DropRouteId { get; set; } = string.Empty;
        public string DropPointId { get; set; } = string.Empty;
    }
    #endregion

    #region---------------Enquiry ModelData---------------------
    public class EnquiryListResponse : CommonClass
    {
        public int EnquiryId { get; set; }
        public string? EnquiryNo { get; set; }
        [Display(Name = "Student Name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Student Name can contain only letters and spaces")]
        public string? StudentFirstName { get; set; }
        public string? StudentMiddleName { get; set; }
        public string? StudentLastName { get; set; }
        [Display(Name = "Father Name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Father Name can contain only letters and spaces")]
        public string? FatherName { get; set; }
        [Display(Name = "Mobile No.")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Mobile No can contain only numbric.")]
        public string? MobileNo { get; set; }
        [Display(Name = "Gender")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? Gender { get; set; }
        public DateTime DateOfBirth { get; set; } = DateTime.Today;
        //public DateTime DOB { get; set; } = DateTime.Today;
        public string? ClassName { get; set; }
        [Display(Name = "Enquiry Date")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public DateTime EnquiryDate { get; set; } = DateTime.Today;
        public int EnquiryStatus { get; set; }
        public int IsOnline { get; set; }
        public string? RegistrationNo { get; set; }
        public string? LastFollowUpId { get; set; }
        public string? NextDate { get; set; }
        public string? NextFollowupDate { get; set; }
        public string? FollowUpRemark { get; set; }
        public int FollowupStatus { get; set; }
        [Display(Name = "Class Name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? ClassCode { get; set; }
        public string? MotherName { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.com$", ErrorMessage = "Please enter a valid email ")]
        public string? Email { get; set; }

        public string? AlternateContactNo { get; set; }
        public string? SourceOfEnquiry { get; set; }
        [Display(Name = "Address")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? Address { get; set; }
        public string? Remarks { get; set; }

        public string? EnquiryConverttoReg { get; set; }
        public string? RegistrationDate { get; set; }
        public string? ConvertedRegtoAdm { get; set; }
        public string? AdmissionDate { get; set; }

    }
    public class EnquiryRequestDto : CommonClass
    {

        public string? Course { get; set; }
        public DateTime? FromDate { get; set; } = DateTime.Today;
        public DateTime? ToDate { get; set; } = DateTime.Today;
        public string? DateWorkAs { get; set; }
        public string? FollowStatus { get; set; }
        public string? AppliedFrom { get; set; }
        public string? StudentName { get; set; }
        public string? MobileNo { get; set; }
    }
    public class EnquiryModel : CommonClass
    {
        public int EnqId { get; set; }
        [Display(Name = "Enquiry Date")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public DateTime EnquiryDate { get; set; } = DateTime.Today;
        [Display(Name = "Student Name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Student Name can contain only letters and spaces")]
        public string? ChildFirstName { get; set; }
        public string? ChildMiddleName { get; set; }
        public string? ChildLastName { get; set; }
        public string? EnquiryNo { get; set; }
        [Display(Name = "Class Name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? ClassCode { get; set; }
        public DateTime DOB { get; set; } = DateTime.Today;
        [Display(Name = "Gender")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? Gender { get; set; }
        public string? Email { get; set; }
        [Display(Name = "Mobile No.")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Mobile No can contain only numbric.")]
        public string? MobileNo { get; set; }
        public string? AlternateContactNo { get; set; }
        [Display(Name = "Father Name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Father Name can contain only letters and spaces")]
        public string? FatherFirstName { get; set; }
        public string? MotherFirstName { get; set; }
        [Display(Name = "Address")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? Address { get; set; }
        public string? SourceOfEnquiry { get; set; }
        public string? Remarks { get; set; }
        public int IsOnline { get; set; }

    }
    public class AddFollowupRequest : CommonClass
    {
        public string? EnquiryId { get; set; }
        public DateTime? FollowupDate { get; set; } = DateTime.Now;
        public string? FollowupStatus { get; set; } = "1";
        public DateTime? NextFollowupDate { get; set; } = DateTime.Now;
        [Display(Name = "Remarks")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? Remarks { get; set; }
        public string? NeverCall { get; set; }
        public string? InteractionVia { get; set; } = "0";

    }
    public class FollowupDetailsResponse : CommonClass
    {
        public int SrNo { get; set; }
        public long FollowUpId { get; set; }
        public string? EnquiryId { get; set; }
        public string? FollowUpDate { get; set; }
        public string? FollowUpRemark { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? NextDate { get; set; }
        public bool? IsValid { get; set; }
        public bool? NeverCall { get; set; }
        public string? InteractionVia { get; set; }
        public int FollowupStatus { get; set; }
        public string? FollowupStatusName { get; set; }

    }
    #endregion

    #region------------------ Concession Model---------------------------
    public class ConcessionGroupHistroy : MNGTCommon
    {
    }
    public class ConcessionModel : MNGTCommon
    {
        public int ConcessionId { get; set; }
        [Display(Name = "Concession  name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Concession  name can contain only letters")]
        public string? Concession { get; set; }
        public string? Remarks { get; set; }
        [Display(Name = "Concession type")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Concession type  name can contain only letters")]
        public string? Type { get; set; } = "";
        public decimal IGST { get; set; }
        public decimal SGST { get; set; }
        public decimal CGST { get; set; }

    }

    public class StudentWithConcessionDto : MNGTCommon
    {
        public string? StudentNo { get; set; }
        public string? ControlNo { get; set; }
        public string? AdmissionNo { get; set; }
        public string? StudentName { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ClassCode { get; set; }
        public string? ClassName { get; set; }
        public string? ClassSection { get; set; }
        public long? SectionId { get; set; }
        public int ConcStudId { get; set; }
        public string? SectionName { get; set; }
        public string? RollNo { get; set; }
        public string? SMSMobileNo { get; set; }
        public string? FatherName { get; set; }
        public string? FatherContactNo { get; set; }
        public string? MotherName { get; set; }
        public string? MotherContactNo { get; set; }
        public string? IsReservedSeat { get; set; }
        public string? ImagePath { get; set; }
        public string? MotherImagePath { get; set; }
        public string? FatherImagePath { get; set; }
        public DateTime? AdmissionDate { get; set; }
        public string? AdmClass { get; set; }
        public string? AdmSession { get; set; }
        public string? BoardRollNo { get; set; }
        public string? StudentCategoryName { get; set; }
        public long? SocietyId { get; set; }
        public string? AadhaarNo { get; set; }
        public string? ReligionName { get; set; }
        public string? Visitor1ImagePath { get; set; }
        public string? Visitor2ImagePath { get; set; }
        public string? Visitor3ImagePath { get; set; }
        public string? Visitor4ImagePath { get; set; }
        public int ConcessionId { get; set; }
        public string? Concession { get; set; }
        public string? ValidFrom { get; set; }
        public string? ValidUpto { get; set; }
        public string? Remarks { get; set; }
        public int Status { get; set; }
        public string? ConcessionType { get; set; }
    }
    public class StudentConcessionFilterDto
    {
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }
        public long SessionId { get; set; }
        public string? ClassCode { get; set; }
        public int SectionId { get; set; }
        public int ConcessionId { get; set; }
        public string? ConStatus { get; set; } = "3";
        public string? StudentStatus { get; set; } = "-1";
    }
    public class ConcessionFeehead : CommonClass
    {
        public int ConcessionId { get; set; }
        public int FeeHeadId { get; set; }
        public int ConcessionType { get; set; }
        public decimal ConcessionValue { get; set; }

    }
    public class StudentConcessionDto : MNGTCommon
    {
        public string? ClassCode { get; set; }
        public string? Remarks { get; set; }
        public DateTime FromDate { get; set; } = DateTime.Today;
        public DateTime ToDate { get; set; } = DateTime.Today;

        public List<int>? ConcessionIds { get; set; }
        public List<FeeHeadDto>? FeeHeadList { get; set; }
    }
    public class FeeHeadDto
    {
        public int ConcessionId { get; set; }
        public int ConcStudId { get; set; } 
        public int FeeHeadId { get; set; }
        public int ConcessionType { get; set; }
        public decimal ConcessionValue { get; set; }
    }
    public class StudentConcessionRemarks : CommonClass
    {
        public long StudentId { get; set; }
        public int ConcStudId { get; set; }
        public string? Remarks { get; set; }
        public string? Status { get; set; }
    }
    public class ConcessionManageRequest : MNGTCommon
    {
        public int ConcStudId { get; set; }
        public string? ApproveBy { get; set; }
        public string? ApproveRemarks { get; set; }
    }
    public class StudentMappedConcessionDto : MNGTCommon
    {
        public string? ClassCode { get; set; }

        public int ConcessionId { get; set; }
        public int ConcStudId { get; set; }
        public string? ConcessionName { get; set; }
        public int Status { get; set; }

        public int FeeHeadId { get; set; }
        public string? FeeHeadName { get; set; }
        public string? ApproveRemarks { get; set; }

        public int ConcessionType { get; set; }
        public decimal ConcessionValue { get; set; }

        public DateTime ValidFrom { get; set; }
        public DateTime ValidUpto { get; set; }
    }
    public class UnMapConcessionRequest : MNGTCommon
    {
        public int ConcessionId { get; set; }
        public int ConcStudId { get; set; }
        public string? ApproveRemarks { get; set; }
        public string? ApproveBy { get; set; }

    }

    #endregion

    #region------------------------------- Set Late Fee Model--------------------
    public class LateFeeConfigration : MNGTCommon
    {
        public int PeriodId { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today;

        //[Display(Name = "Late Fee")]
        //[Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        //[RegularExpression(@"^[0-9]$", ErrorMessage = "Late Fee can contain only numbric.")]
        public decimal Amount { get; set; }
        public decimal MaxAmount { get; set; }
        public string? ClassCode { get; set; }
        public string? PenaltyType { get; set; } = "1";
    }
    public class LateFeeConfigData : MNGTCommon
    {
        public int Lid { get; set; }
        public decimal Amount { get; set; }
        public decimal MaxLimit { get; set; }
        public int PeriodId { get; set; }
        public string? PeriodName { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today;
        public string? ClassCode { get; set; }
        public string? ClassName { get; set; }
        public string? ClassOrder { get; set; }
        public int IsMapped { get; set; }
        public string? PenaltyType { get; set; }


    }
    public class MaxLateFeeModal : MNGTCommon
    {
        public decimal Amount { get; set; }
        public string? PenaltyType { get; set; }

    }
    public class ActivateModal
    {
        public int Lid { get; set; }
        public long SessionId { get; set; }
        public int Status { get; set; }
        public string? CreatedBy { get; set; }

    }
    #endregion

    #region------------------- Student Concession Promoted------------
    public class PromoteStudent : MNGTCommon
    {

        public string? ClassCode { get; set; }

        public string? SectionId { get; set; }

        public int ConcessionId { get; set; }

        public string? Concessiontype { get; set; }

        public string? PromoteComment { get; set; }

    }
    public class StudentResponse : MNGTCommon
    {
        public string? ControlNo { get; set; }
        public string? AdmissionNo { get; set; }
        public string? StudentName { get; set; }
        public string? Gender { get; set; }
        public DateTime? DOB { get; set; }
        public string? ClassSection { get; set; }
        public int RollNo { get; set; }
        public int? ConcessionId { get; set; }
        public string? Concession { get; set; }
        public string? Type { get; set; }
    }
    public class PromoteConcessionRequest
    {
        public string GroupCode { get; set; }

        public string BranchCode { get; set; }

        public long FromSessionId { get; set; }

        public long ToSessionId { get; set; }

        public string CreatedBy { get; set; }
        public string Remarks { get; set; } = string.Empty;

        public List<long> StudentIds { get; set; }
    }
    public class MissingStudentModel
    {
        public long StudentId { get; set; }
        public string StudentNo { get; set; }
        public string StudentName { get; set; }
    }
    public class PromoteConcessionResponse
    {
        public int ResultCode { get; set; }

        public List<MissingStudentModel> MissingStudents { get; set; }
    }

    #endregion

    #region---------------Student Promotion-------------
    public class ClassWiseStudentForPromotion : MNGTCommon
    {

        public string? StudentNo { get; set; }
        public string? ControlNo { get; set; }

        public string? StudentName { get; set; }

        public string? FatherName { get; set; }

        public string? Gender { get; set; }

        public string? ClassName { get; set; }

        public string? SectionName { get; set; }

        public int MobileNo { get; set; }

    }
    public class PromoteClassModel : MNGTCommon
    {

        public string? ClassCode { get; set; }

        public int SectionId { get; set; }

        public string? PromoteType { get; set; }

        public string? PromoteComment { get; set; }

    }

    public class StudentNotPromotedModel
    {
        public int SlNo { get; set; }
        public string AdmissionNo { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string ClassSection { get; set; } = string.Empty;
        public string? EWS { get; set; }
        public decimal Due { get; set; }
        public string? Active { get; set; }
    }

    #endregion

    #region------------ Student Geneate Invoice--------------

    public class StudentForInvoiceRequestModel
    {
        public string GroupCode { get; set; } = string.Empty;

        public string BranchCode { get; set; } = string.Empty;

        public long SessionId { get; set; }

        public string ClassCode { get; set; } = string.Empty;

        public string Section { get; set; } = "0";

        public string StudentName { get; set; } = string.Empty;

        public string PeriodType { get; set; } = "Monthly";

        public int PeriodId { get; set; }
        public int SocietyId { get; set; } = 0;

        public string StudentNo { get; set; } = string.Empty;

        public string IsGenerated { get; set; } = "-1";

        public string StudentCategory { get; set; } = "0";
    }
    #endregion

    public class GetSearchedStudentRequestModel : MNGTCommon
    {
        public string? ClassCode { get; set; }
        public string? SectionCode { get; set; }
        public string? Gender { get; set; }
        public string? ControlNo { get; set; }
        public string? StudentName { get; set; }
        public string? IsEWS { get; set; }
        public string? JoinType { get; set; }
    }
    public class GetSearchedViewStudentModel : MNGTCommon
    {
        public string? StudentNo { get; set; }
        public string? ControlNo { get; set; }
        public string? AdmissionNo { get; set; }
        public string? StudentName { get; set; }
        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? ClassCode { get; set; }
        public string? ClassName { get; set; }
        public string? ClassSection { get; set; }
        public string? SectionId { get; set; }
        public string? SectionName { get; set; }
        public string? RollNo { get; set; }

        public string? SMSMobileNo { get; set; }

        public string? FatherName { get; set; }
        public string? FatherContactNo { get; set; }

        public string? MotherName { get; set; }
        public string? MotherContactNo { get; set; }

        public string? IsReservedSeat { get; set; }

        public string? ImagePath { get; set; }
        public string? MotherImagePath { get; set; }
        public string? FatherImagePath { get; set; }

        public DateTime? AdmissionDate { get; set; }

        public string? AdmClass { get; set; }
        public string? AdmSession { get; set; }

        public string? BoardRollNo { get; set; }

        public string? StudentCategoryName { get; set; }

        public string? SocietyId { get; set; }

        public string? AadhaarNo { get; set; }
        public string? ReligionName { get; set; }

        public string? Visitor1ImagePath { get; set; }
        public string? Visitor2ImagePath { get; set; }
        public string? Visitor3ImagePath { get; set; }
        public string? Visitor4ImagePath { get; set; }
    }
    public class FeeCollectionMonthMappingModel : MNGTCommon
    {
        public int MonthNo { get; set; }
        public string? MonthName { get; set; }
        public int IsMapped { get; set; }
    }

    #region--------Generate Invoice Model ----------------------
    public class StudentInvoice
    {

        public string? ClassCode { get; set; }
        public int SectionId { get; set; }
        public int InvoiceTypeId { get; set; }
        public string? PeriodType { get; set; }
        public string? PeriodId { get; set; }
        public string? StudentId { get; set; }
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }
        public int SocietyId { get; set; } = 0;
        public string? CreatedBy { get; set; }
        public long SessionId { get; set; }
        public string? InvoiceFor { get; set; }

    }
    public class InvoiceTypeModel
    {
        public int InvoiceTypeId { get; set; }
        public string InvoiceTypeName { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class StudentClassModal
    {
        public int SessionId { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? ClassCode { get; set; }
        public string? AdmissionNo { get; set; }
        public string? ClassSection { get; set; }
        public string? ControlNo { get; set; }
        public bool IsSelected { get; set; }

    }
    public class StudentFeeInvoiceResponseModel
    {
        public long SessionId { get; set; }
        public bool IsSelected { get; set; }
        public long StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string RollNo { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public int ClassOrder { get; set; }
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public string InvoiceNo { get; set; } = string.Empty;
        public string ControlNo { get; set; } = string.Empty;
        public string StudentCategoryName { get; set; } = string.Empty;
        public string CreatedDate { get; set; } = string.Empty;
        public int IsHighCount { get; set; }
    }
    #endregion

    #region-----------View Student Details Model And Submit Student-------------------------------
    public class ViewStudentModal : MNGTCommon
    {
        public string? ControlNo { get; set; }
        public string? AdmissionNo { get; set; }
        public string? StudentName { get; set; }
        public string? RollNo { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
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
        public DateTime? AdmissionDate { get; set; }
    }
    public class StudentRequest : MNGTCommon
    {
        public string? ClassCode { get; set; }
        public string? SectionId { get; set; }
        public string? Gender { get; set; }
        public string? StudentName { get; set; }
        public string? StudentNo { get; set; }
        public int IsSearchOnAdmDate { get; set; }
        public DateTime? AdmFromDate { get; set; } = DateTime.Today;
        public DateTime? AdmToDate { get; set; } = DateTime.Today;
        public string? StudentStatus { get; set; } = "-1";
        public int ValidStatus { get; set; } = 1;
        public string OrderBy { get; set; } = "1";
    }
    public class StudentViewDetailsModel : MNGTCommon
    {
        public string? AdmissionNo { get; set; }
        public string? ControlNo { get; set; }
        public string? StudentNo { get; set; }
        [Display(Name = "First name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "First name can contain only letters")]
        [StringLength(100)]
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        [Display(Name = "Gender name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "First name can contain only letters")]
        public string? Gender { get; set; }
        [Display(Name = "Date Of birth")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public DateTime? DateOfBirth { get; set; }
        public string? ClassCode { get; set; }
        public long? SectionId { get; set; }
        public string? BloodGroup { get; set; }
        public string? MedicalInformation { get; set; }
        [Display(Name = "Nationality")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public string? Nationality { get; set; }
        public string? SocialCategory { get; set; }
        public string? StudentImg { get; set; }
        public string? Religion { get; set; }
        public string? GSRNNo { get; set; }
        public string? Background { get; set; }
        public string? MotherTongue { get; set; }
        public string? HouseNo { get; set; }
        public int IsTransportRequired { get; set; }
        public bool IsHostelRequired { get; set; }
        public bool IsSMSRequired { get; set; }
        public bool IsEmailRequired { get; set; }
        public bool? IsNRI { get; set; }
        public decimal? RouteDistance { get; set; }
        public string? SpecialComments { get; set; }
        public string? ImagePath { get; set; }
        public string? ClassName { get; set; }
        public string? SectionName { get; set; }
        public string? StudentName { get; set; }
        public DateTime? AdmissionDate { get; set; }
        public string? SiblingID { get; set; }
        public string? AadhaarNo { get; set; }
        public string? ClassSection { get; set; }
        public int IsReservedSeat { get; set; }
        public string? RoutName { get; set; }
        public string? LastSchool { get; set; }
        public string? LoginName { get; set; }
        public DateTime? WithdrawalDate { get; set; }
        public string? StudentBankName { get; set; }
        public string? IFSC { get; set; }
        public string? StudentBankAccount { get; set; }
        public string? AccountHolderName { get; set; }
        public bool IsDisability { get; set; }
        public string? NatureOfDisability { get; set; }
        public string? AdmissionClass { get; set; }
        public string? LastClassStudied { get; set; }
        public string? LastAcademicSession { get; set; }
        public string? LastSchoolResult { get; set; }
        public string? Attendance { get; set; }
        public string? IsCustody { get; set; }
        public string? CustodyStatus { get; set; }
        public string? StateCode { get; set; }
        public long? EWSId { get; set; }
        public string? LastSchoolAddress { get; set; }
        public string? LastSchoolTcNo { get; set; }
        public DateTime? TcDate { get; set; }
        public string? LastSchoolBoard { get; set; }
        public decimal? Percentage { get; set; }
        public string? Subjects { get; set; }
        public string? Caste { get; set; }
        public bool IsTCAttach { get; set; }
        public bool IsSubjectApproved { get; set; }
        public string? RecommendationBy { get; set; }
        public string? EmployeeApproched { get; set; }
        public string? RecommendationDocPath { get; set; }
        public string? RecommendationDocFile { get; set; }
        public string? ApaarId { get; set; }
        public string? PENNo { get; set; }
        public string? FamilyId { get; set; }
        public string? EmailId { get; set; }
        public string? MobileNo { get; set; }
        public DateTime? TransportAppliedFrom { get; set; }
        public string? CBSEGamesId { get; set; }
        public int FeeTemplateId { get; set; }
    }
    public class StudentParentDetailsModel : StudentAdditionalInformationModel
    {

        // Father Details
        public string? FatherTitle { get; set; }
        public string? FatherName { get; set; }
        public string? FatherMName { get; set; }
        public string? FatherLName { get; set; }
        public DateTime? FatherDOB { get; set; }
        public string? FatherQualification { get; set; }
        public string? FatherCollege { get; set; }
        public string? FatherOccupation { get; set; }
        public string? FatherOccupationOther { get; set; }
        public string? FatherDesignation { get; set; }
        public string? FatherOrganisation { get; set; }
        public string? FatherOfficeContactNo { get; set; }
        public string? FatherOfficeAddress { get; set; }
        public string? FatherEMail { get; set; }
        public decimal? FatherAnnualIncome { get; set; }
        public string? FatherContactNo { get; set; }
        public string? FatherAchievement { get; set; }
        public string? FatherImagePath { get; set; }
        public string? FatherAadhaarNo { get; set; }

        // Mother Details
        public string? MotherTitle { get; set; }
        public string? MotherName { get; set; }
        public string? MotherMName { get; set; }
        public string? MotherLName { get; set; }
        public DateTime? MotherDOB { get; set; }
        public string? MotherQualification { get; set; }
        public string? MotherCollege { get; set; }
        public string? MotherOccupation { get; set; }
        public string? MotherOccupationOther { get; set; }
        public string? MotherDesignation { get; set; }
        public string? MotherOrganisation { get; set; }
        public string? MotherOfficeContactNo { get; set; }
        public string? MotherOfficeAddress { get; set; }
        public string? MotherEMail { get; set; }
        public decimal? MotherAnnualIncome { get; set; }
        public string? MotherContactNo { get; set; }
        public string? MotherAchievement { get; set; }
        public string? MotherImagePath { get; set; }
        public string? MotherAadhaarNo { get; set; }
        [Display(Name = "SMS mobile no")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression("^[0-9]{10}$", ErrorMessage = "Mobile number must be numeric and exactly 10 digits.")]
        public string? SMSMobileNo { get; set; }
        public string? ContactEmail { get; set; }
        public string? EmergencyPersonName { get; set; }
        public string? EmergencyPersonRelationShip { get; set; }
        public string? EmergencyPersonContactNo { get; set; }
        public string? EmergencyPersonAddress { get; set; }
        public string? GuardianName { get; set; }
        public string? GuardianEmail { get; set; }
        public string? GuardianContactNo { get; set; }
        public string? GuardianRelationship { get; set; }
        public string? GuardianAddress { get; set; }

    }
    public class SiblingDetailsModel : MNGTCommon
    {
        public string? FatherName { get; set; }
        public string? StudentName { get; set; }
        public string? MotherName { get; set; }
        public string? ClassName { get; set; }
        public string? SectionName { get; set; }
        public string? BranchName { get; set; }
        public string? SiblingID { get; set; }
        public string? ControlNo { get; set; }
    }
    public class AddSiblingRequest
    {
        public string GroupCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public long ParentStudentId { get; set; }
        public long ChildStudentId { get; set; }
    }
    public class StudentAddressDetailsModel : MNGTCommon
    {
        [Display(Name = "Address Line1")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(150, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? Line1 { get; set; }
        [Display(Name = "Address Line1")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [StringLength(150, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? PermanentLine1 { get; set; }
        public string? Line2 { get; set; }
        public string? PinCode { get; set; }
        public string? FatherName { get; set; }
        public string? MotherName { get; set; }
        public int DistrictId { get; set; }
        public int StateId { get; set; }
        [Display(Name = "Contact number")]
        [StringLength(10, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? ContactNo { get; set; }
        public string? AddressTo { get; set; }
        public string? AddressType { get; set; }
        [Display(Name = "Father contact number")]
        [StringLength(10, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? FatherContactNo { get; set; }
        [Display(Name = "Mother contact number")]
        [StringLength(10, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? MotherContactNo { get; set; }
        [Display(Name = "Emergency person name")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Emergency person can contain only letters")]
        public string? EmergencyPersonName { get; set; }
        [Display(Name = "Relationship")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Relationship name can contain only letters")]
        public string? EmergencyPersonRelationShip { get; set; }
        [Display(Name = "Emergency contact number")]
        [StringLength(10, ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MaxLengthExceeded")]
        public string? EmergencyPersonContactNo { get; set; }
        public string? EmergencyPersonAddress { get; set; }
    }
    public class StudentPassportVisaModel : MNGTCommon
    {
        public string? PassportNo { get; set; }
        public int PassportType { get; set; }
        public bool PassportRegistrationRquired { get; set; }
        public DateTime PassportIssueDate { get; set; } = DateTime.Today;
        public DateTime PassportExpiryDate { get; set; } = DateTime.Today;
        public string? VisaNo { get; set; }
        public string? RecommendationBy { get; set; }
        public string? EmployeeApproched { get; set; }
        public string? RecommendationDocFile { get; set; }
        public string? RecommendationDocPath { get; set; }
        public string? SpecialComments { get; set; }
        public int VisaType { get; set; }
        public bool VisaRegistrationRequired { get; set; }
        public DateTime? VisaIssueDate { get; set; } = DateTime.Today;
        public DateTime? VisaExpiryDate { get; set; } = DateTime.Today;

    }
    public class StudentAdditionalInformationModel : MNGTCommon
    {
        public string? StuBankName { get; set; }

        public string? StuAccountNo { get; set; }

        public string? StuBankIFSC { get; set; }

        public string? AccountHolderName { get; set; }

        public string? LastSchool { get; set; }

        public string? LastClassStudied { get; set; }

        public string? LastAcademicSession { get; set; }

        public string? LastSchoolResult { get; set; }

        public string? Attendance { get; set; }

        public string? CustodyStatus { get; set; }

        public string? LastSchoolAddress { get; set; }

        public string? LastSchoolTcNo { get; set; }

        public DateTime? TcDate { get; set; }

        public string? LastSchoolBoard { get; set; }

        public string? Percentage { get; set; }

        public string? Subjects { get; set; }

        public bool IsTCAttach { get; set; }

        public bool IsSubjectApproved { get; set; }
    }
    #endregion

    #region----------------- Visitor Details Model---------------------------------
    public class StudentVisitorsModel : CommonClass
    {
        public bool IsMotherAllowed { get; set; }
        public bool IsFatherAllowed { get; set; }
        public string? Visitor1Name { get; set; }
        public string? Visitor1Relation { get; set; }
        public string? Visitor1ImagePath { get; set; }
        public string? Visitor1SignImagePath { get; set; }
        public bool IsVisitor1Allowed { get; set; }
        public string? Visitor2Name { get; set; }
        public string? Visitor2Relation { get; set; }
        public string? Visitor2ImagePath { get; set; }
        public string? Visitor2SignImagePath { get; set; }
        public bool IsVisitor2Allowed { get; set; }
        public string? Visitor3Name { get; set; }
        public string? Visitor3Relation { get; set; }
        public string? Visitor3ImagePath { get; set; }
        public string? Visitor3SignImagePath { get; set; }
        public bool IsVisitor3Allowed { get; set; }
        public string? Visitor4Name { get; set; }
        public string? Visitor4Relation { get; set; }
        public string? Visitor4ImagePath { get; set; }
        public string? Visitor4SignImagePath { get; set; }
        public bool IsVisitor4Allowed { get; set; }
        public string? Visitor5Name { get; set; }
        public string? Visitor5Relation { get; set; }
        public string? Visitor5ContactNo { get; set; }
        public string? Visitor5Email { get; set; }
        public string? Visitor5Remarks { get; set; }
        public string? Visitor5ImagePath { get; set; }
        public bool IsVisitor5Allowed { get; set; }
        public string? VisitorFSignImagePath { get; set; }
        public string? VisitorMSignImagePath { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.com$", ErrorMessage = "Please enter a valid email ")]
        public string? Visitor1Email { get; set; }
        public string? Visitor1ContactNo { get; set; }
        public string? Visitor1Remarks { get; set; }
        public string? Visitor1OfficeNo { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.com$", ErrorMessage = "Please enter a valid email ")]
        public string? Visitor2Email { get; set; }
        public string? Visitor2ContactNo { get; set; }
        public string? Visitor2Remarks { get; set; }
        public string? Visitor2OfficeNo { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.com$", ErrorMessage = "Please enter a valid email ")]
        public string? Visitor3Email { get; set; }
        public string? Visitor3ContactNo { get; set; }
        public string? Visitor3Remarks { get; set; }
        public string? Visitor3OfficeNo { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.com$", ErrorMessage = "Please enter a valid email ")]
        public string? Visitor4Email { get; set; }
        public string? Visitor4ContactNo { get; set; }
        public string? Visitor4Remarks { get; set; }
        public string? Visitor4OfficeNo { get; set; }
        public bool IsValid { get; set; }
        public DateTime? CreatedDate { get; set; }
        public long StudentId { get; set; }
    }
    public class ProfileImageModal : MNGTCommon
    {

        public string? ImageFor { get; set; }

        public string? UpdatedBy { get; set; }

    }

    #endregion

    #region----------- View Challan Dues Model --------------------------
    public class StudentInvoiceDuesRequest : MNGTCommon
    {
        public string ClassCode { get; set; } = string.Empty;
        public string SectionId { get; set; } = "";
        public string StudentNo { get; set; } = string.Empty;
        public int PeriodId { get; set; }
        public int SocietyId { get; set; }
        public int Status { get; set; } = -1;
        public string PeriodType { get; set; } = string.Empty;
        public string DuesAmountCheck { get; set; } = "0";
        public string RoleName { get; set; } = "SMSNo";
        public string EmployeeId { get; set; } = string.Empty;
        public bool IsWithLateFee { get; set; }
        public decimal? DuesAmount { get; set; }
        public int StudentCategory { get; set; } = 0;
    }
    public class StudentDuesModel : MNGTCommon
    {
        public int InvoiceId { get; set; }
        public string? BloodGroup { get; set; }
        public int RollNo { get; set; }
        public string? StudentNo { get; set; }
        public string? ControlNo { get; set; }
        public string? StudentName { get; set; }
        public string? ClassSection { get; set; }
        public int ClassOrder { get; set; }
        public string? SMSMobileNo { get; set; }
        public string? FatherEMail { get; set; }
        public string? FatherName { get; set; }
        public string? ContactEmail { get; set; }
        public string? CurrentAddress { get; set; }
        public string? EWS { get; set; }
        public string? MotherName { get; set; }
        public bool IsSMSRequired { get; set; }
        public string? PeriodName { get; set; }
        public decimal? StudentLateFee { get; set; }
        public string? SectionName { get; set; }
        public string? CommentText { get; set; }
        public decimal Amount { get; set; }
        public string? Category { get; set; }
        public string? Period { get; set; }

    }
    #endregion

    #region------------------- Print Challan Model ----------------------------
    public class InvoiceMasterModel
    {
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public long PeriodId { get; set; }
        public long SessionId { get; set; }
        public string PeriodName { get; set; } = string.Empty;
        public string SessionName { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string ClassSection { get; set; } = string.Empty;
        public long StudentId { get; set; }
        public string ControlNo { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string MotherName { get; set; } = string.Empty;
        public string DistanceName { get; set; } = string.Empty;
        public bool ISTRANSPORTREQUIRED { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public string NextToKin { get; set; } = string.Empty;
        public string NextToKinName { get; set; } = string.Empty;
        public string SMSMobileNo { get; set; } = string.Empty;
        public string AccountNo { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalConcession { get; set; }
        public decimal ArrearAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public int SocietyId { get; set; }
        public string PeriodType { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public long? ConcessionId { get; set; }
        public string Concession { get; set; } = string.Empty;
        public string QuarterName { get; set; } = string.Empty;
        public decimal StudentBalance { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal InvoicePreBalance { get; set; }
        public decimal StudentLateFee { get; set; }
        public decimal LateCharges { get; set; }
        public decimal PreviousAdvance { get; set; }
        public decimal AdjustmentApproved { get; set; }
        public string PeriodList { get; set; } = string.Empty;
        public int RouteDistance { get; set; } 
    }
    public class InvoiceFeeHeadModel:MNGTCommon
    {
        public int InvoiceId { get; set; }
        public int FeeHeadId { get; set; }
        public string FeeHeadName { get; set; } = string.Empty;
        public decimal FeeHeadAmount { get; set; }
        public decimal Concession { get; set; }
        public string FeeApplicableType { get; set; } = string.Empty;
        public decimal Payable { get; set; }
        public decimal Adjusted { get; set; }
        public decimal WaiveOff { get; set; }
        public decimal ReceivedAmt { get; set; }
        public string MonthDisplay { get; set; } = string.Empty;
        public string MonthName { get; set; } = string.Empty;
        public int MonthNo { get; set; } 
    }
    public class InvoiceDetailsResponse
    {
        public List<InvoiceMasterModel> InvoiceMaster { get; set; } = new();
        public List<InvoiceFeeHeadModel> FeeHeadList { get; set; } = new();
    }
    #endregion

    public class DashboardCardDto
    {
        public string Title { get; set; } = "";
        public decimal Value { get; set; }
        public decimal Percentage { get; set; }
        public bool IsIncrease { get; set; }
        public string CompareText { get; set; } = "";
        public int Progress { get; set; }
        public List<DashboardGraphDto> GraphData { get; set; } = new();
    }
    public class DashboardGraphDto
    {
        public int MonthNo { get; set; }
        public int PreviousValue { get; set; }
        public int CurrentValue { get; set; }
    }


    public class DashboardResponse
    {
        public DashboardCardDto TotalStudents { get; set; } = new();
        public DashboardCardDto NewAdmissions { get; set; } = new();
        public DashboardCardDto TodaysCollection { get; set; } = new();
        public DashboardCardDto OutstandingFees { get; set; } = new();
    }

    #region-------------- Search Student Balance------------------------------
    public class SearchStudentBalanceDto : MNGTCommon
    {
        public string? StudentName { get; set; }
        public string? StudentNo { get; set; }
        public string? ControlNo { get; set; }
        public string? ClassSection { get; set; }
        public string? AdmissionNo { get; set; }
        public string? ClassCode { get; set; }
        public int TemplateId { get; set; }
        public int PeriodId { get; set; }
        public int InvType { get; set; }
        public int InvoiceId { get; set; }
        public string? SectionId { get; set; }
        public string? ImagePath { get; set; }
        public string? TemplateName { get; set; }
        public string? PeriodType { get; set; }
        public string? DueDate { get; set; }
        public string? InvoiceDate { get; set; }
        public decimal ArrearAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalConcession { get; set; }
        public decimal ReceivedAmount { get; set; }


    }
    #endregion

    #region------------------ Edit Challan -------------------------
    public class ChallanDueDateModal
    {
        public string GroupCode { get; set; } = string.Empty;

        public string BranchCode { get; set; } = string.Empty;

        public int InvoiceId { get; set; }

        [Display(Name = "Due Date")]
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "RequiredField")]
        public DateTime? DueDate { get; set; }

        public string UpdatedBy { get; set; } = string.Empty;

    }
    #endregion
    public class FeeHeadDropdownModel
    {
        public long FeeHeadId { get; set; }
        public string FeeHeadName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
    public class TransportSelectMonthModel
    {   
        public long StudentId { get; set; }
        public long SessionId { get; set; }
        public int MonthNo { get; set; }
    }
    public class FeeHeadToStudentChallan
    {
        [Required]
        public long StudentId { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        [Required]
        public int HeadId { get; set; }

        public int SelectMonthNo { get; set; }

        public List<int> MonthNo { get; set; } = new();

        [Required]
        public decimal HeadAmount { get; set; }

        [Required]  
        [StringLength(50)]
        public string CreatedBy { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Narration1 { get; set; }
    }


}



