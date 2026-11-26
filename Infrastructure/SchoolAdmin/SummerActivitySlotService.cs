using ApplicationInterface.SchoolAdmin;
using Dapper;
using DomainModel.SchoolAdmin;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.SchoolAdmin
{
    public class SummerActivitySlotService : ISummerActivitySlot
    {
        private readonly string _connectionString;

        public SummerActivitySlotService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async Task<int> AddUpdateSlotAsync(SummerActivitySlotModel model)
        {
            using var con = new SqlConnection(_connectionString);

            // the save SP takes no group / branch: the activity (and, on update, the slot)
            // must belong to this group / branch / session
            var activityOk = (await con.QueryAsync<int>("Usp_GetSummerActivitiesList",
                new { Valid = -1, ActivityId = model.SAId.ToString(), model.GroupCode, model.BranchCode, model.SessionId },
                commandType: CommandType.StoredProcedure)).Any();
            if (!activityOk)
                return -2;

            if (model.SASId > 0)
            {
                var slots = await GetSlotsAsync(new SummerActivitySlotSearchModel
                {
                    GroupCode = model.GroupCode,
                    BranchCode = model.BranchCode,
                    SessionId = model.SessionId,
                    ActivityId = 0,
                    Valid = -1
                });
                if (!slots.Any(x => x.SASId == model.SASId))
                    return -2;
            }

            var param = new DynamicParameters();
            param.Add("@SASId", model.SASId);
            param.Add("@SAId", model.SAId);
            param.Add("@SlotName", model.SlotName?.Trim());
            param.Add("@FeeAmount", model.FeeAmount, DbType.Decimal, precision: 10, scale: 2);
            param.Add("@CreatedBy", model.CreatedBy);
            param.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await con.ExecuteAsync("USP_AddUpdateSummerActivitySlots", param, commandType: CommandType.StoredProcedure);
            return param.Get<int?>("@Result") ?? -99;
        }

        public async Task<IEnumerable<SummerActivitySlotModel>> GetSlotsAsync(SummerActivitySlotSearchModel search)
        {
            using var con = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@Valid", search.Valid);
            param.Add("@SoltName", search.SlotName?.Trim() ?? "");
            param.Add("@GroupCode", search.GroupCode);
            param.Add("@BranchCode", search.BranchCode);
            param.Add("@SessionId", search.SessionId);
            param.Add("@ActId", search.ActivityId.ToString());

            return await con.QueryAsync<SummerActivitySlotModel>("Usp_GetSummerActivitySlotList", param, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> ActiveDeactiveSlotAsync(SummerActivitySlotStatusModel model)
        {
            using var con = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@GroupCode", model.GroupCode);
            param.Add("@BranchCode", model.BranchCode);
            param.Add("@SASId", model.SASId);
            param.Add("@IsValid", model.IsValid);
            param.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await con.ExecuteAsync("Usp_ActiveDeactiveSummerActivitySlot", param, commandType: CommandType.StoredProcedure);
            return param.Get<int?>("@Result") ?? -1;
        }
    }
}
