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
    public class MstClassService : IClassRepository
    {
        private readonly string _connectionString;
        public MstClassService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<int> DeleteClassData(int classId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE MstClass SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END WHERE ClassId = @ClassId";
                return await con.ExecuteAsync(sql, new { ClassId = classId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<IEnumerable<ClassModel>> GetClassData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT ClassId,GroupCode,ClassCode,ClassName,ClassOrder,IsValid,CreatedDate,CreatedBy FROM MstClass with(nolock)";
                return await con.QueryAsync<ClassModel>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<string> AddUpdateClass(ClassModel objClass)
        {
            try
            {
                string returnValue;
                var sqlQry = "V3M_InsertUpdate_Class";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ClassCode", objClass.ClassCode);
                        cmd.Parameters.AddWithValue("@ClassId", objClass.ClassId);
                        cmd.Parameters.AddWithValue("@ClassName", objClass.ClassName);
                        cmd.Parameters.AddWithValue("@ClassOrder", objClass.ClassOrder);
                        cmd.Parameters.AddWithValue("@GroupCode", 01);  
                        cmd.Parameters.AddWithValue("@IsValid", objClass.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", objClass.CreatedBy);
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
