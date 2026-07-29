using ApplicationInterface.SchoolMaster;
using Dapper;
using DocumentFormat.OpenXml.Bibliography;
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
    public class DepartmentService : IDepartmentRepository
    {
        private readonly string _connectionString;

        public DepartmentService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async  Task<string> AddUpdateDepartment(DepartmentModel objdepartment)
        {
            try
            {
                string returnValue;
                var sqlQry = "V3M_InsertUpdate_Department";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@DepartmentId", objdepartment.DepartmentId);
                        cmd.Parameters.AddWithValue("@DepartmentName", objdepartment.DepartmentName);
                        cmd.Parameters.AddWithValue("@DepartmentCode", objdepartment.DepartmentCode);
                        cmd.Parameters.AddWithValue("@GroupCode", 01);
                        cmd.Parameters.AddWithValue("@IsValid", objdepartment.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", objdepartment.CreatedBy);
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

        public async Task<int> DeleteDepartmentAsync(int deprtId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE MstDepartment
                       SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE DepartmentId = @DepartmentId";

                return await con.ExecuteAsync(sql, new { DepartmentId = deprtId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<IEnumerable<DepartmentModel>> GetAllDepartmentAsync()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = " select DepartmentId,GroupCode,DepartmentCode,DepartmentName,Remarks,IsValid,CreatedBy,CreatedDate from MstDepartment";
                return await con.QueryAsync<DepartmentModel>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
    }
}
