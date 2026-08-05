using ApplicationInterface.SchoolMaster;
using Dapper;
using DomainModel.SchoolMaster;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Infrastructure.SchoolMaster
{
    public class SourceOfEnquiryRepository : ISourceOfEnquiryRepository
    {
        private readonly string _connectionString;

        public SourceOfEnquiryRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async Task<IEnumerable<SourceOfEnquiryModel>> GetAllAsync()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                string sql = @"SELECT
                                    SourceId,
                                    SourceName,
                                    IsValid,
                                    CreatedDate,
                                    CreatedBy
                               FROM Mst_SourceOfEnquiry
                               ORDER BY SourceId ASC";

                return await con.QueryAsync<SourceOfEnquiryModel>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<int> DeleteSourceOfEnquiry(int sourceId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                string sql = @"
                    UPDATE Mst_SourceOfEnquiry
                    SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                    WHERE SourceId = @SourceId";

                return await con.ExecuteAsync(sql, new { SourceId = sourceId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<string> AddUpdateSourceOfEnquiry(SourceOfEnquiryModel model)
        {
            try
            {
                string returnValue;

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using SqlCommand cmd = new SqlCommand("V3M_InsertUpdate_SourceOfEnquiry", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@SourceId", model.SourceId);
                cmd.Parameters.AddWithValue("@SourceName", model.SourceName);
                cmd.Parameters.AddWithValue("@IsValid", model.IsValid);
                cmd.Parameters.AddWithValue("@CreatedBy", (object?)model.CreatedBy ?? DBNull.Value);

                SqlParameter outputParam = new SqlParameter
                {
                    ParameterName = "@ReturnValue",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 50,
                    Direction = ParameterDirection.Output
                };

                cmd.Parameters.Add(outputParam);

                await cmd.ExecuteNonQueryAsync();

                returnValue = outputParam.Value?.ToString() ?? string.Empty;

                return returnValue;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while inserting/updating Source Of Enquiry.", ex);
            }
        }
    }
}