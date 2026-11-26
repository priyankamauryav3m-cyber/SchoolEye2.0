using ApplicationInterface.SchoolAdmin;
using ApplicationInterface.User;
using ClosedXML.Excel;
using Dapper;
using DomainModel.SchoolAdmin;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.SchoolAdmin
{
    public class SummerActivityService : ISummerActivity
    {
        private readonly string _connectionString;
        private readonly IUser _user;

        public SummerActivityService(IConfiguration configuration, IUser user)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
            _user = user;
            _registrationPageUrl = configuration.GetValue<string>("SummerActivity:RegistrationPageUrl")?.Trim();
            _useTinyUrl = configuration.GetValue<bool>("SummerActivity:UseTinyUrl");
        }

        private readonly string? _registrationPageUrl;
        // false: the link is the page url itself (testing); true: tiny url through V3MTokenLink (live)
        private readonly bool _useTinyUrl;

        // link of the registration page: only from the link icon (never from add / edit)
        public async Task<SummerActivityLinkResult> GenerateLinkAsync(SummerActivityLinkRequest request, string? createdBy)
        {
            using var con = new SqlConnection(_connectionString);

            // only an active activity of this group / branch / session
            var activity = (await GetSummerActivitiesAsync(new SummerActivitySearchModel
            {
                GroupCode = request.GroupCode,
                BranchCode = request.BranchCode,
                SessionId = request.SessionId,
                ActivityId = request.SAId,
                Valid = 1
            })).FirstOrDefault(x => x.SAId == request.SAId);
            if (activity == null)
                return new SummerActivityLinkResult { Code = -1 };

            // Blazor registration page (appsettings SummerActivity:RegistrationPageUrl)
            if (string.IsNullOrWhiteSpace(_registrationPageUrl)
                || !Uri.TryCreate(_registrationPageUrl, UriKind.Absolute, out var pageUri)
                || (pageUri.Scheme != Uri.UriSchemeHttps && pageUri.Scheme != Uri.UriSchemeHttp))
                return new SummerActivityLinkResult { Code = 2 };

            // same token as the old pages (G|B|Session|Activity, "+" -> "V3M"), url-encoded for the query string
            var token = _user.EncryptPassword($"{request.GroupCode}|{request.BranchCode}|{request.SessionId}|{request.SAId}", true)
                .Replace("+", "V3M");
            var targetUrl = _registrationPageUrl.TrimEnd('/') + "?key=" + Uri.EscapeDataString(token);

            if (!_useTinyUrl)
            {
                // testing: the page link itself is the activity's link (no tiny url / V3MTokenLink)
                var saved = await con.ExecuteAsync(
                    "UPDATE SummerActivities SET Link = @Link WHERE SAId = @SAId AND GroupCode = @GroupCode AND BranchCode = @BranchCode AND IsValid = 1",
                    new { Link = targetUrl, request.SAId, request.GroupCode, request.BranchCode });
                return saved > 0
                    ? new SummerActivityLinkResult { Code = 1, Link = targetUrl }
                    : new SummerActivityLinkResult { Code = -1 };
            }

            // live: tiny link (DefaultSettings TinyUrl) -> Alert_Usp_GenerateSummerActiviryLink (V3MTokenLink)
            var tinyUrl = (await GetBranchDefaultKeyValueAsync(con, request.GroupCode!, request.BranchCode!, "TinyUrl")).Trim();
            if (tinyUrl.Length == 0 || tinyUrl.LastIndexOf('?') <= 0)
                return new SummerActivityLinkResult { Code = 0 };

            var refCode = Guid.NewGuid().ToString().Substring(0, 8);
            var link = tinyUrl + refCode;

            var param = new DynamicParameters();
            param.Add("@GroupCode", request.GroupCode);
            param.Add("@BranchCode", request.BranchCode);
            param.Add("@SessionId", request.SessionId);
            param.Add("@ActivityId", request.SAId);
            param.Add("@TargetUrl", targetUrl);
            param.Add("@tinyUrl", link);
            param.Add("@RefCode", refCode);
            param.Add("@CreatedBy", createdBy);
            param.Add("@Status", dbType: DbType.String, size: 5, direction: ParameterDirection.Output);

            await con.ExecuteAsync("Alert_Usp_GenerateSummerActiviryLink", param, commandType: CommandType.StoredProcedure);
            return param.Get<string?>("@Status") == "1"
                ? new SummerActivityLinkResult { Code = 1, Link = link }
                : new SummerActivityLinkResult { Code = -99 };
        }

        private static async Task<string> GetBranchDefaultKeyValueAsync(SqlConnection con, string groupCode, string branchCode, string keyName) =>
            await con.ExecuteScalarAsync<string?>(
                "SELECT TOP 1 KeyValue FROM DefaultSettings WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND KeyName = @KeyName",
                new { GroupCode = groupCode, BranchCode = branchCode, KeyName = keyName }) ?? "";

        public async Task<byte[]> ExportSummerActivitiesAsync(SummerActivitySearchModel search)
        {
            var rows = (await GetSummerActivitiesAsync(search)).ToList();

            using var workbook = new XLWorkbook();
            var ws = workbook.AddWorksheet("Summer Activities");

            var headers = new[] { "S.No.", "Activity Name", "Start Date", "End Date", "Link", "Status" };
            ws.Range(1, 1, 1, headers.Length).Merge();
            ws.Cell(1, 1).Value = "Summer Activity";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (var c = 0; c < headers.Length; c++)
                ws.Cell(3, c + 1).Value = headers[c];
            var head = ws.Range(3, 1, 3, headers.Length);
            head.Style.Font.Bold = true;
            head.Style.Fill.BackgroundColor = XLColor.FromHtml("#DCE6F1");

            var r = 4;
            foreach (var row in rows)
            {
                ws.Cell(r, 1).Value = r - 3;
                ws.Cell(r, 2).Value = row.ActivityName ?? "";
                ws.Cell(r, 3).Value = row.StartDate?.ToString("dd-MM-yyyy") ?? "";
                ws.Cell(r, 4).Value = row.EndDate?.ToString("dd-MM-yyyy") ?? "";
                ws.Cell(r, 5).Value = row.Link ?? "";
                ws.Cell(r, 6).Value = row.IsValid ? "Active" : "Deactive";
                r++;
            }

            var table = ws.Range(3, 1, Math.Max(r - 1, 3), headers.Length);
            table.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            table.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<int> AddUpdateSummerActivityAsync(SummerActivityModel model)
        {
            using var con = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@GroupCode", model.GroupCode);
            param.Add("@BranchCode", model.BranchCode);
            param.Add("@SessionId", model.SessionId);
            param.Add("@SAId", model.SAId);
            param.Add("@ActivityName", model.ActivityName?.Trim());
            param.Add("@StartDate", model.StartDate?.Date);
            param.Add("@EndDate", model.EndDate?.Date);
            param.Add("@CreatedBy", model.CreatedBy);
            param.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await con.ExecuteAsync("USP_AddUpdateSummerActivities", param, commandType: CommandType.StoredProcedure);
            return param.Get<int?>("@Result") ?? -99;
        }

        public async Task<IEnumerable<SummerActivityModel>> GetSummerActivitiesAsync(SummerActivitySearchModel search)
        {
            using var con = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@Valid", search.Valid);
            param.Add("@ActivityId", search.ActivityId.ToString());
            param.Add("@GroupCode", search.GroupCode);
            param.Add("@BranchCode", search.BranchCode);
            param.Add("@SessionId", search.SessionId);

            return await con.QueryAsync<SummerActivityModel>("Usp_GetSummerActivitiesList", param, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> ActiveDeactiveSummerActivityAsync(SummerActivityStatusModel model)
        {
            using var con = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@GroupCode", model.GroupCode);
            param.Add("@BranchCode", model.BranchCode);
            param.Add("@SAId", model.SAId);
            param.Add("@IsValid", model.IsValid);
            param.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await con.ExecuteAsync("Usp_ActiveDeactiveSummerActivity", param, commandType: CommandType.StoredProcedure);
            return param.Get<int?>("@Result") ?? -1;
        }
    }
}
