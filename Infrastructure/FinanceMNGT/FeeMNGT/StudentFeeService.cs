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
using static DomainModel.FinanceMNGT.FeeCollectionDuesNew;

namespace Infrastructure.FinanceMNGT.FeeMNGT
{
    public class StudentFeeService:IStudentFeeRepository
    {
        private readonly string _connectionString;

        public StudentFeeService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
         ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
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
