using ApplicationInterface.GenerateFile;
using ApplicationInterface.SchoolAdmin;
using ApplicationInterface.User;
using Dapper;
using DomainModel.SchoolAdmin;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.SchoolAdmin
{
    public class SummerActivityRegistrationService : ISummerActivityRegistration
    {
        private const int MaxValues = 200;
        private const int MaxValueLength = 4000;
        // NVARCHAR(100) is the column of a string field without max length (USP_AddUpdateDynamicFormController2)
        private const int DefaultStringLength = 100;

        private readonly string _connectionString;
        private readonly IUser _user;
        private readonly IReportHeaderService _header;

        public SummerActivityRegistrationService(IConfiguration configuration, IUser user, IReportHeaderService header)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
            _user = user;
            _header = header;
        }

        private sealed record LinkKey(string GroupCode, string BranchCode, long SessionId, int SAId);

        private sealed class FieldRow
        {
            public string FieldName { get; set; } = "";
            public string? DisplayName { get; set; }
            public int? DisplayOrder { get; set; }
            public int? MaxLength { get; set; }
            public string? FieldType { get; set; }
            public string? DataType { get; set; }
            public string? OptionValues { get; set; }
            public bool? IsMandatory { get; set; }
        }

        // token of SummerActivityService.GenerateLinkAsync: GroupCode|BranchCode|SessionId|SAId
        private LinkKey? ReadKey(string? key)
        {
            if (string.IsNullOrWhiteSpace(key) || key.Length > 500)
                return null;
            try
            {
                var parts = _user.DecryptPassword(key.Trim(), true).Split('|');
                if (parts.Length != 4
                    || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1])
                    || !long.TryParse(parts[2], out var sessionId) || sessionId <= 0
                    || !int.TryParse(parts[3], out var saId) || saId <= 0)
                    return null;
                return new LinkKey(parts[0].Trim(), parts[1].Trim(), sessionId, saId);
            }
            catch
            {
                // not a token of ours
                return null;
            }
        }

        private static async Task<SummerActivityModel?> GetActivityAsync(SqlConnection con, LinkKey k) =>
            (await con.QueryAsync<SummerActivityModel>("Usp_GetSummerActivitiesList",
                new { Valid = 1, ActivityId = k.SAId.ToString(), k.GroupCode, k.BranchCode, k.SessionId },
                commandType: CommandType.StoredProcedure)).FirstOrDefault(x => x.SAId == k.SAId);

        // registration is open till the end date of the activity
        private static bool IsClosed(SummerActivityModel activity) =>
            activity.EndDate.HasValue && activity.EndDate.Value.Date < DateTime.Today;

        private static async Task<List<SummerActivitySlotOption>> GetSlotsAsync(SqlConnection con, LinkKey k) =>
            (await con.QueryAsync<SummerActivitySlotModel>("Usp_GetSummerActivitySlotList",
                new { Valid = 1, SoltName = "", k.GroupCode, k.BranchCode, k.SessionId, ActId = k.SAId.ToString() },
                commandType: CommandType.StoredProcedure))
            .Where(x => x.SAId == k.SAId)
            .Select(x => new SummerActivitySlotOption { SASId = x.SASId, SlotName = x.SlotName, FeeAmount = x.FeeAmount ?? 0 })
            .ToList();

        private static async Task<List<SummerActivityFormField>> GetFieldsAsync(SqlConnection con, LinkKey k)
        {
            var rows = await con.QueryAsync<FieldRow>(
                @"SELECT FieldName, DisplayName, DisplayOrder, MaxLength, FieldType, DataType, OptionValues, IsMandatory
                  FROM DynamicFormController
                  WHERE GroupCode = @GroupCode AND BranchCode = @BranchCode AND SAId = @SAId AND IsValid = 1
                  ORDER BY DisplayOrder, DFCId",
                new { k.GroupCode, k.BranchCode, k.SAId });

            return rows.Select(r => new SummerActivityFormField
            {
                FieldName = r.FieldName,
                DisplayName = string.IsNullOrWhiteSpace(r.DisplayName) ? r.FieldName : r.DisplayName,
                DisplayOrder = r.DisplayOrder ?? 0,
                MaxLength = r.MaxLength,
                FieldType = r.FieldType,
                DataType = r.DataType,
                Options = DynamicFormControllerModel.SplitOptions(r.OptionValues),
                IsMandatory = r.IsMandatory == true
            }).ToList();
        }

        public async Task<(int Code, SummerActivityFormInfo? Form)> GetFormAsync(string key)
        {
            var k = ReadKey(key);
            if (k == null)
                return (-6, null);

            using var con = new SqlConnection(_connectionString);
            var activity = await GetActivityAsync(con, k);
            if (activity == null)
                return (-1, null);
            if (IsClosed(activity))
                return (-2, null);

            var header = await _header.GetReportHeaderAsync(k.GroupCode, k.BranchCode);
            return (1, new SummerActivityFormInfo
            {
                SchoolName = header.SchoolName,
                Address = header.Address,
                Phone = header.Phone,
                Email = header.Email,
                Website = header.Website,
                ActivityName = activity.ActivityName,
                StartDate = activity.StartDate,
                EndDate = activity.EndDate,
                Slots = await GetSlotsAsync(con, k),
                Fields = await GetFieldsAsync(con, k)
            });
        }

        public async Task<SummerActivityRegistrationResult> RegisterAsync(SummerActivityRegistrationRequest request)
        {
            var result = new SummerActivityRegistrationResult();
            var k = ReadKey(request.Key);
            if (k == null)
            {
                result.Code = -6;
                return result;
            }
            if (request.Values != null && request.Values.Count > MaxValues)
            {
                result.Code = -5;
                return result;
            }

            using var con = new SqlConnection(_connectionString);
            var activity = await GetActivityAsync(con, k);
            if (activity == null)
            {
                result.Code = -1;
                return result;
            }
            if (IsClosed(activity))
            {
                result.Code = -2;
                return result;
            }

            var slot = (await GetSlotsAsync(con, k)).FirstOrDefault(x => x.SASId == request.SASId);
            if (slot == null)
            {
                result.Code = -3;
                result.Errors["SASId"] = "Please select a slot.";
                return result;
            }

            // only the active fields of this activity; anything else sent is ignored
            var values = new Dictionary<string, string?>(request.Values ?? new(), StringComparer.OrdinalIgnoreCase);
            var clean = new List<object>();
            foreach (var field in await GetFieldsAsync(con, k))
            {
                values.TryGetValue(field.FieldName, out var raw);
                var error = Normalize(field, raw, out var value);
                if (error != null)
                    result.Errors[field.FieldName] = error;
                else if (!string.IsNullOrEmpty(value))
                    clean.Add(new { field.FieldName, Value = value });
            }
            if (result.Errors.Count > 0)
            {
                result.Code = result.Errors.Values.Any(e => e.EndsWith(" is required.")) ? -4 : -5;
                return result;
            }

            var param = new DynamicParameters();
            param.Add("@GroupCode", k.GroupCode);
            param.Add("@BranchCode", k.BranchCode);
            param.Add("@SessionId", k.SessionId);
            param.Add("@SAId", k.SAId);
            param.Add("@SASId", slot.SASId);
            param.Add("@FieldValues", JsonSerializer.Serialize(clean));
            param.Add("@CreatedBy", "Online");
            param.Add("@OAId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            param.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await con.ExecuteAsync("Usp_SaveSummerActivityRegistration", param, commandType: CommandType.StoredProcedure);
            result.Code = param.Get<int?>("@Result") ?? -99;
            if (result.Code == 1)
            {
                result.RegistrationId = param.Get<int?>("@OAId");
                result.ActivityName = activity.ActivityName;
                result.SlotName = slot.SlotName;
                result.Amount = slot.FeeAmount;
            }
            return result;
        }

        // checks one value against its field; returns an error message or null (value = what is saved)
        private static string? Normalize(SummerActivityFormField field, string? raw, out string? value)
        {
            var name = field.DisplayName ?? field.FieldName;
            value = raw?.Trim();
            if (value != null && value.Length > MaxValueLength)
                return $"{name} is too long.";

            var type = field.FieldType?.Trim().ToLowerInvariant();
            var dataType = field.DataType?.Trim().ToLowerInvariant();

            // single tick (checkbox without options): only a ticked box counts as filled
            if (type == "checkbox" && field.Options.Count == 0)
            {
                var ticked = value is "1" or "true" or "True" or "yes" or "Yes";
                if (field.IsMandatory && !ticked)
                    return $"{name} is required.";
                value = dataType is "bit" or "int" or "long" or "decimal" or "double"
                    ? (ticked ? "1" : "0")
                    : (ticked ? "Yes" : "No");
                return null;
            }

            if (string.IsNullOrEmpty(value))
            {
                value = null;
                return field.IsMandatory ? $"{name} is required." : null;
            }

            // options: Dropdown / Radio one of them, Checkbox list any of them
            if (field.Options.Count > 0 && type is "dropdown" or "radio" or "checkbox")
            {
                var picked = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (type != "checkbox" && picked.Length != 1)
                    return $"Select one {name}.";
                var matched = new List<string>();
                foreach (var p in picked)
                {
                    var option = field.Options.FirstOrDefault(o => string.Equals(o, p, StringComparison.OrdinalIgnoreCase));
                    if (option == null)
                        return $"Invalid {name}.";
                    if (!matched.Contains(option))
                        matched.Add(option);
                }
                if (matched.Count == 0)
                    return field.IsMandatory ? $"{name} is required." : null;
                value = string.Join(",", matched);
            }

            switch (dataType)
            {
                case "int":
                    if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                        return $"Enter a valid number for {name}.";
                    break;
                case "long":
                    if (!long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                        return $"Enter a valid number for {name}.";
                    break;
                case "decimal":
                    if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) || Math.Abs(d) >= 100000000m)
                        return $"Enter a valid amount for {name}.";
                    value = d.ToString(CultureInfo.InvariantCulture);
                    break;
                case "double":
                    if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) || double.IsNaN(f) || double.IsInfinity(f))
                        return $"Enter a valid number for {name}.";
                    value = f.ToString(CultureInfo.InvariantCulture);
                    break;
                case "datetime":
                    if (!DateTime.TryParseExact(value, new[] { "yyyy-MM-dd", "dd-MM-yyyy", "dd/MM/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
                        || dt.Year < 1900 || dt.Year > 2100)
                        return $"Enter a valid date for {name}.";
                    value = dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    break;
                case "bit":
                    if (value is "1" or "true" or "True" or "yes" or "Yes") value = "1";
                    else if (value is "0" or "false" or "False" or "no" or "No") value = "0";
                    else return $"Invalid {name}.";
                    break;
                default:
                    var max = field.MaxLength > 0 ? field.MaxLength.Value : DefaultStringLength;
                    if (value.Length > max)
                        return $"{name} can be up to {max} characters.";
                    break;
            }
            return null;
        }
    }
}
