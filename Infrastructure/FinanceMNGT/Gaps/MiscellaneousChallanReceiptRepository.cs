using ApplicationInterface.FinanceMNGT;
using Dapper;
using DomainModel.FinanceMNGT;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Infrastructure.FeeMgt.Gaps
{
    public class MiscellaneousChallanReceiptRepository : IMiscellaneousChallanReceiptRepository
    {
        private readonly string _connectionString;

        public MiscellaneousChallanReceiptRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async Task<string> AddUpdateMiscChallan(MiscellaneousChallanModel model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var p = new DynamicParameters();
                p.Add("MiscChallanId", model.MiscChallanId);
                p.Add("GroupCode", model.GroupCode);
                p.Add("BranchCode", model.BranchCode);
                p.Add("SessionId", model.SessionId);
                p.Add("StudentId", model.StudentIdRef);
                p.Add("FeeHeadId", model.FeeHeadId);
                p.Add("ForMonth", model.ForMonth);
                p.Add("Amount", model.Amount);
                p.Add("DueDate", model.DueDate);
                p.Add("Remarks", model.Remarks);
                p.Add("CreatedBy", model.CreatedBy);
                p.Add("ReturnValue", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);

                await con.ExecuteAsync("FEE_UspAddUpdateMiscChallan", p, commandType: CommandType.StoredProcedure);
                return p.Get<string>("ReturnValue");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<MiscellaneousChallanModel>> GetMiscChallanList(string groupCode, string branchCode, long sessionId, long? studentId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var p = new { GroupCode = groupCode, BranchCode = branchCode, SessionId = sessionId, StudentId = studentId };
                var result = await con.QueryAsync<MiscellaneousChallanModel>(
                    "FEE_UspGetMiscChallanList", p, commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> AddMiscReceipt(MiscellaneousReceiptModel model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var p = new DynamicParameters();
                p.Add("GroupCode", model.GroupCode);
                p.Add("BranchCode", model.BranchCode);
                p.Add("SessionId", model.SessionId);
                p.Add("MiscChallanId", model.MiscChallanId);
                p.Add("StudentId", model.StudentIdRef);
                p.Add("FeeHeadId", model.FeeHeadId);
                p.Add("ReceiptNo", model.ReceiptNo);
                p.Add("Amount", model.Amount);
                p.Add("PaymentModeId", model.PaymentModeId);
                p.Add("Remarks", model.Remarks);
                p.Add("CreatedBy", model.CreatedBy);
                p.Add("ReturnValue", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);

                await con.ExecuteAsync("FEE_UspAddMiscReceipt", p, commandType: CommandType.StoredProcedure);
                return p.Get<string>("ReturnValue");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<MiscellaneousReceiptModel>> GetMiscReceiptList(string groupCode, string branchCode, long sessionId, long? studentId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var p = new { GroupCode = groupCode, BranchCode = branchCode, SessionId = sessionId, StudentId = studentId };
                var result = await con.QueryAsync<MiscellaneousReceiptModel>(
                    "FEE_UspGetMiscReceiptList", p, commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
