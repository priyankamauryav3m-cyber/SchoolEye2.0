using ApplicationInterface.FinanceMNGT;
using Dapper;
using DomainModel.FinanceMNGT;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Infrastructure.FinanceMNGT
{
    public class FeeCollectionPeriodConfigRepository : IFeeCollectionPeriodConfigRepository
    {
        private readonly string _connectionString;
        public FeeCollectionPeriodConfigRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddUpdateFeeCollectionPeriodConfig(FeeCollectionPeriodConfig feeCollectionPeriodConfig)
        {
            try
            {
                string returnValue;
                var sqlQry = "MNGT_InsertUpdate_FeeCollectionPeriod";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PeriodId", feeCollectionPeriodConfig.PeriodId);
                        cmd.Parameters.AddWithValue("@GroupCode", feeCollectionPeriodConfig.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", feeCollectionPeriodConfig.BranchCode);
                        cmd.Parameters.AddWithValue("@QuarterName", feeCollectionPeriodConfig.QuarterName);
                        cmd.Parameters.AddWithValue("@PeriodType", feeCollectionPeriodConfig.PeriodType);
                        cmd.Parameters.AddWithValue("@SessionId", feeCollectionPeriodConfig.SessionId);
                        cmd.Parameters.AddWithValue("@PeriodName", feeCollectionPeriodConfig.PeriodName);
                        cmd.Parameters.AddWithValue("@NoOfMonth", feeCollectionPeriodConfig.NoOfMonth);
                        cmd.Parameters.AddWithValue("@DueFrom", feeCollectionPeriodConfig.DueFrom);
                        cmd.Parameters.AddWithValue("@DueTo", feeCollectionPeriodConfig.DueTo);
                        cmd.Parameters.AddWithValue("@FeeDueDate", feeCollectionPeriodConfig.FeeDueDate);
                        cmd.Parameters.AddWithValue("@MonthNos", feeCollectionPeriodConfig.MonthNos);
                        cmd.Parameters.AddWithValue("@IsValid", feeCollectionPeriodConfig.IsValid);
                        cmd.Parameters.AddWithValue("@CretaedBy", feeCollectionPeriodConfig.CreatedBy);
                        var returnValueParam = new SqlParameter
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
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<int> DeleteFeeCollectionPeriodConfigData(int fid)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE V3M_FIN_Mst_FeeCollectionPeriodConfig SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END  WHERE PeriodId = @fid";
                return await con.ExecuteAsync(sql, new { PeriodId = fid });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<FeeCollectionPeriodConfig>> GetFeeCollectionPeriodConfigData(SearchAnyRequestModel requestModel)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", requestModel.GroupCode);
                parameters.Add("@BranchCode", requestModel.BranchCode);
                parameters.Add("@SessionId", requestModel.SessionId);
                parameters.Add("@PeriodType", requestModel.RequestName);
                var result = await con.QueryAsync<FeeCollectionPeriodConfig>(
                    "V3M_GetFeeCollectionPeriodConfig",
                    parameters,
                    commandType: CommandType.StoredProcedure    
                );
                return result.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> InsertLateFeeConfigration(LateFeeConfigration request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();

                parameters.Add("@GroupCode", request.GroupCode);
                parameters.Add("@BranchCode", request.BranchCode);
                parameters.Add("@SessionId", request.SessionId);
                parameters.Add("@PeriodId", request.PeriodId);
                parameters.Add("@StartDate", request.StartDate);
                parameters.Add("@EndDate", request.EndDate);
                parameters.Add("@Amount", request.Amount);
                parameters.Add("@ClassCode", request.ClassCode);
                parameters.Add("@CreatedBy", request.CreatedBy);
                parameters.Add("@PenaltyType", request.PenaltyType);
                parameters.Add("@MaxAmount", request.MaxAmount);
                parameters.Add(
                    "@ResultValue",
                    dbType: DbType.Int32,
                    direction: ParameterDirection.Output
                );

                await connection.ExecuteAsync(
                    "V3M_FIN_UspInsertLateFeeConfigration",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return parameters.Get<int>("@ResultValue");
            }
            catch (Exception)
            {
                return -1;
            }
        }
        public async Task<IEnumerable<LateFeeConfigData>> GetLateFeeConfigListData(LateFeeConfigData requestModel)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", requestModel.GroupCode);
                parameters.Add("@BranchCode", requestModel.BranchCode);
                parameters.Add("@SessionId", requestModel.SessionId);
                parameters.Add("@PeriodId", requestModel.PeriodId);
                parameters.Add("@ClassCode", requestModel.ClassCode);
                parameters.Add("@PenaltyType", requestModel.PenaltyType);

                var result = await con.QueryAsync<LateFeeConfigData>(
                    "USP_GetLateFeeConfigList",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching late fee config list", ex);
            }
        }
        public async Task<IEnumerable<LateFeeConfigData>> GetClassesListData(LateFeeConfigration requestModel)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", requestModel.GroupCode);
                parameters.Add("@BranchCode", requestModel.BranchCode);
                parameters.Add("@SessionId", requestModel.SessionId);
                parameters.Add("@PeriodId", requestModel.PeriodId);
                parameters.Add("@SDate", requestModel.StartDate);
                parameters.Add("@EDate", requestModel.EndDate);
                var result = await con.QueryAsync<LateFeeConfigData>(
                    "USP_GetClassesList",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching late fee Classes List", ex);
            }
        }
        public async Task<int> UpdateLateFeeDataData(LateFeeConfigData lateFee)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@Lid", lateFee.Lid);
                parameters.Add("@GroupCode", lateFee.GroupCode);
                parameters.Add("@BranchCode", lateFee.BranchCode);
                parameters.Add("@SessionId", lateFee.SessionId);
                parameters.Add("@Amount", lateFee.Amount);
                parameters.Add("@periodId", lateFee.PeriodId);
                parameters.Add("@ClassCode", lateFee.ClassCode);
                parameters.Add("@StartDate", lateFee.StartDate);
                parameters.Add("@EndDate", lateFee.EndDate);
                parameters.Add("@CreatedBy", lateFee.CreatedBy);
                parameters.Add(
                    "@ResultValue",
                    dbType: DbType.Int32,
                    direction: ParameterDirection.Output);
                await con.ExecuteAsync(
                    "V3M_FIN_UspUpdateLateFee",
                    parameters,
                    commandType: CommandType.StoredProcedure);
                return parameters.Get<int>("@ResultValue");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<int> ActivateDeactivateLateFeeConfig(ActivateModal request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@Lid", request.Lid);
                parameters.Add("@SessionId", request.SessionId);
                parameters.Add("@Status", request.Status);
                parameters.Add("@CreatedBy", request.CreatedBy);
                parameters.Add(
                    "@ReturnValue",
                    dbType: DbType.Int32,
                    direction: ParameterDirection.Output);
                await con.ExecuteAsync(
                    "V3M_UspActivateDeactivateLateFeeConfig",
                    parameters,
                    commandType: CommandType.StoredProcedure);
                return parameters.Get<int>("@ReturnValue");
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching  Activated late fee config", ex);
            }
        }

        public async Task<IEnumerable<PeriodMaster>> GetPeriodType()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT PeriodTypeId, PeriodTypeName FROM V3M_FIN_MstPeriodType";
                return await con.QueryAsync<PeriodMaster>(sql);
            }
            catch (Exception ex)
            {
                throw new Exception("Error while Period Type", ex);
            }
        }
        public async Task<IEnumerable<FeeCollectionMonthMappingModel>> GetQuarterlyMonthMapping(SearchAnyRequestModel request)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();

                    parameters.Add("@GroupCode", request.GroupCode);
                    parameters.Add("@BranchCode", request.BranchCode);
                    parameters.Add("@SessionId", request.SessionId);
                    parameters.Add("@PeriodType", request.RequestName);

                    var result = await connection.QueryAsync<FeeCollectionMonthMappingModel>(
                        "USP_GetQuarterlyMonthMapping",
                        parameters,
                        commandType: CommandType.StoredProcedure);

                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while Month name", ex);
            }
        }

    }
}
