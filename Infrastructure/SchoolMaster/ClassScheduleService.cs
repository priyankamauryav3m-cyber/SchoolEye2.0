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
    public class ClassScheduleService : IClassScheduleRepository
    {
        private readonly string _connectionString;

        public ClassScheduleService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }



        // 📋 Get All
        public async Task<IEnumerable<ClassSchedule>> GetAllAsync()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                var sql = "select ClassCode,GroupCode,BranchCode,SessionName,OffDayName,OffDayValue,CreatedBy,CreatedDate,HolidayType,OffClassDate from MstClassSchedule";
                return await con.QueryAsync<ClassSchedule>(sql);
            }
            catch (Exception)
            {
                // optionally log the exception here
                return Enumerable.Empty<ClassSchedule>();
            }
        }

        public async Task<int> InsertAsync(ClassSchedule model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                var sql = @"
        INSERT INTO MstClassSchedule
        (ClassCode, GroupCode, BranchCode, SessionName, OffDayName, OffDayValue,
         CreatedBy, CreatedDate, HolidayType, OffClassDate)
        VALUES
        (@ClassCode, @GroupCode, @BranchCode, @SessionName, @OffDayName, @OffDayValue,
         @CreatedBy, GETDATE(), @HolidayType, @OffClassDate)";

                return await con.ExecuteAsync(sql, model);
            }
            catch (Exception ex)
            {
                throw new Exception("Error inserting class schedule data", ex);
            }
        }

        public async Task<int> UpdateAsync(ClassSchedule model)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var sql = @"
                     UPDATE MstClassSchedule SET ClassCode = @ClassCode, GroupCode = @GroupCode, BranchCode = @BranchCode, SessionName = @SessionName, 
                    OffDayName = @OffDayName, OffDayValue = @OffDayValue, HolidayType = @HolidayType, OffClassDate = @OffClassDate WHERE Sid = @Sid";
                return await con.ExecuteAsync(sql, model);
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating class schedule data", ex);
            }
        }

        public async Task<int> DeleteAsync(int sid)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var sql = "UPDATE MstClassSchedule SET IsValid = 0 WHERE Sid = @Sid";
                return await con.ExecuteAsync(sql, new { Sid = sid });
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting class schedule data", ex);
            }
        }
    }
}
