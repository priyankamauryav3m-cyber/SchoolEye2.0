using ApplicationInterface.FinanceMNGT;
using Dapper;
using DomainModel.FinanceMNGT;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Infrastructure.FeeMgt.Gaps
{
    public class OptionalFeeGroupRepository : IOptionalFeeGroupRepository
    {
        private readonly string _connectionString;

        public OptionalFeeGroupRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async Task<string> AddUpdateOptionalFeeGroup(OptionalFeeGroupModel model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var p = new DynamicParameters();
                p.Add("OptionalGroupId", model.OptionalGroupId);
                p.Add("GroupCode", model.GroupCode);
                p.Add("BranchCode", model.BranchCode);
                p.Add("SessionId", model.SessionId);
                p.Add("OptionalGroupName", model.OptionalGroupName);
                p.Add("OptionalGroupCode", model.OptionalGroupCode);
                p.Add("MaxFeeHeadCount", model.MaxFeeHeadCount);
                p.Add("OpenTillDate", model.OpenTillDate);
                p.Add("Remarks", model.Remarks);
                p.Add("CreatedBy", model.CreatedBy);
                p.Add("IsValid", model.IsValid);
                p.Add("ReturnValue", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);

                await con.ExecuteAsync("FEE_UspAddUpdateOptionalFeeGroup", p, commandType: CommandType.StoredProcedure);
                return p.Get<string>("ReturnValue");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<OptionalFeeGroupModel>> GetOptionalFeeGroupList(string groupCode, string branchCode, long sessionId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var p = new { GroupCode = groupCode, BranchCode = branchCode, SessionId = sessionId };
                var result = await con.QueryAsync<OptionalFeeGroupModel>(
                    "FEE_UspGetOptionalFeeGroupList", p, commandType: CommandType.StoredProcedure);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task ToggleOptionalFeeGroup(OptionalFeeGroupModel model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var p = new
                {
                    OptionalGroupId = model.OptionalGroupId,
                    GroupCode = model.GroupCode,
                    BranchCode = model.BranchCode,
                    SessionId = model.SessionId
                };
                await con.ExecuteAsync("FEE_UspToggleOptionalFeeGroup", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task MapClass(OptionalFeeGroupClassMapModel model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var p = new
                {
                    model.OptionalGroupId,
                    model.ClassCode,
                    model.CreatedBy
                };
                await con.ExecuteAsync("FEE_UspMapOptionalFeeGroupClass", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task MapFeeHead(OptionalFeeGroupHeadModel model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var p = new
                {
                    model.OptionalGroupId,
                    model.FeeHeadId,
                    model.Amount,
                    model.CreatedBy
                };
                await con.ExecuteAsync("FEE_UspMapOptionalFeeGroupHead", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
