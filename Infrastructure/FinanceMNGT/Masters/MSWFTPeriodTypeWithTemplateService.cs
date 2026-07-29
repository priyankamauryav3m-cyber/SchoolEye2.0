using ApplicationInterface.FinanceMNGT.FeeMNGTMasters;
using Dapper;
using DomainModel.FinanceMNGT;
using DomainModel.SchoolMaster;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.FinanceMNGT.FeeMNGTMasters
{
    public class MSWFTPeriodTypeWithTemplateService: IMSWFTPeriodTypeWithTemplateRepository
    {
        private readonly string _connectionString;
        public MSWFTPeriodTypeWithTemplateService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<List<IMSWFTPeriodType>> GetIMSWFTPeriodTypeData(IMSWFTPeriodType model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", model.GroupCode);
                parameters.Add("@BranchCode", model.BranchCode);
                parameters.Add("@SessionId", model.SessionId);
                parameters.Add("@ClassCode", model.ClassCode);
                parameters.Add("@Section", model.SectionId);
                parameters.Add("@StudentName", model.StudentName);
                parameters.Add("@StudentNo", model.StudentNo);
                parameters.Add("@PeriodType", model.PeriodType);
                parameters.Add("@TemplateId", model.TemplateId);
                var result = await con.QueryAsync<IMSWFTPeriodType>(
                    "Usp_Fin_GetStudentForFeePeriodTypeMapping",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
     
        public async Task<bool> MapFeePeriodWithStudent(MapFeePeriodWithStudentModel model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", model.GroupCode ?? "");
                parameters.Add("@BranchCode", model.BranchCode ?? "");
                parameters.Add("@PeriodType", model.PeriodType ?? "");
                parameters.Add("@StudentId", model.StudentId);
                parameters.Add("@StudentNo", model.StudentNo);
                parameters.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.Output);
                await con.ExecuteAsync(
                    "Usp_MapFeePeriodWithStudent",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
                int result = parameters.Get<int>("@ReturnValue");
                return result == 1;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<List<SectionModel>> GetClassSection(SearchAnyRequestModel model)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", model.GroupCode ?? "");
                parameters.Add("@BranchCode", model.BranchCode ?? "");
                parameters.Add("@ClassCode", model.RequestName ?? "");
                var result = await connection.QueryAsync<SectionModel>(
                    "Usp_GetClassSection",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        public async Task<bool> MapFeeTemplateWithStudent(MapFeePeriodWithStudentModel model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", model.GroupCode ?? "");
                parameters.Add("@BranchCode", model.BranchCode ?? "");
                parameters.Add("@SessionId", model.SessionId);
                parameters.Add("@TemplateId", model.TemplateId ?? "");
                parameters.Add("@StudentId", model.StudentId);
                parameters.Add("@UpdatedBy", model.CreatedBy ?? "");
                parameters.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await con.ExecuteAsync(
                    "Usp_MapFeeTemplateWithStudent",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                int result = parameters.Get<int>("@ReturnValue");
                return result == 1;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
