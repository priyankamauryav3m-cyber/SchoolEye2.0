
using ApplicationInterface.FinanceMNGT;
using Dapper;
using DomainModel.FinanceMNGT;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Infrastructure.FeeMgt.Gaps
{
    public class AdjustmentReceiptRepository : IAdjustmentReceiptRepository
    {
        private readonly string _connectionString;

        public AdjustmentReceiptRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async Task<string> AddAdjustmentReceipt(AdjustmentReceiptModel model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var p = new DynamicParameters();
                p.Add("GroupCode", model.GroupCode);
                p.Add("BranchCode", model.BranchCode);
                p.Add("SessionId", model.SessionId);
                p.Add("StudentId", model.StudentIdRef);
                p.Add("FeeHeadId", model.FeeHeadId);
                p.Add("AdjustmentType", model.AdjustmentType);
                p.Add("Amount", model.Amount);
                p.Add("Reason", model.Reason);
                p.Add("ApprovedBy", model.CreatedBy);
                p.Add("CreatedBy", model.CreatedBy);
                p.Add("ReturnValue", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);

                await con.ExecuteAsync("FEE_UspAddAdjustmentReceipt", p, commandType: CommandType.StoredProcedure);
                return p.Get<string>("ReturnValue");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> ApproveAdjustmentReceipt(long adjustmentId, string groupCode, string branchCode, long sessionId, string approvedBy)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var p = new DynamicParameters();
                p.Add("AdjustmentId", adjustmentId);
                p.Add("GroupCode", groupCode);
                p.Add("BranchCode", branchCode);
                p.Add("SessionId", sessionId);
                p.Add("ApprovedBy", approvedBy);
                p.Add("ReturnValue", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);

                await con.ExecuteAsync("FEE_UspApproveAdjustmentReceipt", p, commandType: CommandType.StoredProcedure);
                return p.Get<string>("ReturnValue");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<AdjustmentReceiptModel>> GetAdjustmentReceiptList(string groupCode, string branchCode, long sessionId, long? studentId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var p = new { GroupCode = groupCode, BranchCode = branchCode, SessionId = sessionId, StudentId = studentId };
                var result = await con.QueryAsync<AdjustmentReceiptModel>(
                    "FEE_UspGetAdjustmentReceiptList", p, commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
