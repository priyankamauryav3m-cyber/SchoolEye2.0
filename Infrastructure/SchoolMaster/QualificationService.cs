using ApplicationInterface.SchoolMaster;
using Dapper;
using DomainModel.SchoolMaster;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Infrastructure.SchoolMaster
{
    public class QualificationService : IQualification
    {
        private readonly string _connectionString;
        public QualificationService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                 ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async  Task<string> AddUpdateQualification(Qualification objqualification)
        {
            try
            {
                string returnValue;
                var sqlQry = "V3M_InsertUpdate_Qualification";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@QualificationId", objqualification.QualificationId);
                        cmd.Parameters.AddWithValue("@QualificationName", objqualification.QualificationName);
                        cmd.Parameters.AddWithValue("@QualificationTypeId", objqualification.QualificationTypeId);
                        cmd.Parameters.AddWithValue("@IsValid", objqualification.IsValid);
                        cmd.Parameters.AddWithValue("@Remarks",(object?) objqualification.Remarks ??DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreatedBy", objqualification.CreatedBy);
                        cmd.Parameters.AddWithValue("@CreatedDate", objqualification.CreatedDate);
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

        public async Task<int> DeleteQualification(int Id)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE MstQualification
                       SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE QualificationId = @QualificationId";
                return await con.ExecuteAsync(sql, new { QualificationId = Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting qualification: {ex.Message}");
                return 0; // or throw; depending on your requirement
            }
        }

        public async Task<IEnumerable<Qualification>> GetAllQualification()
        {
            try
            {
                using var db = new SqlConnection(_connectionString);
                string sql = "SELECT QualificationId,QualificationName,QualificationTypeId,Remarks,IsValid FROM MstQualification with(nolock)";
                return await db.QueryAsync<Qualification>(sql);
            }
            catch(Exception ex)
            {
                return Enumerable.Empty<Qualification>();
            }
        }
    }
}

