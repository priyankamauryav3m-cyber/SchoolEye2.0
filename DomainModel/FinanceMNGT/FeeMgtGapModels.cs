using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DomainModel.FinanceMNGT
{
    // ============================================================
    // Optional Fee Group  (Configuration #8)
    // ============================================================
    #region Optional Fee Group

    public class OptionalFeeGroupModel : MNGTCommon 
    {
        public int OptionalGroupId { get; set; }

        [Required(ErrorMessage = "Group Name is required.")]
        [StringLength(100, ErrorMessage = "Group Name cannot exceed 100 characters.")]
        public string? OptionalGroupName { get; set; }

        [Required(ErrorMessage = "Group Code is required.")]
        [StringLength(30, ErrorMessage = "Group Code cannot exceed 30 characters.")]
        public string? OptionalGroupCode { get; set; }

        [Range(1, 50, ErrorMessage = "Max Fee Head Count must be between 1 and 50.")]
        public int MaxFeeHeadCount { get; set; } = 1;

        public DateTime? OpenTillDate { get; set; }

        [StringLength(250)]
        public string? Remarks { get; set; }

        public List<string>? MappedClasses { get; set; }
        public List<OptionalFeeGroupHeadModel>? MappedHeads { get; set; }
    }

    public class OptionalFeeGroupClassMapModel
    {
        public int MapId { get; set; }
        public int OptionalGroupId { get; set; }

        [Required(ErrorMessage = "Please select a Class.")]
        public string? ClassCode { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class OptionalFeeGroupHeadModel
    {
        public int MapId { get; set; }
        public int OptionalGroupId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a Fee Head.")]
        public int FeeHeadId { get; set; }
        public string? FeeHeadName { get; set; }

        [Range(0, 9999999999999999.99, ErrorMessage = "Amount cannot be negative.")]
        public decimal Amount { get; set; }
        public string? CreatedBy { get; set; }
    }

    #endregion

    // ============================================================
    // Ledger  (Challan #3 View Student Ledger, #4 FeeHead Ledger)
    // ============================================================
    #region Ledger

    public class StudentLedgerEntryModel
    {
        public long LedgerTxnId { get; set; }
        public DateTime TxnDate { get; set; }
        public string? TxnType { get; set; }
        public int? FeeHeadId { get; set; }
        public string? FeeHeadName { get; set; }
        public string? ReferenceType { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Remarks { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal RunningBalance { get; set; }
    }

    public class StudentLedgerRequestModel
    {
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }
        public long SessionId { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Please select a Student.")]
        public long StudentId { get; set; }
        public string? StudentName { get; set; }
    }

    public class FeeHeadLedgerEntryModel
    {
        public long LedgerTxnId { get; set; }
        public long StudentId { get; set; }
        public string? StudentName { get; set; }
        public DateTime TxnDate { get; set; }
        public string? TxnType { get; set; }
        public string? ReferenceType { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Remarks { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
    }

    public class FeeHeadLedgerRequestModel
    {
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }
        public long SessionId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a Fee Head.")]
        public int FeeHeadId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    #endregion

    // ============================================================
    // Miscellaneous Challan / Receipt  (Challan #5, Fee Collection #4)
    // ============================================================
    #region Miscellaneous Challan / Receipt

    public class MiscellaneousChallanModel : MNGTCommon
    {
        public long MiscChallanId { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Please select a Student.")]
        public long StudentIdRef { get; set; }
        public string? StudentName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a Fee Head.")]
        public int FeeHeadId { get; set; }
        public string? FeeHeadName { get; set; }

        public string? ForMonth { get; set; }

        [Range(0.01, 9999999999999999.99, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        public DateTime? DueDate { get; set; }

        [StringLength(250)]
        public string? Remarks { get; set; }
        public string? Status { get; set; }
    }

    public class MiscellaneousReceiptModel : MNGTCommon
    {
        public long MiscReceiptId { get; set; }
        public long? MiscChallanId { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Please select a Student.")]
        public long StudentIdRef { get; set; }
        public string? StudentName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a Fee Head.")]
        public int FeeHeadId { get; set; }
        public string? FeeHeadName { get; set; }

        [Required(ErrorMessage = "Receipt No is required.")]
        [StringLength(30)]
        public string? ReceiptNo { get; set; }

        [Range(0.01, 9999999999999999.99, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }
        public DateTime ReceiptDate { get; set; } = DateTime.Today;

        [Range(1, int.MaxValue, ErrorMessage = "Please select a Payment Mode.")]
        public int PaymentModeId { get; set; }
        public string? PaymentModeName { get; set; }

        [StringLength(250)]
        public string? Remarks { get; set; }
    }

    #endregion

    // ============================================================
    // Adjustment Receipts  (Fee Collection #6)
    // ============================================================
    #region Adjustment Receipts

    public class AdjustmentReceiptModel : MNGTCommon
    {
        public long AdjustmentId { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Please select a Student.")]
        public long StudentIdRef { get; set; }
        public string? StudentName { get; set; }

        public int? FeeHeadId { get; set; }
        public string? FeeHeadName { get; set; }

        [Required(ErrorMessage = "Please select Add or Deduct.")]
        public string? AdjustmentType { get; set; } // "Add" or "Deduct"

        [Range(0.01, 9999999999999999.99, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Reason is required.")]
        [StringLength(250)]
        public string? Reason { get; set; }

        public string? Status { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime AdjustmentDate { get; set; } = DateTime.Today;
    }

    public class AdjustmentApprovalRequest
    {
        public long AdjustmentId { get; set; }
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }
        public long SessionId { get; set; }
    }

    #endregion

    // ============================================================
    // Journal Account / Journal Voucher  (Payment #1, #2)
    // ============================================================
    #region Journal Account / Voucher

    public class JournalAccountGroupModel : MNGTCommon
    {
        public int AccountGroupId { get; set; }

        [Required(ErrorMessage = "Account Type is required.")]
        public string? AccountType { get; set; } // Asset, Liability, Income, Expense, Equity

        [Required(ErrorMessage = "First Group is required.")]
        [StringLength(100)]
        public string? FirstGroup { get; set; }

        [StringLength(100)]
        public string? SecondGroup { get; set; }
    }

    public class JournalAccountModel : MNGTCommon
    {
        public int AccountId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select an Account Group.")]
        public int AccountGroupId { get; set; }
        public string? FirstGroup { get; set; }
        public string? AccountType { get; set; }

        [Required(ErrorMessage = "Account Name is required.")]
        [StringLength(150)]
        public string? AccountName { get; set; }

        [Range(0, 9999999999999999.99)]
        public decimal OpeningBalance { get; set; }

        [Required(ErrorMessage = "Please select Opening Balance type.")]
        public string OpeningBalanceType { get; set; } = "D"; // D / C
    }

    public class JournalVoucherModel : MNGTCommon
    {
        public long VoucherId { get; set; }

        [Required(ErrorMessage = "Voucher No is required.")]
        [StringLength(30)]
        public string? VoucherNo { get; set; }

        [Required(ErrorMessage = "Voucher Date is required.")]
        public DateTime VoucherDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Transaction Type is required.")]
        public string? TransactionType { get; set; } // Journal, Contra, Purchase, Payment, Receipt, Sale, DebitNote, CreditNote

        [StringLength(500)]
        public string? Narration { get; set; }

        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }

        public List<JournalVoucherDetailModel> Details { get; set; } = new();
    }

    public class JournalVoucherDetailModel
    {
        public long DetailId { get; set; }
        public long VoucherId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select an Account.")]
        public int AccountId { get; set; }
        public string? AccountName { get; set; }

        [Required(ErrorMessage = "Please select Debit or Credit.")]
        public string DrCr { get; set; } = "D"; // D / C

        [Range(0.01, 9999999999999999.99, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [StringLength(250)]
        public string? Narration { get; set; }
    }

    #endregion
}
