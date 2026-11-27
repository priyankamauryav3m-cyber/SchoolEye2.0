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
    public class DynamicFormControllerService : IDynamicFormController
    {
        private readonly string _connectionString;

        // system columns of OtherActivities: written by the registration / payment flow, never by a form field
        private static readonly HashSet<string> ReservedColumns = new(StringComparer.OrdinalIgnoreCase)
        {
            "OAId", "GroupCode", "BranchCode", "Activity", "SlotName", "Amount", "IsValid", "IsFeePaid",
            "BankTransacId", "RefNo", "TransacDate", "SettlementDate", "UTRNo", "PaymentMode",
            "CreatedDate", "CreatedBy", "SAId", "SASId"
        };

        public DynamicFormControllerService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async Task<int> AddUpdateFieldAsync(DynamicFormControllerModel model)
        {
            using var con = new SqlConnection(_connectionString);

            // the activity must be of this group / branch / session
            var activityOk = (await con.QueryAsync<int>("Usp_GetSummerActivitiesList",
                new { Valid = -1, ActivityId = model.SAId.ToString(), model.GroupCode, model.BranchCode, model.SessionId },
                commandType: CommandType.StoredProcedure)).Any();
            if (!activityOk)
                return -5;

            var fieldName = model.FieldName?.Trim();
            if (model.DFCId > 0)
            {
                // the SP alters the column named by @FieldName: always use the saved name of this row
                // (the name cannot be changed, and no other column can be altered through an update)
                fieldName = await con.ExecuteScalarAsync<string?>(
                    "SELECT FieldName FROM DynamicFormController WHERE DFCId = @DFCId AND GroupCode = @GroupCode AND BranchCode = @BranchCode",
                    new { model.DFCId, model.GroupCode, model.BranchCode });
                if (fieldName == null)
                    return -1;
            }
            else
            {
                // system columns of OtherActivities (id, activity, fee, payment) are not form fields.
                // Other existing columns (StudentName, Class, Gender, ...) are mapped to the field.
                if (fieldName != null && ReservedColumns.Contains(fieldName))
                    return -6;
            }

            var param = new DynamicParameters();
            param.Add("@GroupCode", model.GroupCode);
            param.Add("@BranchCode", model.BranchCode);
            param.Add("@DFCId", model.DFCId);
            param.Add("@FieldName", fieldName);
            param.Add("@DisplayName", model.DisplayName?.Trim());
            param.Add("@DisplayOrder", model.DisplayOrder);
            param.Add("@MaxLength", model.MaxLength);
            param.Add("@FieldType", model.FieldType);
            param.Add("@DataType", model.DataType);
            param.Add("@OptionValues", model.AllowsOptions && !string.IsNullOrWhiteSpace(model.OptionValues) ? string.Join(",", DynamicFormControllerModel.SplitOptions(model.OptionValues)) : null);
            param.Add("@IsMandatory", model.IsMandatory);
            param.Add("@CreatedBy", model.CreatedBy);
            param.Add("@SAId", model.SAId);
            param.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await con.ExecuteAsync("USP_AddUpdateDynamicFormController2", param, commandType: CommandType.StoredProcedure);
            return param.Get<int?>("@Result") ?? -99;
        }

        public async Task<IEnumerable<DynamicFormControllerModel>> GetFieldsAsync(DynamicFormControllerSearchModel search)
        {
            using var con = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@Valid", search.Valid);
            param.Add("@FieldName", search.FieldName?.Trim() ?? "");
            param.Add("@GroupCode", search.GroupCode);
            param.Add("@BranchCode", search.BranchCode);
            param.Add("@SessionId", search.SessionId);
            param.Add("@ActId", search.ActivityId.ToString());

            return await con.QueryAsync<DynamicFormControllerModel>("Usp_GetFieldNameList", param, commandType: CommandType.StoredProcedure);
        }

        public async Task<int> ActiveDeactiveFieldAsync(DynamicFormControllerStatusModel model)
        {
            using var con = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@GroupCode", model.GroupCode);
            param.Add("@BranchCode", model.BranchCode);
            param.Add("@DFCId", model.DFCId);
            param.Add("@IsValid", model.IsValid);
            param.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await con.ExecuteAsync("Usp_ActiveDeactiveDynamicFormController", param, commandType: CommandType.StoredProcedure);
            return param.Get<int?>("@Result") ?? -1;
        }
    }
}
