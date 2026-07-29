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
    public class StateService : IStateRepository
    {
        private readonly string _connectionString;

        public StateService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
       public async Task<IEnumerable<StateModel>> GetAllAsync()
        {
            try
            {
                using var db = new SqlConnection(_connectionString);
                string sql = "SELECT * FROM MstState with(nolock) where IsValid=1";
                return await db.QueryAsync<StateModel>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<int> DeleteStateData(int stateId)
        {
            try
            {
                using var db = new SqlConnection(_connectionString);
                string sql = @"UPDATE MstState SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END WHERE StateId = @StateId";
                return await db.ExecuteAsync(sql, new { StateId = stateId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<string> AddUpdateState(StateModel objState)
        {
            try
            {
                string returnValue;
                var sqlQry = "V3M_InsertUpdate_State";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@StateId", objState.StateId);
                        cmd.Parameters.AddWithValue("@CountryId", objState.CountryId);
                        cmd.Parameters.AddWithValue("@StateName", objState.StateName);                        
                        cmd.Parameters.AddWithValue("@IsValid", objState.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", objState.CreatedBy);
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
