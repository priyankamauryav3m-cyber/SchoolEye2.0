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
    public class CategoryService : ICategoryRepository
    {
        private readonly string _connectionString;

        public CategoryService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<RTECategoryModel>> GetAllAsync()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT CategoryId,GroupCode,BranchCode,CategoryName,DisplayOrder,IsValid,CreatedDate,CreatedBy FROM V3M_Mst_RTECategory ORDER BY CategoryId ASC";
                return await con.QueryAsync<RTECategoryModel>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<int> DeleteCategoryData(int categoryId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE V3M_Mst_RTECategory SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE CategoryId = @CategoryId";
                return await con.ExecuteAsync(sql, new { CategoryId = categoryId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<string> AddUpdateCategory(RTECategoryModel objCategory)
        {
            try
            {
                string returnValue;
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("V3M_InsertUpdate_RTECategory", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CategoryId", objCategory.CategoryId);
                        cmd.Parameters.AddWithValue("@GroupCode", objCategory.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", objCategory.BranchCode);
                        cmd.Parameters.AddWithValue("@CategoryName", objCategory.CategoryName);
                        cmd.Parameters.AddWithValue("@DisplayOrder", objCategory.DisplayOrder);
                        cmd.Parameters.AddWithValue("@IsValid", objCategory.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", objCategory.CreatedBy);
                        SqlParameter returnValueParam = new SqlParameter
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
                throw new Exception("Error while inserting/updating Category", ex);
            }
        }
    }
}