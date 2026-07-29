using ApplicationInterface.FinanceMNGT;
using Dapper;
using DomainModel.FinanceMNGT;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Infrastructure.FinanceMNGT
{
    public class ChequeTypeService : IChequeTypeRepository
    {
        private readonly string _connectionString;

        public ChequeTypeService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async Task<string> AddUpdateChecktype(ChequeTypeModel checktype)
        {
            try
            {
                string returnValue;
                var sqlQry = "MNGT_InsertUpdate_ChequeType";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@sid", checktype.Sid);
                        cmd.Parameters.AddWithValue("@ChequeTypeName", checktype.ChequeTypeName);
                        cmd.Parameters.AddWithValue("@DisplayOrder", checktype.DisplayOrder);
                        cmd.Parameters.AddWithValue("@GroupCode", checktype.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", checktype.BranchCode);
                        cmd.Parameters.AddWithValue("@IsValid", checktype.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", checktype.CreatedBy);
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
        public async Task<IEnumerable<ChequeTypeModel>> GetChecktypeData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "select * from V3M_FIN_MstChequeType with(nolock)";
                return await con.QueryAsync<ChequeTypeModel>(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteChecktypeData(int sid)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE V3M_FIN_MstChequeType
                SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                WHERE sid = @sid";
                return await con.ExecuteAsync(sql, new { sid = sid });
            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}
