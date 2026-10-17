using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SchoolEye.ApplicationInterface.Admin;


namespace SchoolEye.Infrastructure.Admin
{
    public class AdmitChildService : IAdmitChildRepository
    {
        private readonly string _connectionString;

    
        public AdmitChildService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async Task<AdmitChildResult> AdmitChild(AdmitChildsRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@RegistrationId", request.RegistrationId);
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@CreatedBy", request.CreatedBy);
                param.Add("@RouteDistance", request.RouteDistance);
                param.Add("@SocietyId", request.SocietyId);
                param.Add("@StudentNo", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);
                param.Add("@ControlNo", dbType: DbType.String, size: 20, direction: ParameterDirection.Output);
                param.Add("@StudentId", dbType: DbType.Int64, direction: ParameterDirection.Output);
                param.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
                await con.ExecuteAsync(
                    "V3M_FIN_UspAdmitChild",
                    param,
                    commandType: CommandType.StoredProcedure);
                return new AdmitChildResult
                {
                    ReturnValue = param.Get<int>("@ReturnValue"),
                    StudentId = param.Get<long?>("@StudentId"),
                    StudentNo = param.Get<string>("@StudentNo"),
                    ControlNo = param.Get<string>("@ControlNo")
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<RegistrationDetailsResponse> GetRegistrationDetails(RegistrationDetailsRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@RegistrationNo", request.RegistrationNo);
                param.Add("@StudentName", request.StudentName);
                param.Add("@SessionId", request.SessionId);
                var result= await con.QueryFirstOrDefaultAsync<RegistrationDetailsResponse>(
                    "Usp_GetRegistrationDetails",
                    param,
                    commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<IEnumerable<ClassMapTemplateFeeHeadResponse>> GetClassMapTemplateFeeHead(ClassMapTemplateFeeHeadRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@ClassCode", request.ClassCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@PeriodId", request.PeriodId);

                return await con.QueryAsync<ClassMapTemplateFeeHeadResponse>(
                    "V3M_FIN_GetClassMapTempateFeeHead",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<AdmitChildWithReceiptResponse> AdmitChildWithReceipt(AdmitChildWithReceiptRequest request)
        {
            var result = new AdmitChildWithReceiptResponse();
            await using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var tran = con.BeginTransaction();
            try
            {
                var admitParam = new DynamicParameters();
                admitParam.Add("@RegistrationId", request.RegistrationId);
                admitParam.Add("@GroupCode", request.GroupCode);
                admitParam.Add("@BranchCode", request.BranchCode);
                admitParam.Add("@SessionId", request.SessionId);
                admitParam.Add("@CreatedBy", request.CreatedBy);
                admitParam.Add("@RouteDistance", request.RouteDistance);
                admitParam.Add("@SocietyId", request.SocietyId);
                admitParam.Add("@StudentNo", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);
                admitParam.Add("@ControlNo", dbType: DbType.String, size: 20, direction: ParameterDirection.Output);
                admitParam.Add("@StudentId", dbType: DbType.Int64, direction: ParameterDirection.Output);
                admitParam.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
                await con.ExecuteAsync("V3M_FIN_UspAdmitChild", admitParam, tran, commandType: CommandType.StoredProcedure);

                result.StudentId = admitParam.Get<long?>("@StudentId") ?? 0;
                result.StudentNo = admitParam.Get<string>("@StudentNo");
                result.ControlNo = admitParam.Get<string>("@ControlNo");
                if (admitParam.Get<int>("@ReturnValue") != 1 || result.StudentId <= 0)
                {
                    RollbackQuietly(tran);
                    result.ReturnValue = 0;
                    return result;
                }

                var receiptParam = new DynamicParameters();
                receiptParam.Add("@StudentId", result.StudentId.ToString());
                receiptParam.Add("@GroupCode", request.GroupCode);
                receiptParam.Add("@BranchCode", request.BranchCode);
                receiptParam.Add("@CurrentSession", request.SessionId.ToString());
                receiptParam.Add("@AdmissionSession", request.SessionId.ToString());
                receiptParam.Add("@ClassCode", request.ClassCode);
                receiptParam.Add("@TotalAmount", request.TotalAmount);
                receiptParam.Add("@PaidAmount", request.PaidAmount);
                receiptParam.Add("@Concession", request.Concession);
                receiptParam.Add("@Balance", request.Balance);
                receiptParam.Add("@PaymentMode", request.PaymentMode ?? "");
                receiptParam.Add("@ChequeNo", request.ChequeNo ?? "");
                receiptParam.Add("@ChequeDate", request.ChequeDate, DbType.Date);
                receiptParam.Add("@ChequeBank", request.ChequeBank ?? "");
                receiptParam.Add("@LateFee", request.LateFee);
                receiptParam.Add("@PreviousBalance", request.PreviousBalance);
                receiptParam.Add("@Remark", request.Remark ?? "");
                receiptParam.Add("@IsAdmissionReceipt", 1);
                receiptParam.Add("@PeriodType", request.PeriodType ?? "");
                receiptParam.Add("@PeriodId", request.PeriodId.ToString());
                receiptParam.Add("@SocietyId", request.SocietyId);
                receiptParam.Add("@ChequeType", request.ChequeType ?? "");
                receiptParam.Add("@CreatedBy", request.CreatedBy);
                receiptParam.Add("@ReceiptDate", request.ReceiptDate, DbType.Date);
                receiptParam.Add("@StudentReceiptNo", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);
                receiptParam.Add("@ReceiptId", dbType: DbType.Int64, direction: ParameterDirection.Output);
                await con.ExecuteAsync("V3M_FIN_GeneratedStudentReceiptNo", receiptParam, tran, commandType: CommandType.StoredProcedure);

                result.StudentReceiptNo = receiptParam.Get<string>("@StudentReceiptNo");
                result.ReceiptId = receiptParam.Get<long?>("@ReceiptId") ?? 0;
                if (string.IsNullOrWhiteSpace(result.StudentReceiptNo) || result.ReceiptId <= 0)
                {
                    RollbackQuietly(tran);
                    result.ReturnValue = 2;
                    return result;
                }

                var table = new DataTable();
                table.Columns.Add("RowNo", typeof(int));
                table.Columns.Add("FeeHeadId", typeof(int));
                table.Columns.Add("MonthNo", typeof(int));
                table.Columns.Add("PayableAmount", typeof(decimal));
                table.Columns.Add("FeeHeadAmount", typeof(decimal));
                table.Columns.Add("ConcessionAmt", typeof(decimal));
                foreach (var item in request.FeeHeads)
                {
                    table.Rows.Add(item.RowNo, item.FeeHeadId, item.MonthNo, item.PayableAmount, item.FeeHeadAmount, item.ConcessionAmt);
                }

                var detailParam = new DynamicParameters();
                detailParam.Add("@GroupCode", request.GroupCode);
                detailParam.Add("@BranchCode", request.BranchCode);
                detailParam.Add("@StudentId", result.StudentId);
                detailParam.Add("@SessionId", request.SessionId);
                detailParam.Add("@AdmissionSession", request.SessionId.ToString());
                detailParam.Add("@IsBankProcess", request.IsBankProcess);
                detailParam.Add("@ClassCode", request.ClassCode);
                detailParam.Add("@ReceiptNo", result.StudentReceiptNo);
                detailParam.Add("@ReceiptId", result.ReceiptId);
                detailParam.Add("@TotalPaidAmount", request.PaidAmount);
                detailParam.Add("@CreatedBy", request.CreatedBy);
                detailParam.Add("@FeeHeads", table.AsTableValuedParameter("dbo.TVP_AdmissionReceiptFeeHead"));
                await con.ExecuteAsync("V3M_FIN_InsertAdmissionReceiptDetails", detailParam, tran, commandType: CommandType.StoredProcedure);

                if (request.ConcessionId > 0)
                {
                    var concessionFeeHead = string.Join("~", (request.ConcessionFeeHeads ?? new())
                        .Where(x => x.FeeHeadId > 0 && !string.IsNullOrWhiteSpace(x.ConcessionType))
                        .Select(x => $"{x.FeeHeadId}|{x.ConcessionType!.Trim()}|{decimal.Truncate(x.ConcessionValue).ToString(CultureInfo.InvariantCulture)}"));

                    var concessionParam = new DynamicParameters();
                    concessionParam.Add("@GroupCode", request.GroupCode);
                    concessionParam.Add("@BranchCode", request.BranchCode);
                    concessionParam.Add("@SessionId", request.SessionId);
                    concessionParam.Add("@ConcessionId", request.ConcessionId);
                    concessionParam.Add("@StudentId", result.StudentId);
                    concessionParam.Add("@ClassCode", request.ClassCode);
                    concessionParam.Add("@CreatedBy", request.CreatedBy);
                    concessionParam.Add("@ConcessionFeeHead", string.IsNullOrEmpty(concessionFeeHead) ? null : concessionFeeHead);
                    var concessionResult = await con.QueryFirstOrDefaultAsync<ConcessionMapResult>(
                        "V3M_FIN_UspMapStudentConcessionWithAdmission", concessionParam, tran, commandType: CommandType.StoredProcedure);

                    if (concessionResult == null || concessionResult.Status != 1)
                    {
                        Console.WriteLine($"Concession mapping failed: {concessionResult?.Message}");
                        RollbackQuietly(tran);
                        result.ReturnValue = 3;
                        return result;
                    }
                }

                tran.Commit();
                result.ReturnValue = 1;
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                RollbackQuietly(tran);
                throw;
            }
        }

        // result row of V3M_FIN_UspMapStudentConcessionWithAdmission
        private sealed class ConcessionMapResult
        {
            public int Status { get; set; }
            public string? Message { get; set; }
        }

        // a procedure's own ROLLBACK may already have ended the transaction
        private static void RollbackQuietly(SqlTransaction tran)
        {
            try
            {
                tran.Rollback();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Rollback: {ex.Message}");
            }
        }
    }
}
