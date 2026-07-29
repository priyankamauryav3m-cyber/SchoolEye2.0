using ApplicationInterface.Admin;
using ApplicationInterface.SuperAdmin;
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

namespace Infrastructure.SuperAdmin
{
    public class BookMarkService : IBookMarkRepository
    {
        private readonly string _connectionString;

        public BookMarkService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddOrUpdateBookMarksData(BookMarkModel objbook)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@BookMarkId", objbook.BookMarkId);
                parameters.Add("@BookMarkCaption", objbook.BookMarkCaption);
                parameters.Add("@Url", objbook.Url);
                parameters.Add("@Icon", objbook.Icon);
                parameters.Add("@IsValid", objbook.IsValid);
                parameters.Add("@CreatedDate", objbook.CreatedDate);
                parameters.Add("@CreatedBy", objbook.CreateBy);
                parameters.Add(
                    "@ReturnValue",
                    dbType: DbType.String,
                    size: 50,
                    direction: ParameterDirection.Output
                );
                await connection.ExecuteAsync(
                    "BM_BookMarksInsertUpdat",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
                return parameters.Get<string>("@ReturnValue");
            }
            catch (Exception ex)
            {
                throw new Exception("Error while saving bookmark", ex);
            }
        }

        public async Task<IEnumerable<BookMarkModel>> GetBookMarksData(string createdby)
        {
            try
            {
                using var db = new SqlConnection(_connectionString);
                string sql = @"SELECT *  FROM MstBookMarks WHERE IsValid = 1  AND CreatedBy = @CreatedBy";
                return await db.QueryAsync<BookMarkModel>(sql, new { CreatedBy = createdby });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<int> DeleteBookMarksData(int bookMarkId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"DELETE FROM MstBookMarks WHERE BookMarkId = @BookMarkId";
                return await con.ExecuteAsync(sql, new { BookMarkId = bookMarkId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
    }
}
