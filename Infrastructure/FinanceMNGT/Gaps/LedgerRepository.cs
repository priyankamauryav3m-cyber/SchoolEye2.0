using ApplicationInterface.FinanceMNGT;
using Dapper;
using DomainModel.FinanceMNGT;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Infrastructure.FeeMgt.Gaps
{
    public class LedgerRepository : ILedgerRepository
    {
        private readonly string _connectionString;

        public LedgerRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async Task<IEnumerable<StudentLedgerEntryModel>> GetStudentLedger(StudentLedgerRequestModel request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var p = new
                {
                    request.GroupCode,
                    request.BranchCode,
                    request.SessionId,
                    request.StudentId
                };
                var result = await con.QueryAsync<StudentLedgerEntryModel>(
                    "FEE_UspGetStudentLedger", p, commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<FeeHeadLedgerEntryModel>> GetFeeHeadLedger(FeeHeadLedgerRequestModel request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var p = new
                {
                    request.GroupCode,
                    request.BranchCode,
                    request.SessionId,
                    request.FeeHeadId,
                    request.FromDate,
                    request.ToDate
                };
                var result = await con.QueryAsync<FeeHeadLedgerEntryModel>(
                    "FEE_UspGetFeeHeadLedger", p, commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
