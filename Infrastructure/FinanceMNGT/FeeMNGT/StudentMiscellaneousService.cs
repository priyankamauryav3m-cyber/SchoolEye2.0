using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using DomainModel.FinanceMNGT;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SchoolEye.ApplicationInterface.Admin;

namespace SchoolEye.Infrastructure.Admin
{
    public class StudentMiscellaneousService : IStudentMiscellaneousRepository
    {
        private readonly string _connectionString;

        public StudentMiscellaneousService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString") ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }


        public async Task<IEnumerable<MiscellaneousResponse>> GetStudentMiscellaneous(MiscellaneousRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@ClassCode", request.ClassCode);
                param.Add("@SectionCode", request.SectionCode);
                param.Add("@Gender", request.Gender);
                param.Add("@ControlNo", request.ControlNo);
                param.Add("@StudentName", request.StudentName);
                param.Add("@IsEWS", request.IsEWS);
                param.Add("@IsValid", request.IsValid);

                return await con.QueryAsync<MiscellaneousResponse>(
                    "Usp_GetStudentmiscellaneous",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }


        // one call for all students x fee heads; the SP runs everything in one transaction
        public async Task<IEnumerable<GenerateMiscChallanResponse>> GenerateMiscellaneousChallan(GenerateMiscChallanRequest request)
        {
            try
            {
                var studentTable = new DataTable();
                studentTable.Columns.Add("StudentId", typeof(long));
                studentTable.Columns.Add("StudentNo", typeof(string));
                studentTable.Columns.Add("ClassCode", typeof(string));
                foreach (var item in request.Students)
                {
                    studentTable.Rows.Add(item.StudentId, item.StudentNo, item.ClassCode);
                }

                var feeHeadTable = new DataTable();
                feeHeadTable.Columns.Add("FeeHeadId", typeof(int));
                feeHeadTable.Columns.Add("DisplayOrder", typeof(int));
                feeHeadTable.Columns.Add("MonthNo", typeof(int));
                feeHeadTable.Columns.Add("Amount", typeof(decimal));
                feeHeadTable.Columns.Add("Concession", typeof(decimal));
                foreach (var item in request.FeeHeads)
                {
                    feeHeadTable.Rows.Add(item.FeeHeadId, item.DisplayOrder, item.MonthNo, item.Amount, item.Concession);
                }

                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SocietyId", request.SocietyId);
                param.Add("@SessionId", request.SessionId);
                param.Add("@DueDate", request.DueDate.Date, DbType.Date);
                param.Add("@Narration", request.Narration);
                param.Add("@CreatedBy", request.CreatedBy);
                param.Add("@Students", studentTable.AsTableValuedParameter("dbo.V3M_FIN_UDT_MiscChallanStudent"));
                param.Add("@FeeHeads", feeHeadTable.AsTableValuedParameter("dbo.V3M_FIN_UDT_MiscChallanFeeHead"));

                return await con.QueryAsync<GenerateMiscChallanResponse>(
                    "V3M_FIN_GenerateMiscellaneousChallan",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<FeeHeadForComboResponse>> GetFeeHeadForCombo(FeeHeadForComboRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@FeeHeadType", request.FeeHeadType);
                param.Add("@IsMiscellaneous", request.IsMiscellaneous);
                return await con.QueryAsync<FeeHeadForComboResponse>(
                    "V3M_FIN_GetFeeHeadForCombo",
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
