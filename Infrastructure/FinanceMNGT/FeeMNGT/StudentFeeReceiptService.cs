using ApplicationInterface.FinanceMNGT.FeeMNGT;
using System;
using System.Collections.Generic;
using System.Data;
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

        public StudentFeeReceiptService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString") ??
                throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

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
                param.Add("@CancelType", request.CancelType);

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
            parameters.Add("@CancelType", request.CancelType);
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
    }
}
