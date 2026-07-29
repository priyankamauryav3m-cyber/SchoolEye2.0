using ApplicationInterface.FinanceMNGT.FeeMNGT;
using Dapper;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.InkML;
using DomainModel.FinanceMNGT;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.FinanceMNGT.FeeMNGT
{
    public class StudentInvoiceService : IStudentInvoiceRepository
    {
        private readonly string _connectionString;
        public StudentInvoiceService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<StudentFeeInvoiceResponseModel>> GetStudentForInvoiceGenerate(StudentForInvoiceRequestModel requestModel)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", requestModel.GroupCode);
                parameters.Add("@BranchCode", requestModel.BranchCode);
                parameters.Add("@SessionId", requestModel.SessionId);
                parameters.Add("@ClassCode", requestModel.ClassCode);
                parameters.Add("@Section", requestModel.Section);
                parameters.Add("@StudentName", requestModel.StudentName);
                parameters.Add("@PeriodType", requestModel.PeriodType);
                parameters.Add("@PeriodId", requestModel.PeriodId);
                parameters.Add("@SocietyId", requestModel.SocietyId);
                parameters.Add("@StudentNo", requestModel.StudentNo);
                parameters.Add("@IsGenerated", requestModel.IsGenerated);
                parameters.Add("@StudentCategory", requestModel.StudentCategory);
                var result = await con.QueryAsync<StudentFeeInvoiceResponseModel>(
                "USP_GetStudentForFeeChallan",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching Promotion list", ex);
            }
        }
        public async Task<int> SaveStudentChallanGenerateData(StudentInvoice request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", request.GroupCode);
                parameters.Add("@BranchCode", request.BranchCode);
                parameters.Add("@SessionId", request.SessionId);
                parameters.Add("@SelectedClasses", request.ClassCode);
                parameters.Add("@PeriodIds", request.PeriodId);
                parameters.Add("@CreatedBy", request.CreatedBy);
                parameters.Add("@SocietyId", request.SocietyId);
                parameters.Add("@PeriodType", request.PeriodType);
                parameters.Add("@StudentIds", request.StudentId);
                parameters.Add("@InvoiceTypeId", request.InvoiceTypeId);
                parameters.Add("@InvoiceFor", request.InvoiceFor);
                var result = await con.ExecuteAsync("Usp_GenerateStudentChallanForAllClassWithTemplate", parameters, commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while generating challan.", ex);
            }
        }
        public async Task<List<InvoiceTypeModel>> GetInvoiceTypeList()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string query = @" SELECT InvoiceTypeId, InvoiceTypeName, IsValid, CreatedDate FROM V3M_FIN_MstInvoicetype";
                var result = await con.QueryAsync<InvoiceTypeModel>(query);
                return result.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IEnumerable<StudentClassModal>> GetStudentsByClassAsync(SearchAnyRequestModel searchAnyRequest)
        {
            try
            {
                using var db = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", searchAnyRequest.GroupCode);
                parameters.Add("@BranchCode", searchAnyRequest.BranchCode);
                parameters.Add("@SessionId", searchAnyRequest.SessionId);
                parameters.Add("@ClassCode", searchAnyRequest.RequestName);
                return await db.QueryAsync<StudentClassModal>(
                    "USP_GetStudentsByClass",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching Class list", ex);
            }
        }
        public async Task<IEnumerable<StudentDuesModel>> GetStudentInvoiceDuesData(StudentInvoiceDuesRequest request)
        {
            try
            {
                using var db = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", request.GroupCode);
                parameters.Add("@BranchCode", request.BranchCode);
                parameters.Add("@SessionId", request.SessionId);
                parameters.Add("@ClassCode", request.ClassCode);
                parameters.Add("@SectionId", request.SectionId);
                parameters.Add("@StudentNo", request.StudentNo);
                parameters.Add("@PeriodId", request.PeriodId);
                parameters.Add("@SocietyId", request.SocietyId);
                parameters.Add("@Status", request.Status);
                parameters.Add("@PeriodType", request.PeriodType);
                parameters.Add("@DuesAmountCheck", request.DuesAmountCheck);
                parameters.Add("@RoleName", request.RoleName);
                parameters.Add("@EmployeeId", request.EmployeeId);
                parameters.Add("@IsWithLateFee", request.IsWithLateFee);
                parameters.Add("@StudentCategory", request.StudentCategory);
                parameters.Add("@DuesAmount", request.DuesAmount);
                var result = await db.QueryAsync<StudentDuesModel>(
                    "USP_GetStudentInvoiceDues",
                    parameters,
                    commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database Error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: {ex.Message}", ex);
            }
        }
        public async Task<IEnumerable<InvoiceDetailsResponse>> GetInvoiceDetailsAsync(SearchAnyRequestModel RequestModel)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", RequestModel.GroupCode);
                param.Add("@BranchCode", RequestModel.BranchCode);
                param.Add("@InvoiceId", RequestModel.RequestName);
                param.Add("@InvType", RequestModel.RequestId);
                param.Add("@IsForBalPrint", RequestModel.IsActive);
                param.Add("@ValueType", RequestModel.RequestName2);
                using var multi = await con.QueryMultipleAsync("V3M_FIN_GetInvoiceMasterDetails", param, commandType: CommandType.StoredProcedure);
                var masterList = (await multi.ReadAsync<InvoiceMasterModel>()).ToList() ?? new();
                var feeHeadList = multi.IsConsumed ? new List<InvoiceFeeHeadModel>() : (await multi.ReadAsync<InvoiceFeeHeadModel>()).ToList();
                if (!masterList.Any())
                    return Enumerable.Empty<InvoiceDetailsResponse>();
                var responseList = masterList.GroupBy(m => m.InvoiceId).Select(g => new InvoiceDetailsResponse
                { InvoiceMaster = g.ToList(), FeeHeadList = feeHeadList.Where(f => f.InvoiceId == g.Key).ToList() }).ToList();
                return responseList;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database Error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: {ex.Message}", ex);
            }
        }
        public async Task<IEnumerable<SearchStudentBalanceDto>> GetStudentAdvanceBalanceData(SearchStudentBalanceDto request)
        {
            try
            {
                using var db = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", request.GroupCode);
                parameters.Add("@BranchCode", request.BranchCode);
                parameters.Add("@SessionId", request.SessionId);
                parameters.Add("@ClassCode", request.ClassCode);
                parameters.Add("@SectionId", request.SectionId);
                parameters.Add("@StudentNo", request.StudentNo);
                parameters.Add("@PeriodId", request.PeriodId);
                parameters.Add("@FeeTemplateId", request.TemplateId);
                parameters.Add("@PeriodType", request.PeriodType);
                parameters.Add("@InvType", request.InvType);
                var result = await db.QueryAsync<SearchStudentBalanceDto>(
                    "USP_FINGetStudentBalanceList",
                    parameters,
                    commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database Error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: {ex.Message}", ex);
            }
        }
        public async Task<int> StudentUpdateChallanDueDate(ChallanDueDateModal request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", request.GroupCode);
                parameters.Add("@BranchCode", request.BranchCode);
                parameters.Add("@InvoiceId", request.InvoiceId);
                parameters.Add("@DueDate", request.DueDate);
                parameters.Add("@UpdatedBy", request.UpdatedBy);
                parameters.Add("@Result",
                               dbType: DbType.String,
                               size: 5,
                               direction: ParameterDirection.Output);
                await con.ExecuteAsync("FIN_Usp_UpdateChallanDueDate", parameters,
                    commandType: CommandType.StoredProcedure);
                string result = parameters.Get<string>("@Result");

                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while updating challan due date.", ex);
            }
        }

        public async Task<IEnumerable<FeeHeadDropdownModel>> GetFeeHeadDropdown(SearchAnyRequestModel searchAnyRequest)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();

                param.Add("@GroupCode", searchAnyRequest.GroupCode);
                param.Add("@BranchCode", searchAnyRequest.BranchCode);
                param.Add("@SessionId", searchAnyRequest.SessionId);
                var result = await con.QueryAsync<FeeHeadDropdownModel>("USP_GetFeeCollectionConfig", param);
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while updating challan due date.", ex);
            }
        }
        public async Task<IEnumerable<TransportSelectMonthModel>> GetMonthWithTranspoet(SearchAnyRequestModel searchAnyRequest)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"SELECT * FROM TPT_FeeMonthConfig WHERE StudentId = @StudentId AND SessionId = @SessionId AND IsValid = 1";
                return await con.QueryAsync<TransportSelectMonthModel>(sql, new
                {
                    searchAnyRequest.StudentId,
                    searchAnyRequest.SessionId
                });
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> AddFeeHeadToStudentChallanData(FeeHeadToStudentChallan request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                await con.OpenAsync();
                var dt = new DataTable();
                dt.Columns.Add("MonthNo", typeof(int));
                foreach (var month in request.MonthNo) { dt.Rows.Add(month);   }
                var parameters = new DynamicParameters();
                parameters.Add("@StudentId", request.StudentId);
                parameters.Add("@InvoiceId", request.InvoiceId);
                parameters.Add("@HeadId", request.HeadId);
               // parameters.Add("@MonthNo", request.MonthNo);
                parameters.Add("@HeadAmount", request.HeadAmount);
                parameters.Add("@CreatedBy", request.CreatedBy);
                parameters.Add("@Narration1", request.Narration1);
                parameters.Add("@MonthNo", dt.AsTableValuedParameter("dbo.TVP_MonthNo"));
                parameters.Add( "@Result", dbType: DbType.Int32,direction: ParameterDirection.Output);
                await con.ExecuteAsync("FIN_Usp_AddFeeHeadToStudentChallan", parameters,commandType: CommandType.StoredProcedure);
                int? result = parameters.Get<int?>("@Result");
                return result ?? 0;
            }
            catch (SqlException ex)
            {
                throw new Exception("Database error while adding Fee Head to Student Challan.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while adding Fee Head to Student Challan.", ex);
            }
        }
        public async Task<int> RemoveFeeHeadToStudentChallanData(FeeHeadToStudentChallan request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                await con.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@StudentId", request.StudentId);
                parameters.Add("@InvoiceId", request.InvoiceId);
                parameters.Add("@HeadId", request.HeadId);
                parameters.Add("@MonthNo", request.SelectMonthNo);
                parameters.Add("@CreatedBy", request.CreatedBy);
                parameters.Add("@Narration1", request.Narration1);
                parameters.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.Output);
                await con.ExecuteAsync("FIN_Usp_RemoveFeeHeadFromChallan", parameters, commandType: CommandType.StoredProcedure);
                int? result = parameters.Get<int?>("@Result");
                return result ?? 0;
            }
            catch (SqlException ex)
            {
                throw new Exception("Database error while adding Fee Head to Student Challan.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while adding Fee Head to Student Challan.", ex);
            }
        }


    }
}
