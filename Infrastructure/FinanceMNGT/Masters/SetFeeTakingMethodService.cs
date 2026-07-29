using ApplicationInterface.FinanceMNGT.FeeMNGTMasters;
using Azure.Core;
using Dapper;
using DocumentFormat.OpenXml.Spreadsheet;
using DomainModel.FinanceMNGT;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.FinanceMNGT.FeeMNGTMasters
{
    public class SetFeeTakingMethodService : ISetFeeTakingMethodRepository
    {
        private readonly string _connectionString;
        public SetFeeTakingMethodService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<dynamic>> GetFeeHeadsOfTemplateData(SearchAnyRequestModel request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@SessionId", request.SessionId);
                parameters.Add("@GroupCode", request.GroupCode);
                parameters.Add("@BranchCode", request.BranchCode);
                parameters.Add("@TemplateId", request.RequestId);
                var result = await con.QueryAsync(
                    "SetFeeTakingFeeHeadWithTemplate",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //using (SqlConnection connection = new SqlConnection(_connectionString))
        //{
        //    await connection.OpenAsync();

        //    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
        //    {
        //        cmd.CommandType = CommandType.StoredProcedure;

        //        cmd.Parameters.AddWithValue("@GroupCode", method.GroupCode);
        //        cmd.Parameters.AddWithValue("@BranchCode", method.BranchCode);
        //        cmd.Parameters.AddWithValue("@SessionName", method.SessionName);
        //        cmd.Parameters.AddWithValue("@MonthNo", method.MonthNo);
        //        cmd.Parameters.AddWithValue("@FeeTemplateID", method.FeeTemplateID);
        //        cmd.Parameters.AddWithValue("@FeeHeadId", method.FeeHeadId);
        //        cmd.Parameters.AddWithValue("@CreatedBy", method.CreatedBy);
        //        cmd.Parameters.AddWithValue("@IsValid", method.IsValid);
        //        cmd.Parameters.AddWithValue("@Amount", method.Amount);

        //        SqlParameter outputParam = new SqlParameter
        //        {
        //            ParameterName = "@Returnvalue",
        //            SqlDbType = SqlDbType.VarChar,
        //            Size = 10,
        //            Direction = ParameterDirection.Output
        //        };

        //        cmd.Parameters.Add(outputParam);

        //        await cmd.ExecuteNonQueryAsync();

        //        returnValue = outputParam.Value?.ToString();
        //    }
        //}

        //return returnValue;

        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Exception: {ex.Message}");
        //        throw;
        //    }
        //}

        public async Task<string> SaveFeeCollectionConfig(FeeTakingMethod method)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                var table = ConvertToDataTable(method.GridData);
                var parameters = new DynamicParameters();

                parameters.Add("@GroupCode", method.GroupCode);
                parameters.Add("@BranchCode", method.BranchCode);
                parameters.Add("@SessionId", method.SessionId);
                parameters.Add("@FeeTemplateID", method.FeeTemplateID);
                parameters.Add("@CreatedBy", method.CreatedBy);
                parameters.Add("@FeeData", table.AsTableValuedParameter("FeeCollectionType"));
                parameters.Add("@ReturnValue", dbType: DbType.String, size: 10, direction: ParameterDirection.Output);

                await connection.ExecuteAsync("V3M_FIN_SaveFeeTemplateCollectionConfig_Bulk",
                    parameters,commandType: CommandType.StoredProcedure );

                var result = parameters.Get<string>("@ReturnValue");
                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private DataTable ConvertToDataTable(List<JsonElement> gridData)
        {
            var dt = new DataTable();

            dt.Columns.Add("MonthNo", typeof(int));
            dt.Columns.Add("FeeHeadId", typeof(int));
            dt.Columns.Add("Amount", typeof(decimal));
            dt.Columns.Add("IsValid", typeof(bool));

            foreach (var row in gridData)
            {
                int feeHeadId = 0;

                if (row.TryGetProperty("FeeHeadId", out var feeHead))
                {
                    if (feeHead.ValueKind == JsonValueKind.Number)
                        feeHeadId = feeHead.GetInt32();
                    else
                        int.TryParse(feeHead.ToString(), out feeHeadId);
                }

                AddMonthIfExists(row, "January", 1, feeHeadId, dt);
                AddMonthIfExists(row, "February", 2, feeHeadId, dt);
                AddMonthIfExists(row, "March", 3, feeHeadId, dt);
                AddMonthIfExists(row, "April", 4, feeHeadId, dt);
                AddMonthIfExists(row, "May", 5, feeHeadId, dt);
                AddMonthIfExists(row, "June", 6, feeHeadId, dt);
                AddMonthIfExists(row, "July", 7, feeHeadId, dt);
                AddMonthIfExists(row, "August", 8, feeHeadId, dt);
                AddMonthIfExists(row, "September", 9, feeHeadId, dt);
                AddMonthIfExists(row, "October", 10, feeHeadId, dt);
                AddMonthIfExists(row, "November", 11, feeHeadId, dt);
                AddMonthIfExists(row, "December", 12, feeHeadId, dt);
            }

            return dt;
        }

        private void AddMonthIfExists(JsonElement row, string monthName, int monthNo, int feeHeadId, DataTable dt)
        {
            if (row.TryGetProperty(monthName, out var monthValue))
            {
                decimal amount = 0;

                var value = monthValue.ToString();

                if (!string.IsNullOrWhiteSpace(value))
                {
                    decimal.TryParse(value, out amount);
                }
                dt.Rows.Add(monthNo, feeHeadId, amount, true);
            }
        }
    }
}

