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
    public class OptionalFeeGroupMasterService : IOptionalFeeGroupMaster
    {
        private readonly string _connectionString;

        public OptionalFeeGroupMasterService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async Task<int> AddUpdateOptionalFeeGroupAsync(OptionalFeeGroupMasterModel model)
        {
            var name = model.ActivityGroupName?.Trim();

            // on update the save SP returns 2 even when no row matched: the group must be of this group / branch
            if (model.SeedId > 0)
            {
                var existing = await GetOptionalFeeGroupsAsync(new OptionalFeeGroupMasterSearchModel
                {
                    GroupCode = model.GroupCode,
                    BranchCode = model.BranchCode,
                    ActivityGroupName = "",
                    Valid = -1
                });
                if (!existing.Any(x => x.SeedId == model.SeedId))
                    return -1;
            }

            using var con = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@SeedId", model.SeedId);
            param.Add("@GroupCode", model.GroupCode, DbType.AnsiString, size: 10);
            param.Add("@BranchCode", model.BranchCode, DbType.AnsiString, size: 10);
            param.Add("@ActivityGroupName", name, DbType.AnsiString, size: 50);
            param.Add("@CreatedBy", model.CreatedBy, DbType.AnsiString, size: 50);
            param.Add("@Status", dbType: DbType.Int32, direction: ParameterDirection.Output);

            // @Status: 1 inserted, 2 updated, 3 name already exists
            await con.ExecuteAsync("FIN_USp_OptionalFeeGroup", param, commandType: CommandType.StoredProcedure);
            return param.Get<int?>("@Status") ?? -99;
        }

        public async Task<IEnumerable<OptionalFeeGroupMasterModel>> GetOptionalFeeGroupsAsync(OptionalFeeGroupMasterSearchModel search)
        {
            using var con = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@GroupCode", search.GroupCode);
            param.Add("@BranchCode", search.BranchCode);
            param.Add("@ActivityGroupName", search.ActivityGroupName?.Trim() ?? "");
            param.Add("@Valid", search.Valid);

            return await con.QueryAsync<OptionalFeeGroupMasterModel>("FIN_Usp_GetOptionalFeeGroupList", param, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<OptionalGroupFeeHeadMapRow>> GetMappingAsync(OptionalGroupMapSearchModel search)
        {
            using var con = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@GroupCode", search.GroupCode, DbType.AnsiString, size: 10);
            param.Add("@BranchCode", search.BranchCode, DbType.AnsiString, size: 10);
            param.Add("@OptFeeGroupId", search.OptFeeGroupId);

            return await con.QueryAsync<OptionalGroupFeeHeadMapRow>("USp_FIN_GetOptionalGroupFeeHeadMapping", param, commandType: CommandType.StoredProcedure);
        }

        public async Task<OptionalGroupMapSaveResult> SaveMappingAsync(OptionalGroupMapSaveModel model)
        {
            var result = new OptionalGroupMapSaveResult { Code = 1 };

            await using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            await using var tran = (SqlTransaction)await con.BeginTransactionAsync();
            try
            {
                foreach (var cls in model.Classes)
                {
                    var param = new DynamicParameters();
                    param.Add("@GroupCode", model.GroupCode, DbType.AnsiString, size: 10);
                    param.Add("@BranchCode", model.BranchCode, DbType.AnsiString, size: 10);
                    param.Add("@OptFeeGroupId", model.OptFeeGroupId);
                    param.Add("@ClassCode", cls.ClassCode?.Trim(), DbType.AnsiString, size: 20);
                    param.Add("@Status", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    string procedure;
                    if (cls.FeeHeads.Count == 0)
                    {
                        // no fee head left = unmap the class
                        param.Add("@UpdatedBy", model.CreatedBy, DbType.AnsiString, size: 50);
                        procedure = "USp_FIN_UnmapOptionalGroupClass";
                    }
                    else
                    {
                        var heads = new DataTable();
                        heads.Columns.Add("FeeHeadId", typeof(int));
                        heads.Columns.Add("Amount", typeof(decimal));
                        foreach (var h in cls.FeeHeads)
                            heads.Rows.Add(h.FeeHeadId, h.Amount);

                        param.Add("@TillDate", cls.TillDate?.Date, DbType.Date);
                        param.Add("@CreatedBy", model.CreatedBy, DbType.AnsiString, size: 50);
                        param.Add("@FeeHeads", heads.AsTableValuedParameter("dbo.FIN_OptionalGroupFeeHeadType"));
                        procedure = "USp_FIN_DetOptionalGroupFeeHeadMapping";
                    }

                    await con.ExecuteAsync(procedure, param, tran, commandType: CommandType.StoredProcedure);
                    var status = param.Get<int?>("@Status") ?? -99;

                    // map: 1 mapped / 2 updated; unmap: 1 unmapped / 0 was not mapped
                    if (status < 0)
                    {
                        await tran.RollbackAsync();
                        return new OptionalGroupMapSaveResult { Code = status, ClassName = cls.ClassName ?? cls.ClassCode };
                    }

                    if (cls.FeeHeads.Count == 0)
                        result.Unmapped += status == 1 ? 1 : 0;
                    else
                        result.Mapped++;
                }

                await tran.CommitAsync();
                return result;
            }
            catch
            {
                if (tran.Connection != null)
                    await tran.RollbackAsync();
                throw;
            }
        }

        public async Task<int> ActiveDeactiveOptionalFeeGroupAsync(OptionalFeeGroupMasterStatusModel model)
        {
            using var con = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@GroupCode", model.GroupCode);
            param.Add("@BranchCode", model.BranchCode);
            param.Add("@SeedId", model.SeedId);
            param.Add("@IsValid", model.IsValid);
            param.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await con.ExecuteAsync("FIN_Usp_ActiveDeactiveOptionalFeeGroup", param, commandType: CommandType.StoredProcedure);
            return param.Get<int?>("@Result") ?? -1;
        }
    }
}
