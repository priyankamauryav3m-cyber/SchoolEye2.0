using ApplicationInterface.FinanceMNGT;
using Dapper;
using DomainModel.FinanceMNGT;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Infrastructure.FeeMgt.Gaps
{
    public class JournalRepository : IJournalRepository
    {
        private readonly string _connectionString;

        public JournalRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async Task<string> AddUpdateAccountGroup(JournalAccountGroupModel model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var p = new DynamicParameters();
                p.Add("AccountGroupId", model.AccountGroupId);
                p.Add("GroupCode", model.GroupCode);
                p.Add("BranchCode", model.BranchCode);
                p.Add("AccountType", model.AccountType);
                p.Add("FirstGroup", model.FirstGroup);
                p.Add("SecondGroup", model.SecondGroup);
                p.Add("CreatedBy", model.CreatedBy);
                p.Add("ReturnValue", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);

                await con.ExecuteAsync("FEE_UspAddUpdateJournalAccountGroup", p, commandType: CommandType.StoredProcedure);
                return p.Get<string>("ReturnValue");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<JournalAccountGroupModel>> GetAccountGroupList(string groupCode, string branchCode)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var p = new { GroupCode = groupCode, BranchCode = branchCode };
                var result = await con.QueryAsync<JournalAccountGroupModel>(
                    "FEE_UspGetJournalAccountGroupList", p, commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> AddUpdateAccount(JournalAccountModel model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var p = new DynamicParameters();
                p.Add("AccountId", model.AccountId);
                p.Add("GroupCode", model.GroupCode);
                p.Add("BranchCode", model.BranchCode);
                p.Add("AccountGroupId", model.AccountGroupId);
                p.Add("AccountName", model.AccountName);
                p.Add("OpeningBalance", model.OpeningBalance);
                p.Add("OpeningBalanceType", model.OpeningBalanceType);
                p.Add("CreatedBy", model.CreatedBy);
                p.Add("ReturnValue", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);

                await con.ExecuteAsync("FEE_UspAddUpdateJournalAccount", p, commandType: CommandType.StoredProcedure);
                return p.Get<string>("ReturnValue");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<JournalAccountModel>> GetAccountList(string groupCode, string branchCode)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var p = new { GroupCode = groupCode, BranchCode = branchCode };
                var result = await con.QueryAsync<JournalAccountModel>(
                    "FEE_UspGetJournalAccountList", p, commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<(string ReturnValue, long VoucherId)> AddJournalVoucher(JournalVoucherModel model)
        {
            using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            using var tran = con.BeginTransaction();
            try
            {
                var masterParams = new DynamicParameters();
                masterParams.Add("GroupCode", model.GroupCode);
                masterParams.Add("BranchCode", model.BranchCode);
                masterParams.Add("SessionId", model.SessionId);
                masterParams.Add("VoucherNo", model.VoucherNo);
                masterParams.Add("VoucherDate", model.VoucherDate);
                masterParams.Add("TransactionType", model.TransactionType);
                masterParams.Add("Narration", model.Narration);
                masterParams.Add("TotalDebit", model.TotalDebit);
                masterParams.Add("TotalCredit", model.TotalCredit);
                masterParams.Add("CreatedBy", model.CreatedBy);
                masterParams.Add("VoucherId", dbType: DbType.Int64, direction: ParameterDirection.Output);
                masterParams.Add("ReturnValue", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);
                await con.ExecuteAsync("FEE_UspAddJournalVoucherMaster", masterParams,
                    commandType: CommandType.StoredProcedure, transaction: tran);
                var returnValue = masterParams.Get<string>("ReturnValue");
                var voucherId = masterParams.Get<long>("VoucherId");
                if (returnValue != "1")
                {
                    tran.Rollback();
                    return (returnValue, 0);
                }

                foreach (var line in model.Details)
                {
                    var detailParams = new
                    {
                        VoucherId = voucherId,
                        line.AccountId,
                        line.DrCr,
                        line.Amount,
                        line.Narration
                    };
                    await con.ExecuteAsync("FEE_UspAddJournalVoucherDetail", detailParams, commandType: CommandType.StoredProcedure, transaction: tran);
                }
                await con.ExecuteAsync("FEE_UspValidateJournalVoucherBalance",
                    new { VoucherId = voucherId }, commandType: CommandType.StoredProcedure, transaction: tran);
                tran.Commit();
                return (returnValue, voucherId);
            }
            catch (Exception)
            {
                tran.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<JournalVoucherModel>> GetJournalVoucherList(string groupCode, string branchCode, long sessionId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var p = new { GroupCode = groupCode, BranchCode = branchCode, SessionId = sessionId };
                var result = await con.QueryAsync<JournalVoucherModel>(
                    "FEE_UspGetJournalVoucherList", p, commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<JournalVoucherDetailModel>> GetJournalVoucherDetail(long voucherId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var result = await con.QueryAsync<JournalVoucherDetailModel>(
                    "FEE_UspGetJournalVoucherDetail", new { VoucherId = voucherId }, commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
