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
    public class UpdateStudentCBSERegNoService : IUpdateStudentCBSERegNoRepository
    {
        private readonly string _connectionString;

        public UpdateStudentCBSERegNoService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async Task<IEnumerable<AdmSearchedStudentResponse>> GetStudentBoardRollNo(AdmSearchedStudentRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@ClassCode", request.ClassCode);
                param.Add("@SectionCode", request.SectionId);
                param.Add("@Gender", request.Gender);
                param.Add("@ControlNo", request.ControlNo);
                param.Add("@StudentName", request.StudentName);
                param.Add("@IsEWS", request.IsEWS);
                param.Add("@JoinType", request.JoinType);
                param.Add("@ValidStuStatus", request.ValidStuStatus);
                return await con.QueryAsync<AdmSearchedStudentResponse>("V3M_ADM_UspGetSearchedStudentForRollNo",param,commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        //public async Task<List<StudentCBSERegNoResult>> AddUpdateStudentCBSERegNo(UpdateStudentCBSERegNoRequest request)
        //{
        //    try
        //    {
        //        using var con = new SqlConnection(_connectionString);
        //        var param = new DynamicParameters();
        //        param.Add("@GroupCode", request.GroupCode);
        //        param.Add("@BranchCode", request.BranchCode);
        //        var packedStudentId = string.Join("~",request.Students.Select(s => $"{s.StudentId},{s.CBSERegNo}"));
        //        param.Add("@StudentId", packedStudentId);
        //        param.Add("@SessionId", request.SessionId);
        //        param.Add("@ClassCode", request.ClassCode);
        //        param.Add("@SectionId", request.SectionId);
        //        param.Add("@CreatedBy", request.CreatedBy);

        //        param.Add("@ReturnValue", dbType: DbType.String, size: -1, direction: ParameterDirection.Output); 

        //        await con.ExecuteAsync("V3M_STU_UspAddUpdateStudentCBSERegNo",param,commandType: CommandType.StoredProcedure);
        //        var packedResult = param.Get<string>("@ReturnValue") ?? string.Empty;
        //        var results = new List<StudentCBSERegNoResult>();
        //        foreach (var entry in packedResult.Split('~', StringSplitOptions.RemoveEmptyEntries))
        //        {
        //            var parts = entry.Split(',');
        //            if (parts.Length < 2 || !long.TryParse(parts[0], out var studentId))
        //                continue;

        //            results.Add(new StudentCBSERegNoResult
        //            {
        //                StudentId = studentId,
        //                IsUpdated = parts[1] == "1"
        //            });
        //        }

        //        return results;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Exception: {ex.Message}");
        //        throw;
        //    }
        //}
    }
}
