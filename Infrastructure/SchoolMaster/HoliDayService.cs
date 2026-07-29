using ApplicationInterface.SchoolMaster;
using Dapper;
using DomainModel.Admin;
using DomainModel.Enum;
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
    public class HoliDayService : IHolidayRepository
    {
        private readonly string _connectionString;

        public HoliDayService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async  Task<string> AddUpdateHoliday(HolidayModal objholidayModal)
        {
            try
            {
                string returnValue;
                var sqlQry = "V3M_InsertUpdate_Holidays";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@HolidayId", objholidayModal.HolidayId);
                        cmd.Parameters.AddWithValue("@HolidayName", objholidayModal.HolidayName);
                        cmd.Parameters.AddWithValue("@HolidayDate", objholidayModal.HolidayDate);
                        cmd.Parameters.AddWithValue("@HolidayEndDate", objholidayModal.HolidayEndDate);
                        cmd.Parameters.AddWithValue("@HolidayType", objholidayModal.HolidayType);
                        cmd.Parameters.AddWithValue("@CreatedBy", objholidayModal.CreatedBy);
                        cmd.Parameters.AddWithValue("@CreatedDate", objholidayModal.CreatedDate);
                        cmd.Parameters.AddWithValue("@AppliedOn", objholidayModal.AppliedOn);
                        cmd.Parameters.AddWithValue("@SessionId", objholidayModal.SessionId);
                        cmd.Parameters.AddWithValue("@Remarks", (object?)objholidayModal.Remarks ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@BranchCode", objholidayModal.BranchCode);
                        cmd.Parameters.AddWithValue("@GroupCode", objholidayModal.GroupCode);
                        cmd.Parameters.AddWithValue("@IsValid", objholidayModal.IsValid);
  
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

        public async Task<int> DeleteHoliday(int Id)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE MstHolidays SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE HolidayId = @HolidayId";
                return await con.ExecuteAsync(sql, new { HolidayId = Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<IEnumerable<HolidayModal>> GetAllHoliday()
        {
            try
            {
                using var db = new SqlConnection(_connectionString);
                string sql = "SELECT HolidayId,GroupCode,BranchCode,HolidayName,IsValid,SessionId,HolidayDate,HolidayEndDate,AppliedOn FROM MstHolidays with(nolock)";
                return await db.QueryAsync<HolidayModal>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
    }
}
