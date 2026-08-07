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
    public class StudentService : IStudentService
    {
        private readonly string _connectionString;

        public StudentService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        private IDbConnection Connection => new SqlConnection(_connectionString);

        public async Task<string> AddUpdateStudent(StudentModel objStudent)
        {
            using IDbConnection db = Connection;

            var parameters = new DynamicParameters();
            parameters.Add("@StudentId", objStudent.StudentId);
            parameters.Add("@StudentName", objStudent.StudentName);
            parameters.Add("@Gender", objStudent.Gender);
            parameters.Add("@DateOfBirth", objStudent.DateOfBirth);
            parameters.Add("@EmailAddress", objStudent.EmailAddress);
            parameters.Add("@CountryId", objStudent.CountryId);
            parameters.Add("@StateId", objStudent.StateId);
            parameters.Add("@CityId", objStudent.CityId);
            parameters.Add("@DisplayOrder", objStudent.DisplayOrder);
            parameters.Add("@PhotoPath", objStudent.PhotoPath);
            parameters.Add("@Output", dbType: DbType.String, direction: ParameterDirection.Output, size: 10);

            await db.ExecuteAsync(
                "USP_AddUpdateStudent",
                parameters,
                commandType: CommandType.StoredProcedure);

            return parameters.Get<string>("@Output");
        }

        public async Task<List<StudentModel>> GetStudent(int studentId = 0)
        {
            using IDbConnection db = Connection;

            var parameters = new DynamicParameters();
            parameters.Add("@StudentId", studentId);

            var result = await db.QueryAsync<StudentModel>(
                "USP_GetStudent",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<bool> DeleteStudent(int studentId, bool isActive)
        {
            using IDbConnection db = Connection;

            var parameters = new DynamicParameters();
            parameters.Add("@StudentId", studentId);
            parameters.Add("@IsActive", isActive);

            var rows = await db.ExecuteAsync(
                "USP_DeleteStudent",
                parameters,
                commandType: CommandType.StoredProcedure);

            return rows > 0;
        }
    }
}
