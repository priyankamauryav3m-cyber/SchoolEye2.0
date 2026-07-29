using ApplicationInterface.FinanceMNGT.FeeMNGTMasters;
using Azure.Core;
using Dapper;
using DomainModel.FinanceMNGT;
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
    public class ViewStudentFeeHeadService: ViewStudentFeeHeadRepository
    {
        private readonly string _connectionString;
        public ViewStudentFeeHeadService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<List<MapwithFeehead>>  GetStudentMappedWithFeeHead(MapwithFeehead model)
        {
            using var con = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@GroupCode", model.GroupCode);
            parameters.Add("@BranchCode", model.BranchCode);
            parameters.Add("@SessionName", model.SessionName);
            parameters.Add("@FeeHeadId", model.FeeHeadId);
            parameters.Add("@ClassCode", model.ClassCode);
            parameters.Add("@SectionId", model.SectionId);
            parameters.Add("@Mode", model.Mode);
            parameters.Add("@StudentStatus", model.StudentStatus);
            parameters.Add("@EWSCategory", model.EWSCategory);
            return (await con.QueryAsync<MapwithFeehead>(
                "V3M_FIN_GetStudentMappedWithFeeHead",
                parameters,
                commandType: CommandType.StoredProcedure
            )).ToList();
        }
        public async Task<List<MapwithFeehead>> GetSearchedStudentData(MapwithFeehead model)
        {
            using var con = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@GroupCode", model.GroupCode);
            parameters.Add("@BranchCode", model.BranchCode);
            parameters.Add("@SessionName", model.SessionName);
            parameters.Add("@ClassCode", model.ClassCode);
            parameters.Add("@SectionCode", model.ClassSection);
            parameters.Add("@Gender", model.Gender);
            parameters.Add("@ControlNo", model.ControlNo);
            parameters.Add("@StudentName", model.StudentName);
            parameters.Add("@IsEWS", model.IsEWS);
            parameters.Add("@JoinType", model.JoinType);
            return (await con.QueryAsync<MapwithFeehead>(
                "V3M_STU_GetSearchedStudent",
                parameters,
                commandType: CommandType.StoredProcedure
            )).ToList();
        }

        public async Task<UnMapFeeHead> UnMapFeeHeadWithStudent(UnMapFeeHead model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", model.GroupCode);
                parameters.Add("@BranchCode", model.BranchCode);
                parameters.Add("@SessionName", model.SessionName);
                parameters.Add("@FeeHeadId", model.FeeHeadId);
                parameters.Add("@StudentId", model.StudentId);
                parameters.Add("@CreatedBy", model.CreatedBy);
                await con.ExecuteAsync(
                    "V3M_FIN_UnMapFeeHeadWithStudent",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
                model.Status = true;
                model.Message = "Fee head unmapped successfully.";
            }
            catch (Exception ex)
            {
                model.Status = false;
                model.Message = ex.Message;
            }
            return model;
        }

        public async Task<string> StudentCopyHeadData(StudenmapheadModal stu)
        {
            try
            {
                string returnValue;
                var sqlQry = "Usp_Fin_CopyFeeHeadMappedToStudent";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@GroupCode", stu.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", stu.BranchCode);
                        cmd.Parameters.AddWithValue("@SessionName", stu.SessionName);
                        cmd.Parameters.AddWithValue("@SId", stu.SId);
                        cmd.Parameters.AddWithValue("@CreatedBy", stu.CreatedBy);
                        var returnValueParam = new SqlParameter
                        {
                            ParameterName = "@Result",
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
    }
}


