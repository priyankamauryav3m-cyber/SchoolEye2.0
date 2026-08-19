using ApplicationInterface.SchoolMaster;
using Dapper;
using DocumentFormat.OpenXml.EMMA;
using DomainModel.Admin;
using DomainModel.SchoolMaster;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.SchoolMaster
{
    public class InteractionPanelService : IInteractionPanelRepository
    {
        private readonly string _connectionString;

        public InteractionPanelService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<InteractionPanelModel>> GetAllAsync()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT PID,GroupCode,BranchCode,SessionId,PanelName,Remarks,IsValid,CreatedDate,CreatedBy FROM ADM_InteractionPanel ORDER BY PID ASC";
                return await con.QueryAsync<InteractionPanelModel>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<int> DeleteInteractionPanelData(int pid)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE ADM_InteractionPanel SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE PID = @PID";
                return await con.ExecuteAsync(sql, new { PID = pid });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<string> AddUpdateInteractionPanel(InteractionPanelModel objPanel)
        {
            try
            {
                string returnValue;
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("V3M_InsertUpdate_InteractionPanel", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PID", objPanel.PID);
                        cmd.Parameters.AddWithValue("@GroupCode", objPanel.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", objPanel.BranchCode);
                        cmd.Parameters.AddWithValue("@SessionId", objPanel.SessionId);
                        cmd.Parameters.AddWithValue("@PanelName", objPanel.PanelName);
                        cmd.Parameters.AddWithValue("@Remarks", (object)objPanel.Remarks ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsValid", objPanel.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", objPanel.CreatedBy);
                        SqlParameter returnValueParam = new SqlParameter
                        {
                            ParameterName = "@ReturnValue",
                            SqlDbType = SqlDbType.VarChar,
                            Size = 50,
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(returnValueParam);
                        await cmd.ExecuteNonQueryAsync();
                        returnValue = returnValueParam.Value?.ToString();
                    }
                }
                return returnValue;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while inserting/updating Interaction Panel", ex);
            }
        }
        public async Task<string> AddUpdateInteractionComments(InteractionCommentsModel model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@IID", model.IID);
                param.Add("@GroupCode", model.GroupCode);
                param.Add("@BranchCode", model.BranchCode);
                param.Add("@SessionId", model.SessionId);
                param.Add("@RegistrationId", model.RegistrationId);
                param.Add("@InteractionBy", model.InteractionBy);
                param.Add("@InteractionComments", model.InteractionComments);
                param.Add("@IsApproved", model.IsApproved);
                param.Add("@StarRating", model.StarRating);
                param.Add("@FileName", model.FileName);
                param.Add("@FilePath", model.FilePath);
                param.Add("@GeneralRemarks", model.GeneralRemarks);
                param.Add("@FinanceRemarks", model.FinanceRemarks);
                param.Add("@IsValid", model.IsValid);
                param.Add("@CreatedBy", model.CreatedBy);
                param.Add("@Recommendation ", model.Recommendation);
                param.Add("@Remarks", model.Remarks);
                param.Add("@PrincipalRemarks", model.RemarksPrincipal);
                param.Add("@FinanceDept", model.FinanceDept);
                param.Add("@ReturnValue", dbType: DbType.String, size: 50, direction: ParameterDirection.Output);

                await con.ExecuteAsync(
                    "USP_ADM_InteractionComments",
                    param,
                    commandType: CommandType.StoredProcedure);

                return param.Get<string>("@ReturnValue") ?? string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<EmployeeModel?> GetEmployeeList(EmployeeModel emp)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                var param = new DynamicParameters();
                param.Add("@GroupCode", emp.GroupCode);
                param.Add("@BranchCode", emp.BranchCode);

                return await con.QueryFirstOrDefaultAsync<EmployeeModel>(
                    "USP_GetEmployeeData",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

    }
}
