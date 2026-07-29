using ApplicationInterface.SchoolMaster;
using Dapper;
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
    public class SubjectService: ISubjectRepository
    {
        private readonly string _connectionString;
        public SubjectService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddUpdateSubject(SubjectModel objsubject)
        {
            try
            {
                string returnValue;
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand("V3M_InsertUpdate_MstSubject", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@SubjectId", objsubject.SubjectId);
                        cmd.Parameters.AddWithValue("@SubjectName", objsubject.SubjectName);
                        cmd.Parameters.AddWithValue("@GroupCode", objsubject.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", objsubject.BranchCode);
                        cmd.Parameters.AddWithValue("@DepartmentId", objsubject.DepartmentId);
                        cmd.Parameters.AddWithValue("@SubjectCode", objsubject.SubjectCode);
                        cmd.Parameters.AddWithValue("@UGCCode", (object?)objsubject.UGCCode ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Abbreviation", (object?)objsubject.Abbreviation ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DisplayOrder", objsubject.DisplayOrder);
                        cmd.Parameters.AddWithValue("@Credit", objsubject.Credit);
                        cmd.Parameters.AddWithValue("@Remarks", (object?)objsubject.Remarks ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsValid", objsubject.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedDate", objsubject.CreatedDate);
                        cmd.Parameters.AddWithValue("@CreatedBy", objsubject.CreatedBy);
                        SqlParameter returnValueParam = new SqlParameter("@ReturnValue", SqlDbType.VarChar, 50)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(returnValueParam);

                        await cmd.ExecuteNonQueryAsync();
                        returnValue = returnValueParam.Value?.ToString() ?? string.Empty;
                    }
                }
                return returnValue;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to insert/update Subject", ex);
            }
        }


        public async Task<int> DeleteSubject(int SubjectId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE MstSubject SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END 
                       WHERE SubjectId = @SubjectId";
                return await con.ExecuteAsync(sql, new { SubjectId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<IEnumerable<SubjectModel>> GetSubject()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT SubjectId,GroupCode,BranchCode,DepartmentId,SubjectCode,UGCCode,SubjectName,Abbreviation,DisplayOrder,Remarks,IsValid,Credit FROM MstSubject with(nolock)";
                return await con.QueryAsync<SubjectModel>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
    }
}
