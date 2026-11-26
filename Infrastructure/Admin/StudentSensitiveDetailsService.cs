using ApplicationInterface.Admin;
using Dapper;
using DomainModel.Admin;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Admin
{
    // DPDP: Student Sensitive Details (Dapper -> stored procedures)
    public class StudentSensitiveDetailsService : IStudentSensitiveDetailsRepository
    {
        private readonly string _connectionString;

        public StudentSensitiveDetailsService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async Task<PiiAccessResult> CheckPiiAccessAsync(int userSid, string pageUrl, string groupCode, string branchCode)
        {
            using var con = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@UserSid", userSid);
            param.Add("@URL", pageUrl);
            param.Add("@GroupCode", groupCode);
            param.Add("@BranchCode", branchCode);

            var row = await con.QueryFirstOrDefaultAsync<PiiAccessResult>(
                "dbo.V3M_DPDP_CheckPiiAccess", param, commandType: CommandType.StoredProcedure);
            return row ?? new PiiAccessResult { HasAccess = false };
        }

        public async Task<StudentSensitiveDetailsResponse?> GetDetailsAsync(StudentSensitiveDetailsRequest request)
        {
            using var con = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@GroupCode", request.GroupCode);
            param.Add("@BranchCode", request.BranchCode);
            param.Add("@SessionId", request.SessionId);
            param.Add("@StudentId", request.StudentId);

            using var grid = await con.QueryMultipleAsync(
                "dbo.V3M_DPDP_GetStudentSensitiveDetails", param, commandType: CommandType.StoredProcedure);

            // the SP returns an empty header when the student is not in this group / branch
            var header = await grid.ReadFirstOrDefaultAsync<StudentSensitiveHeader>();
            if (header == null)
                return null;

            var fields = (await grid.ReadAsync<StudentSensitiveField>()).ToList();
            return new StudentSensitiveDetailsResponse { Header = header, Fields = fields };
        }

        public async Task<StudentSensitiveUpdateResult> UpdateAsync(StudentSensitiveUpdateRequest request, string updatedBy, string? ipAddress)
        {
            var changes = new DataTable();
            changes.Columns.Add("ColumnName", typeof(string));
            changes.Columns.Add("OldValue", typeof(string));
            changes.Columns.Add("NewValue", typeof(string));
            foreach (var c in request.Changes)
                changes.Rows.Add(c.FieldKey, DBNull.Value, (object?)c.NewValue ?? DBNull.Value);

            using var con = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@GroupCode", request.GroupCode);
            param.Add("@BranchCode", request.BranchCode);
            param.Add("@SessionId", request.SessionId);
            param.Add("@StudentId", request.StudentId);
            param.Add("@Changes", changes.AsTableValuedParameter("dbo.ColumnChangeType"));
            param.Add("@Reason", request.Reason.Trim());
            param.Add("@UpdatedBy", updatedBy);
            param.Add("@IP", ipAddress);
            param.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.Output);
            param.Add("@ChangedCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await con.ExecuteAsync("dbo.USP_DPDP_UpdateStudentSensitiveDetails", param, commandType: CommandType.StoredProcedure);

            return new StudentSensitiveUpdateResult
            {
                Result = param.Get<int?>("@Result") ?? 0,
                ChangedCount = param.Get<int?>("@ChangedCount") ?? 0
            };
        }

        public async Task<IEnumerable<StudentSensitiveHistoryRow>> GetHistoryAsync(StudentSensitiveDetailsRequest request)
        {
            using var con = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@GroupCode", request.GroupCode);
            param.Add("@BranchCode", request.BranchCode);
            param.Add("@StudentId", request.StudentId);
            param.Add("@FieldKey", string.IsNullOrWhiteSpace(request.FieldKey) ? null : request.FieldKey.Trim());

            return await con.QueryAsync<StudentSensitiveHistoryRow>(
                "dbo.V3M_DPDP_GetStudentSensitiveHistory", param, commandType: CommandType.StoredProcedure);
        }
    }
}
