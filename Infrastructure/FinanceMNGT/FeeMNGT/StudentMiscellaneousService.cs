using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using DomainModel.FinanceMNGT;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SchoolEye.ApplicationInterface.Admin;


namespace Infrastructure.FinanceMNGT.FeeMNGT
{
    public class StudentMiscellaneousService : IStudentMiscellaneousRepository
    {
        private readonly string _connectionString;

        public StudentMiscellaneousService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString") ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }


        public async Task<IEnumerable<MiscellaneousResponse>> GetStudentMiscellaneous(MiscellaneousRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@ClassCode", request.ClassCode);
                param.Add("@SectionCode", request.SectionCode);
                param.Add("@Gender", request.Gender);
                param.Add("@ControlNo", request.ControlNo);
                param.Add("@StudentName", request.StudentName);
                param.Add("@IsEWS", request.IsEWS);
                param.Add("@IsValid", request.IsValid);

                return await con.QueryAsync<MiscellaneousResponse>(
                    "Usp_GetStudentmiscellaneous",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }


        // one call for all students x fee heads; the SP runs everything in one transaction
        public async Task<IEnumerable<GenerateMiscChallanResponse>> GenerateMiscellaneousChallan(GenerateMiscChallanRequest request)
        {
            try
            {
                var studentTable = new DataTable();
                studentTable.Columns.Add("StudentId", typeof(long));
                studentTable.Columns.Add("StudentNo", typeof(string));
                studentTable.Columns.Add("ClassCode", typeof(string));
                foreach (var item in request.Students)
                {
                    studentTable.Rows.Add(item.StudentId, item.StudentNo, item.ClassCode);
                }

                var feeHeadTable = new DataTable();
                feeHeadTable.Columns.Add("FeeHeadId", typeof(int));
                feeHeadTable.Columns.Add("DisplayOrder", typeof(int));
                feeHeadTable.Columns.Add("MonthNo", typeof(int));
                feeHeadTable.Columns.Add("Amount", typeof(decimal));
                feeHeadTable.Columns.Add("Concession", typeof(decimal));
                foreach (var item in request.FeeHeads)
                {
                    feeHeadTable.Rows.Add(item.FeeHeadId, item.DisplayOrder, item.MonthNo, item.Amount, item.Concession);
                }

                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SocietyId", request.SocietyId);
                param.Add("@SessionId", request.SessionId);
                param.Add("@DueDate", request.DueDate.Date, DbType.Date);
                param.Add("@Narration", request.Narration);
                param.Add("@CreatedBy", request.CreatedBy);
                param.Add("@Students", studentTable.AsTableValuedParameter("dbo.V3M_FIN_UDT_MiscChallanStudent"));
                param.Add("@FeeHeads", feeHeadTable.AsTableValuedParameter("dbo.V3M_FIN_UDT_MiscChallanFeeHead"));

                return await con.QueryAsync<GenerateMiscChallanResponse>(
                    "V3M_FIN_GenerateMiscellaneousChallan",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<StudentByFeeTemplateResponse>> GetStudentByFeeTemplate(StudentByFeeTemplateRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@FeeTemplateId", request.FeeTemplateId);
                param.Add("@ControlNoList", request.ControlNoList);
                param.Add("@ClassCode", request.ClassCode);
                param.Add("@SectionId", request.SectionId);

                return await con.QueryAsync<StudentByFeeTemplateResponse>(
                    "USP_GetAllStudentByFeeTemplate",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        // all selected students x fee head months; the SP runs everything in one transaction
        public async Task<int> GenerateMiscChallanWithMonths(MiscChallanWithMonthsRequest request)
        {
            try
            {
                var feeHeadTable = new DataTable();
                feeHeadTable.Columns.Add("FeeHeadId", typeof(int));
                feeHeadTable.Columns.Add("MonthNo", typeof(int));
                feeHeadTable.Columns.Add("Amount", typeof(decimal));
                foreach (var item in request.FeeHeads)
                {
                    feeHeadTable.Rows.Add(item.FeeHeadId, item.MonthNo, item.Amount);
                }

                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@StudentIds", string.Join(",", request.StudentIds));
                param.Add("@FeeHeads", feeHeadTable.AsTableValuedParameter("dbo.V3M_FIN_FeeHeadMonthType"));
                param.Add("@FeeTemplateId", request.FeeTemplateId);
                param.Add("@DueDate", request.DueDate.Date, DbType.Date);
                param.Add("@Narration", request.Narration);
                param.Add("@CreatedBy", request.CreatedBy);
                param.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await con.ExecuteAsync(
                    "V3M_FIN_MiscellaniousChallanWithMonths",
                    param,
                    commandType: CommandType.StoredProcedure);

                return param.Get<int>("@Result");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        // one receipt for the student's miscellaneous fee heads; the SP runs everything in one transaction
        public async Task<GenerateMiscReceiptResponse> GenerateMiscellaneousReceipt(GenerateMiscReceiptRequest request)
        {
            try
            {
                var feeHeadTable = new DataTable();
                feeHeadTable.Columns.Add("FeeHeadId", typeof(int));
                feeHeadTable.Columns.Add("DisplayOrder", typeof(int));
                feeHeadTable.Columns.Add("MonthNo", typeof(int));
                feeHeadTable.Columns.Add("Amount", typeof(decimal));
                feeHeadTable.Columns.Add("Concession", typeof(decimal));
                foreach (var item in request.FeeHeads)
                {
                    feeHeadTable.Rows.Add(item.FeeHeadId, item.DisplayOrder, item.MonthNo, item.Amount, item.Concession);
                }

                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SocietyId", request.SocietyId);
                param.Add("@SessionId", request.SessionId);
                param.Add("@StudentId", request.StudentId);
                param.Add("@ClassCode", request.ClassCode);
                param.Add("@SectionId", request.SectionId);
                param.Add("@ReceiptDate", request.ReceiptDate.Date, DbType.Date);
                param.Add("@PaidAmount", request.PaidAmount);
                param.Add("@PaymentMode", request.PaymentMode);
                param.Add("@ChequeNo", request.ChequeNo ?? string.Empty);
                param.Add("@ChequeDate", request.ChequeDate?.Date, DbType.Date);
                param.Add("@ChequeBank", request.ChequeBank ?? string.Empty);
                param.Add("@Remark", request.Remark ?? string.Empty);
                param.Add("@CreatedBy", request.CreatedBy);
                param.Add("@FeeHeads", feeHeadTable.AsTableValuedParameter("dbo.V3M_FIN_UDT_MiscChallanFeeHead"));
                param.Add("@StudentReceiptNo", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);
                param.Add("@ReceiptId", dbType: DbType.Int64, direction: ParameterDirection.Output);

                await con.ExecuteAsync(
                    "V3M_FIN_GenerateMiscellaneousReceipt",
                    param,
                    commandType: CommandType.StoredProcedure);

                return new GenerateMiscReceiptResponse
                {
                    StudentReceiptNo = param.Get<string?>("@StudentReceiptNo"),
                    ReceiptId = param.Get<long?>("@ReceiptId") ?? 0
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<FeeHeadForComboResponse>> GetFeeHeadForCombo(FeeHeadForComboRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@FeeHeadType", request.FeeHeadType);
                param.Add("@IsMiscellaneous", request.IsMiscellaneous);
                return await con.QueryAsync<FeeHeadForComboResponse>(
                    "V3M_FIN_GetFeeHeadForCombo",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
    }
}
