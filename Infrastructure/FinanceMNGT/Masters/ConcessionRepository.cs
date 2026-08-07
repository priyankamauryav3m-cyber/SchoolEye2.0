using ApplicationInterface.FinanceMNGT;
using Dapper;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Spreadsheet;
using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using DomainModel.SchoolMaster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.FinanceMNGT.FeeMNGTMasters
{
    public class ConcessionRepository : IConcessionRepository
    {
     
        private readonly string _connectionString;
        public ConcessionRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddUpdateConcession(ConcessionModel concession)
        {
            try
            {
                string returnValue;
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("MNGT_InsertUpdate_Concession", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ConcessionId", concession.ConcessionId);
                        cmd.Parameters.AddWithValue("@Concession", concession.Concession);
                        cmd.Parameters.AddWithValue("@Type", concession.Type);
                        cmd.Parameters.AddWithValue("@GroupCode", concession.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", concession.BranchCode);
                        cmd.Parameters.AddWithValue("@Remarks", concession.Remarks);
                        cmd.Parameters.AddWithValue("@CreatedBy", concession.CreatedBy);
                        cmd.Parameters.AddWithValue("@IsValid", concession.IsValid);
                        SqlParameter outputParam = new SqlParameter
                        {
                            ParameterName = "@ReturnValue",
                            SqlDbType = SqlDbType.VarChar,
                            Size = 50,
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outputParam);
                        await cmd.ExecuteNonQueryAsync();
                        returnValue = outputParam.Value?.ToString();
                    }
                }
                return returnValue;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<IEnumerable<ConcessionModel>> GetConcessionData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "select ConcessionId,GroupCode,BranchCode,Concession,Remarks,IsValid,CreatedBy,Type from V3M_FIN_MstConcession with(nolock)";
                return await con.QueryAsync<ConcessionModel>(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> DeleteConcessionData(int cid)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE V3M_FIN_MstConcession
                       SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE ConcessionId = @ConcessionId";
                return await con.ExecuteAsync(sql, new { ConcessionId = cid });
            }
            catch (Exception)
            {
                throw;  
            }
        }
        public async Task<IEnumerable<StudentWithConcessionDto>> GetStudentWithConcessionAsync(StudentConcessionFilterDto filter)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", filter.GroupCode);
                parameters.Add("@BranchCode", filter.BranchCode);
                parameters.Add("@SessionId", filter.SessionId);
                parameters.Add("@ClassCode", filter.ClassCode);
                parameters.Add("@SectionId", filter.SectionId);
                parameters.Add("@ConcessionId", filter.ConcessionId);
                parameters.Add("@Status", filter.ConStatus);
                parameters.Add("@StudentStatus", filter.StudentStatus);

                var result = await con.QueryAsync<StudentWithConcessionDto>(
                    "USP_GetStudentWithConcession",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
        public async Task<IEnumerable<StudentWithConcessionDto>> GetSearchStudent(SearchAnyRequestModel searchAny)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", searchAny.GroupCode);
                parameters.Add("@BranchCode", searchAny.BranchCode);
                parameters.Add("@SessionId", searchAny.SessionId);
                parameters.Add("@ControlNo", searchAny.RequestName);
                parameters.Add("@StudentName", searchAny.RequestName2);
                var result = await con.QueryAsync<StudentWithConcessionDto>(
                    "ADM_UspGetSearchedStudent",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
        public async Task<string> AddOrUpdateFeeheadConcessionData(List<ConcessionFeehead> model)
        {
            try
            {
                var tasks = model.Select(async item =>
                    {
                        using var connection = new SqlConnection(_connectionString);

                        var parameters = new DynamicParameters();
                        parameters.Add("@GroupCode", item.GroupCode);
                        parameters.Add("@BranchCode", item.BranchCode);
                        parameters.Add("@SessionId", item.SessionId);
                        parameters.Add("@ConcessionId", item.ConcessionId);
                        parameters.Add("@FeeHeadId", item.FeeHeadId);
                        parameters.Add("@ConcessionType", item.ConcessionType);
                        parameters.Add("@ConcessionValue", item.ConcessionValue);
                        parameters.Add("@CreatedBy", item.CreatedBy);
                        parameters.Add("@ReturnValue", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);

                        await connection.ExecuteAsync("dbo.USP_MapConcessionWithFeeHead", parameters, commandType: CommandType.StoredProcedure);

                        return parameters.Get<string>("@ReturnValue");
                    });

                var results = await Task.WhenAll(tasks);
                return string.Join(", ", results);
            }
            catch (Exception ex)
            {

                return $"ERROR: {ex.Message}";
            }
   
        }
       
        public async Task<string> SaveStudentConcession(StudentConcessionDto model)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();

                try
                {
                    var param = new DynamicParameters();

                    param.Add("@GroupCode", model.GroupCode);
                    param.Add("@BranchCode", model.BranchCode);
                    param.Add("@SessionId", model.SessionId);
                    param.Add("@StudentId", model.StudentId);
                    param.Add("@ClassCode", model.ClassCode);
                    param.Add("@Remarks", model.Remarks ?? "");
                    param.Add("@FromDate", model.FromDate);
                    param.Add("@ToDate", model.ToDate);
                    param.Add("@CreatedBy", model.CreatedBy);
                    param.Add("@ConcessionIds", string.Join(",", model.ConcessionIds));
                    param.Add("@FeeHeadJson", JsonConvert.SerializeObject(model.FeeHeadList));
                    param.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.Output);
                    await con.ExecuteAsync("V3M_FIN_SaveStudentConcession", param, commandType: CommandType.StoredProcedure);
                    var result= param.Get<int>("@ReturnValue");
                    return result.ToString();
                }
                catch(Exception ex)
                {
                    return $"ERROR: {ex.Message}";
                }
            }
        }
        public async Task<int> UpdateStudentConcessionRemarksData(StudentConcessionRemarks concession)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", concession.GroupCode);
                parameters.Add("@BranchCode", concession.BranchCode);
                parameters.Add("@studentId", concession.StudentId);
                parameters.Add("@concStudId", concession.ConcStudId);
                parameters.Add("@createdBy", concession.CreatedBy);
                parameters.Add("@remarks", concession.Remarks);
                parameters.Add("@ResultValue", dbType: DbType.Int32, direction: ParameterDirection.Output);
                await connection.ExecuteAsync(
                    "V3M_FIN_UspUpdateStudentConcessionRemarks",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
                return parameters.Get<int>("@ResultValue");
            }
            catch (Exception ex)
            {
                return -1;
            }
        }
        public async Task<int> ManageConcessionAsync(ConcessionManageRequest request)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@GroupCode", request.GroupCode);
                    parameters.Add("@BranchCode", request.BranchCode);
                    parameters.Add("@SessionId", request.SessionId);
                    parameters.Add("@StudentId", request.StudentId);
                    parameters.Add("@ConcStudId", request.ConcStudId);
                    parameters.Add("@CreatedBy", request.CreatedBy);
                    parameters.Add("@ApproveRemarks", request.ApproveRemarks);
                    parameters.Add("@ApproveBy", request.ApproveBy);

                    var result = await connection.QueryFirstOrDefaultAsync<int>(
                        "USP_ManageConcessionApproval",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while processing concession approval", ex);
            }
        }


        public async Task<List<StudentMappedConcessionDto>> GetStudentMappedConcession(SearchAnyRequestModel searchAnyRequest)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", searchAnyRequest.GroupCode);
                parameters.Add("@BranchCode", searchAnyRequest.BranchCode);
                parameters.Add("@StudentId", searchAnyRequest.RequestId);
                parameters.Add("@SessionId", searchAnyRequest.SessionId);

                var result = await connection.QueryAsync<StudentMappedConcessionDto>(
                    "USP_GetStudentMappedConcession",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
        }

        public async Task<int> UnMapConcessionWithStudentAsync(UnMapConcessionRequest request)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@GroupCode", request.GroupCode);
                    parameters.Add("@BranchCode", request.BranchCode);
                    parameters.Add("@SessionId", request.SessionId);
                    parameters.Add("@ConcessionId", request.ConcessionId);
                    parameters.Add("@StudentId", request.StudentId);
                    parameters.Add("@CreatedBy", request.CreatedBy);
                    parameters.Add("@ConcStudId", request.ConcStudId);
                    parameters.Add("@ApproveRemarks", request.ApproveRemarks);
                    parameters.Add("@ApproveBy", request.ApproveBy);
                    parameters.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        await connection.ExecuteAsync(
                        "USP_UnMapConcessionWithStudent",
                        parameters,
                        commandType: CommandType.StoredProcedure);
                    var result = parameters.Get<int>("@ReturnValue");
                    return result;
                }
            }
            catch (Exception ex)
            {
               throw new Exception("Error while unmapping concession.", ex);
            }
        }

    }
}
