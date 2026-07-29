using ApplicationInterface.SchoolMaster;
using Dapper;
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
    public class ClassCodeService : IClassCodeRepository
    {
        private readonly string _connectionString;

        public ClassCodeService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
            ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<ClassCodeModel>> GetAllAsync()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT ClassId,ClassCode,Remarks,IsValid,CreatedDate,CreatedBy FROM MstClassCode with(nolock)";
                return await con.QueryAsync<ClassCodeModel>(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteAsync(int classId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "UPDATE MstClassCode SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END WHERE ClassId = @ClassId";
                return await con.ExecuteAsync(sql, new { ClassId = classId });
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<string> AddUpdateClasscode(ClassCodeModel objClasscode)
        {
            try
            {
                string returnValue;
                var sqlQry = "V3M_InsertUpdate_ClassCode";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ClassId", objClasscode.ClassId);
                        cmd.Parameters.AddWithValue("@ClassCode", objClasscode.ClassCode);
                        cmd.Parameters.AddWithValue("@Remarks", (object?) objClasscode.Remarks ?? DBNull.Value);                  
                        cmd.Parameters.AddWithValue("@IsValid", objClasscode.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", objClasscode.CreatedBy);
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
    }
}
