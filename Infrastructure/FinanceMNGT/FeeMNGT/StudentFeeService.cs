using ApplicationInterface.FinanceMNGT.FeeMNGT;
using Dapper;
using DomainModel.FinanceMNGT;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MyApp.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.FinanceMNGT.FeeMNGT
{
    public class StudentFeeService:IStudentFeeRepository
    {
        private readonly string _connectionString;

        public StudentFeeService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")    ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async Task<AdjustStudentFeeHeadWiseResponse> AdjustStudentFeeHeadWise(AdjustStudentFeeHeadWiseRequest request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var table = new DataTable();
                table.Columns.Add("FeeHeadId", typeof(int));
                table.Columns.Add("PaidAmount", typeof(decimal));
                table.Columns.Add("LumpsumAmount", typeof(decimal));
                table.Columns.Add("MonthNoCsv", typeof(string));
                table.Columns.Add("FeeHeadAmount", typeof(decimal));
                table.Columns.Add("Concession", typeof(decimal));
                table.Columns.Add("Deduction", typeof(decimal));
                table.Columns.Add("StatusFlag", typeof(string));
                table.Columns.Add("DetInvoiceId", typeof(int));
                table.Columns.Add("AdjustAmount", typeof(decimal));

                foreach (var item in request.FeeHeadDetails)
                {
                    table.Rows.Add(
                        item.FeeHeadId, item.PaidAmount, item.LumpsumAmount, item.MonthNoCsv,
                        item.FeeHeadAmount, item.Concession, item.Deduction, item.StatusFlag,
                        item.DetInvoiceId, item.AdjustAmount);
                }

                var param = new DynamicParameters();
                param.Add("@StudentId", request.StudentId);
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@ClassCode", request.ClassCode);
                param.Add("@SectionId", request.SectionId);
                param.Add("@PaidAmount", request.PaidAmount);
                param.Add("@Balance", request.Balance);
                param.Add("@PaymentMode", request.PaymentMode);
                param.Add("@ChequeNo", request.ChequeNo);
                param.Add("@ChequeDate", request.ChequeDate);
                param.Add("@ChequeBank", request.ChequeBank);
                param.Add("@ChequeType", request.ChequeType);
                param.Add("@LateFee", request.LateFee);
                param.Add("@PreviousBalance", request.PreviousBalance);
                param.Add("@Remark", request.Remark);
                param.Add("@SocietyId", request.SocietyId);
                param.Add("@CreatedBy", request.CreatedBy);
                param.Add("@ReceiptDate", request.ReceiptDate);
                param.Add("@BankAccountId", request.BankAccountId);
                param.Add("@BankBranch", request.BankBranch);
                param.Add("@FeeHeadDetails", table.AsTableValuedParameter("dbo.FeeHeadDetailType"));
                param.Add("@StudentReceiptNo", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);
                param.Add("@ReceiptId", dbType: DbType.Int64, direction: ParameterDirection.Output);

                await connection.ExecuteAsync("V3M_FIN_InsertCounterReceiptDetails",
                      param,
                      commandType: CommandType.StoredProcedure);

                return new AdjustStudentFeeHeadWiseResponse
                {
                    StudentReceiptNo = param.Get<string>("@StudentReceiptNo"),
                    ReceiptId = param.Get<long>("@ReceiptId")
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while adjusting student fee head wise: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<StudentDetailsForFee>> GetStudentDetailsForFeeAsync(StudentDetailsForFeeRequest request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();

                parameters.Add("@ControlNo", request.ControlNo?.Trim() ?? "", DbType.String);
                parameters.Add("@StudentName", request.StudentName?.Trim() ?? "", DbType.String);
                parameters.Add("@SessionId", request.SessionId, DbType.String);
                parameters.Add("@GroupCode", request.GroupCode, DbType.String);
                parameters.Add("@BranchCode", request.BranchCode, DbType.String);
                parameters.Add("@ReceiptDate", request.ReceiptDate);
                parameters.Add("@UpToDate", request.UpToDate);

                var result = (await connection.QueryAsync<StudentDetailsForFee>(
                    "dbo.V3M_FIN_GetStudentDetailsForFee",
                    parameters,
                    commandType: CommandType.StoredProcedure
                )).ToList();
                return result;

             
            }
            catch (Exception ex)
            {
                return Enumerable.Empty<StudentDetailsForFee>();
            }
        }

        public async Task<IEnumerable<FeeCollectionDuesNew>> GetStudentDetailsForFeeDue(GetStudentFeeHeadDuesForAdjustmentRequest request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();

                parameters.Add("@GroupCode", request.GroupCode);
                parameters.Add("@BranchCode", request.BranchCode);  
                parameters.Add("@SessionId", request.SessionId);
                parameters.Add("@StudentId", request.StudentId);
                parameters.Add("@BankAccountId", request.BankAccountId);
                parameters.Add("@TillDate", request.TillDate);

                var result = (await connection.QueryAsync<FeeCollectionDuesNew>(
                    "dbo.V3M_FIN_UspGetStudentFeeHeadDuesForAdjustmentDemo",
                    parameters,
                    commandType: CommandType.StoredProcedure
                )).ToList();
                return result;


            }
            catch (Exception ex)
            {
                return Enumerable.Empty<FeeCollectionDuesNew>();
            }
        }
       

    }
}
