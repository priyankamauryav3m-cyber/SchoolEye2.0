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
    public class SubjectCodeService:ISubjectCodeRepository
    {
        private readonly string _connectionString;
        public SubjectCodeService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddUpdateSubjectCode(SubjectCodeMaster objSubject)
        {
            try
            {
                string returnValue;
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand("V3M_InsertUpdate_SubjectCode", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Sid", objSubject.Sid);
                        cmd.Parameters.AddWithValue("@SubjectCode", objSubject.SubjectCode);
                        cmd.Parameters.AddWithValue("@Remarks",(object?) objSubject.Remarks ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsValid", objSubject.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedDate", objSubject.CreatedDate);
                        cmd.Parameters.AddWithValue("@CreatedBy", objSubject.CreatedDate);
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
                throw new Exception("Failed to insert/update Group", ex);
            }
        }


        public async Task<int> DeleteSubjectCode(int Sid)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE MstSubjectCode SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END 
                       WHERE Sid = @Sid";
                return await con.ExecuteAsync(sql, new { Sid });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<IEnumerable<SubjectCodeMaster>> GetSubjectCode()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT SubjectCode,Remarks,IsValid FROM MstSubjectCode with(nolock)";
                return await con.QueryAsync<SubjectCodeMaster>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

    }
}
