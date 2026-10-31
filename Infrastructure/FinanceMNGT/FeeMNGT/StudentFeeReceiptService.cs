using ApplicationInterface.FinanceMNGT.FeeMNGT;
using ApplicationInterface.GenerateFile;
using ApplicationInterface.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DomainModel.FinanceMNGT;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SchoolEye.ApplicationInterface.Admin;

namespace SchoolEye.Infrastructure.Admin
{
    public class StudentFeeReceiptService : IStudentFeeReceiptRepository
    {
        private readonly string _connectionString;
        private readonly IOtpSenderService _otpSender;
        private readonly IPrintStudentRec _generateFile;

        // cancel -> refund to parent OTP
        private const int RefundOtpValidMinutes = 5;

        public StudentFeeReceiptService(IConfiguration configuration, IOtpSenderService otpSender, IPrintStudentRec generateFile)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString") ??
                throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
            _otpSender = otpSender;
            _generateFile = generateFile;
        }

        // CancelType column is tinyint: the screen's "Cheque Bounce" / "Other" choice only decides how a
        // receipt is cancelled, so any non-numeric value is sent as "-1" (all) instead of breaking the SP
        private static string ToCancelTypeFilter(string? cancelType) =>
            int.TryParse(cancelType?.Trim(), out var value) ? value.ToString() : "-1";

        public async Task<IEnumerable<StudentFeeReceiptListResponse>> GetStudentFeeReceiptList(StudentFeeReceiptListRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@GroupClassCode", request.GroupClassCode);
                param.Add("@ClassCode", request.ClassCode);
                param.Add("@PaymentMode", request.PaymentMode);
                param.Add("@ReceiptMode", request.ReceiptMode);
                param.Add("@ReceiptNo", request.ReceiptNo);
                param.Add("@StudentNo", request.StudentNo);
                param.Add("@FromDate", request.FromDate);
                param.Add("@ToDate", request.ToDate);
                param.Add("@Status", request.Status);
                param.Add("@FirstName", request.FirstName);
                param.Add("@SocietyId", request.SocietyId);
                param.Add("@ChequeNo", request.ChequeNo);
                param.Add("@BankAccountId", request.BankAccountId);
                param.Add("@FeeReceivedBy", request.FeeReceivedBy);
                param.Add("@CancelType", ToCancelTypeFilter(request.CancelType));

                return await con.QueryAsync<StudentFeeReceiptListResponse>(
                    "V3M_FIN_GetStudentFeeReceiptList",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<ClassGroupResponse>> GetClassGroup(ClassGroupRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);

                return await con.QueryAsync<ClassGroupResponse>(
                    "V3M_FIN_GetClassGroup",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<StudentReceiptByIdResponse?> GetStudentReceiptById(StudentReceiptByIdRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@ReceiptId", request.ReceiptId);

                return await con.QueryFirstOrDefaultAsync<StudentReceiptByIdResponse>(
                    "V3M_FIN_GetStudentReceiptById",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<ReceiptDetailResponse>> GetReceiptDetail(StudentReceiptByIdRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@ReceiptId", request.ReceiptId);

                return await con.QueryAsync<ReceiptDetailResponse>(
                    "USP_FIN_GetReceiptDetail",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<List<StudentReceiptHeadWiseResponse>> GetWithModeReceiptHeadWiseDetail(StudentReceiptHeadWiseRequest request)
        {
            var result = new List<StudentReceiptHeadWiseResponse>();
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();

            parameters.Add("@GroupCode", request.GroupCode);
            parameters.Add("@BranchCode", request.BranchCode);
            parameters.Add("@SocietyId", request.SocietyId);
            parameters.Add("@SessionId", request.SessionId);
            parameters.Add("@ClassCode", request.ClassCode);
            parameters.Add("@ReceiptNo", request.ReceiptNo);
            parameters.Add("@StudentNo", request.StudentNo);
            parameters.Add("@StudentName", request.StudentName);
            parameters.Add("@CancelType", ToCancelTypeFilter(request.CancelType));
            parameters.Add("@CreatedBy", request.CreatedBy);
            parameters.Add("@FromDate", request.FromDate.Date);
            parameters.Add("@ToDate", request.ToDate.Date);
            parameters.Add("@BankAccountId", request.BankAccountId);
            parameters.Add("@PayMode", request.PayMode);
            parameters.Add("@ClassGroupCode", request.ClassGroupCode);

            var rows = await connection.QueryAsync(
                "dbo.FIN_GetWithModeRecieptHeadWiseDetail",
                parameters,
                commandType: CommandType.StoredProcedure);

            foreach (IDictionary<string, object> row in rows)
            {
                var item = new StudentReceiptHeadWiseResponse();

                foreach (var column in row)
                {
                    item.Values[column.Key] =
                        column.Value == DBNull.Value
                            ? null
                            : column.Value;
                }

                result.Add(item);
            }

            return result;
        }

        // returns @ResultStatus: 1 success, -2 invalid/cancelled receipt, -22 already bounced, -1 error
        public async Task<int> GenerateReceiptChqBounceChallan(ReceiptChqBounceRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@ReceiptId", request.ReceiptId);
                param.Add("@Remark", request.Remark);
                param.Add("@CancelledBy", request.CancelledBy);
                param.Add("@CancelType", request.CancelType);
                param.Add("@CancelledDate", request.CancelledDate?.Date, DbType.Date);
                param.Add("@ResultStatus", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await con.ExecuteAsync(
                    "V3M_FIN_GenerateReceiptChqBounceChallan",
                    param,
                    commandType: CommandType.StoredProcedure);

                return param.Get<int?>("@ResultStatus") ?? -1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        // returns @Result: 1 success, -1 receipt not found / already cancelled,
        // refund: -3 wrong OTP, -4 OTP not found / expired, -5 too many wrong attempts (SQL errors are thrown)
        public async Task<int> CancelStudentReceipt(CancelStudentReceiptRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@ReceiptId", request.ReceiptId);
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@CancelledBy", request.CancelledBy);
                param.Add("@CancelReason", request.CancelReason);
                param.Add("@CancelType", (byte)1, DbType.Byte);   // 1 = Cancelled (2 = Edited is for the edit flow)
                param.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.Output);
                param.Add("@SettleMode", request.SettlementMode, DbType.Byte);
                // only the hash goes to the database, it is compared with the stored hash in the SP
                param.Add("@RefundOtpHash", request.SettlementMode == 2 ? HashRefundOtp(request.ReceiptId, request.OtpCode) : null);

                await con.ExecuteAsync(
                    "V3M_FIN_CancelStudentReceipt",
                    param,
                    commandType: CommandType.StoredProcedure);

                return param.Get<int?>("@Result") ?? 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<ChqCombineReportResponse>> GetChqCombineReport(StudentChequeReportRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@GroupClassCode", request.GroupClassCode);
                param.Add("@ClassCode", request.ClassCode);
                param.Add("@ReceiptMode", request.ReceiptMode);
                param.Add("@ReceiptNo", request.ReceiptNo);
                param.Add("@StudentNo", request.StudentNo);
                param.Add("@FromDate", request.FromDate.Date, DbType.Date);
                param.Add("@ToDate", request.ToDate.Date, DbType.Date);
                param.Add("@Status", request.Status);
                param.Add("@FirstName", request.FirstName);
                param.Add("@SocietyId", request.SocietyId);
                param.Add("@ChequeNo", request.ChequeNo);
                param.Add("@BankAccountId", request.BankAccountId);
                param.Add("@FeeReceivedBy", request.FeeReceivedBy);
                param.Add("@CancelType", ToCancelTypeFilter(request.CancelType));
                param.Add("@ChqType", request.ChqType);

                return await con.QueryAsync<ChqCombineReportResponse>(
                    "Fin_GetOnlyChqCombineReport",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<PrintViewStudentReceiptResponse>> GetPrintViewStudentReceipts(StudentChequeReportRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@GroupClassCode", request.GroupClassCode);
                param.Add("@ClassCode", request.ClassCode);
                param.Add("@PaymentMode", request.PaymentMode);
                param.Add("@ReceiptMode", request.ReceiptMode);
                param.Add("@ReceiptNo", request.ReceiptNo);
                param.Add("@StudentNo", request.StudentNo);
                param.Add("@FromDate", request.FromDate.Date, DbType.Date);
                param.Add("@ToDate", request.ToDate.Date, DbType.Date);
                param.Add("@Status", request.Status);
                param.Add("@FirstName", request.FirstName);
                param.Add("@SocietyId", request.SocietyId);
                param.Add("@ChequeNo", request.ChequeNo);
                param.Add("@BankAccountId", request.BankAccountId);
                param.Add("@FeeReceivedBy", request.FeeReceivedBy);
                param.Add("@CancelType", ToCancelTypeFilter(request.CancelType));
                param.Add("@ReceiptIds", request.ReceiptIds);
                param.Add("@ChqType", request.ChqType);

                return await con.QueryAsync<PrintViewStudentReceiptResponse>(
                    "V3M_FIN_PrintViewStudentReceipts",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        // ---------------- Cancel -> Refund to Parent: OTP ----------------
        private async Task<ReceiptRefundContactRow?> GetRefundContactRow(string? groupCode, string? branchCode, long receiptId)
        {
            using var con = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@GroupCode", groupCode);
            param.Add("@BranchCode", branchCode);
            param.Add("@ReceiptId", receiptId);

            return await con.QueryFirstOrDefaultAsync<ReceiptRefundContactRow>(
                "V3M_FIN_GetReceiptRefundContacts",
                param,
                commandType: CommandType.StoredProcedure);
        }

        // parent email / SMS mobile of an active receipt, masked (ni****@gmail.com, ******3210)
        public async Task<ReceiptRefundContactsResponse?> GetReceiptRefundContacts(StudentReceiptByIdRequest request)
        {
            try
            {
                var row = await GetRefundContactRow(request.GroupCode, request.BranchCode, request.ReceiptId);
                if (row == null || row.Valid != true)
                    return null;

                return new ReceiptRefundContactsResponse
                {
                    ReceiptId = row.ReceiptId,
                    PaidAmount = row.PaidAmount ?? 0,
                    HasEmail = IsValidEmail(row.FatherEMail),
                    MaskedEmail = IsValidEmail(row.FatherEMail) ? MaskEmail(row.FatherEMail!) : null,
                    HasMobile = !string.IsNullOrWhiteSpace(row.SMSMobileNo),
                    MaskedMobile = string.IsNullOrWhiteSpace(row.SMSMobileNo) ? null : MaskMobile(row.SMSMobileNo!)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        // OTP generated here, only its hash is stored (V3M_FIN_SaveReceiptRefundOtp), then mailed to the parent
        public async Task<SendReceiptRefundOtpResponse> SendReceiptRefundOtp(SendReceiptRefundOtpRequest request)
        {
            try
            {
                var row = await GetRefundContactRow(request.GroupCode, request.BranchCode, request.ReceiptId);
                if (row == null || row.Valid != true)
                    return new SendReceiptRefundOtpResponse { Result = -1 };

                var isEmail = string.Equals(request.Channel, "Email", StringComparison.OrdinalIgnoreCase);
                if (isEmail ? !IsValidEmail(row.FatherEMail) : string.IsNullOrWhiteSpace(row.SMSMobileNo))
                    return new SendReceiptRefundOtpResponse { Result = -3 };

                // SMS gateway is not connected yet: the mobile option is shown, the OTP goes by email only
                if (!isEmail)
                    return new SendReceiptRefundOtpResponse { Result = -4, SentTo = MaskMobile(row.SMSMobileNo!) };

                var otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
                var sentTo = MaskEmail(row.FatherEMail!);

                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@ReceiptId", request.ReceiptId);
                param.Add("@Channel", "Email");
                param.Add("@SentTo", sentTo);
                param.Add("@OtpHash", HashRefundOtp(request.ReceiptId, otp));
                param.Add("@ExpiryUtc", DateTime.UtcNow.AddMinutes(RefundOtpValidMinutes));
                param.Add("@CreatedBy", request.RequestedBy);
                param.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await con.ExecuteAsync("V3M_FIN_SaveReceiptRefundOtp", param, commandType: CommandType.StoredProcedure);

                var result = param.Get<int?>("@Result") ?? 0;
                if (result == 1)
                    await _otpSender.SendReceiptRefundOtpAsync(row.FatherEMail!.Trim(), otp, row.ReceiptNo ?? string.Empty, row.PaidAmount ?? 0, RefundOtpValidMinutes);

                return new SendReceiptRefundOtpResponse { Result = result, SentTo = sentTo, ValidMinutes = RefundOtpValidMinutes };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        // refunded receipt (CancelSettlement = 2) from the database; null when the receipt was not refunded
        private async Task<RefundReceiptPrintModel?> GetRefundReceipt(StudentReceiptByIdRequest request)
        {
            using var con = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@GroupCode", request.GroupCode);
            param.Add("@BranchCode", request.BranchCode);
            param.Add("@ReceiptId", request.ReceiptId);

            using var multi = await con.QueryMultipleAsync("V3M_FIN_GetReceiptRefundDetails", param, commandType: CommandType.StoredProcedure);
            // the SP returns no result set at all for a receipt that was not refunded
            if (multi.IsConsumed)
                return null;

            var refund = await multi.ReadFirstOrDefaultAsync<RefundReceiptPrintModel>();
            if (refund == null)
                return null;
            refund.Lines = (await multi.ReadAsync<RefundReceiptPrintLine>()).ToList();
            return refund;
        }

        public async Task<byte[]?> GenerateRefundReceiptPdf(StudentReceiptByIdRequest request)
        {
            try
            {
                var refund = await GetRefundReceipt(request);
                return refund == null ? null : await _generateFile.GenerateRefundReceiptPdf(refund);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        // refund confirmation + refund receipt PDF to the parent's email; returns the masked email, null when there is no email
        public async Task<string?> SendRefundConfirmation(StudentReceiptByIdRequest request)
        {
            try
            {
                var refund = await GetRefundReceipt(request);
                if (refund == null || !IsValidEmail(refund.FatherEMail))
                    return null;

                var pdf = await _generateFile.GenerateRefundReceiptPdf(refund);
                await _otpSender.SendReceiptRefundConfirmationAsync(refund.FatherEMail!.Trim(), refund.StudentName ?? string.Empty,
                    refund.ReceiptNo ?? string.Empty, refund.RefundAmount, refund.RefundDate ?? DateTime.Now, pdf);
                return MaskEmail(refund.FatherEMail!);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        // SHA-256 of "ReceiptId:OTP" (hex) — the same OTP of another receipt gives another hash
        private static string? HashRefundOtp(long receiptId, string? otp)
        {
            if (string.IsNullOrWhiteSpace(otp))
                return null;
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{receiptId}:{otp.Trim()}"));
            return Convert.ToHexString(bytes);
        }

        private static bool IsValidEmail(string? email) =>
            !string.IsNullOrWhiteSpace(email) && MailAddress.TryCreate(email.Trim(), out _);

        private static string MaskEmail(string email)
        {
            var value = email.Trim();
            var at = value.IndexOf('@');
            if (at <= 0)
                return "****";
            var name = value[..at];
            var visible = name.Length <= 2 ? name[..1] : name[..2];
            return visible + new string('*', Math.Max(3, name.Length - visible.Length)) + value[at..];
        }

        private static string MaskMobile(string mobile)
        {
            var digits = new string(mobile.Where(char.IsDigit).ToArray());
            return digits.Length <= 4 ? "****" : new string('*', digits.Length - 4) + digits[^4..];
        }

        // three result sets: header, receipt lines, other pending dues of the student
        public async Task<StudentReceiptForEditResponse?> GetStudentReceiptForEdit(StudentReceiptByIdRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@ReceiptId", request.ReceiptId);

                using var multi = await con.QueryMultipleAsync(
                    "V3M_FIN_GetStudentReceiptForEdit",
                    param,
                    commandType: CommandType.StoredProcedure);

                var header = await multi.ReadFirstOrDefaultAsync<StudentReceiptEditHeader>();
                if (header == null)
                    return null;

                return new StudentReceiptForEditResponse
                {
                    Header = header,
                    Lines = (await multi.ReadAsync<StudentReceiptEditLine>()).ToList(),
                    PendingDues = (await multi.ReadAsync<StudentReceiptEditLine>()).ToList()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        // cancel & re-issue in one SP transaction; returns @Result (see V3M_FIN_EditStudentReceipt), SQL errors are thrown
        public async Task<EditStudentReceiptResponse> EditStudentReceipt(EditStudentReceiptRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                var lines = new DataTable();
                lines.Columns.Add("DetInvoiceId", typeof(long));
                lines.Columns.Add("PaidAmount", typeof(decimal));
                lines.Columns.Add("RemoveFromInvoice", typeof(bool));
                foreach (var line in request.Lines)
                    lines.Rows.Add(line.DetInvoiceId, line.PaidAmount, line.RemoveFromInvoice);

                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@ReceiptId", request.ReceiptId);
                param.Add("@PaymentMode", request.PaymentMode);
                param.Add("@ChequeNo", request.ChequeNo ?? string.Empty);
                param.Add("@ChequeDate", request.ChequeDate?.Date, DbType.Date);
                param.Add("@ChequeBank", request.ChequeBank ?? string.Empty);
                param.Add("@ChequeType", request.ChequeType ?? string.Empty);
                param.Add("@BankBranch", request.BankBranch ?? string.Empty);
                param.Add("@BankAccountId", request.BankAccountId);
                param.Add("@ReceiptDate", request.ReceiptDate?.Date, DbType.Date);
                param.Add("@Remark", request.Remark ?? string.Empty);
                param.Add("@EditReason", request.EditReason);
                param.Add("@DifferenceMode", request.DifferenceMode, DbType.Byte);
                param.Add("@EditedBy", request.EditedBy);
                param.Add("@Lines", lines.AsTableValuedParameter("dbo.EditReceiptLineType"));
                param.Add("@NewReceiptId", dbType: DbType.Int64, direction: ParameterDirection.Output);
                param.Add("@NewReceiptNo", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);
                param.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await con.ExecuteAsync(
                    "V3M_FIN_EditStudentReceipt",
                    param,
                    commandType: CommandType.StoredProcedure);

                return new EditStudentReceiptResponse
                {
                    Result = param.Get<int?>("@Result") ?? 0,
                    NewReceiptId = param.Get<long?>("@NewReceiptId") ?? 0,
                    NewReceiptNo = param.Get<string?>("@NewReceiptNo")
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
    }
}
