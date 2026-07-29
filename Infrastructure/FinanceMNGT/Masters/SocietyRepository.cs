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
    public class SocietyRepository : ISocietyRepository
    {
        private readonly string _connectionString;
        public SocietyRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddUpdateSociety(SocietyModel society)
        {
            try
            {
                string returnValue;
                var sqlQry = "MNGT_InsertUpdate_Society";
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(sqlQry, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("SocietyId", society.SocietyId);
                        cmd.Parameters.AddWithValue("GroupCode", society.GroupCode);
                        cmd.Parameters.AddWithValue("BranchCode", society.BranchCode);
                        cmd.Parameters.AddWithValue("SocietyName", society.SocietyName);
                        cmd.Parameters.AddWithValue("SocietyCode", society.SocietyCode);
                        cmd.Parameters.AddWithValue("SocietyDesc", society.SocietyDesc);
                        cmd.Parameters.AddWithValue("BankProcessApl", society.BankProcessApl);
                        cmd.Parameters.AddWithValue("AddressLine1", society.AddressLine1);
                        cmd.Parameters.AddWithValue("AddressLine2", society.AddressLine2);
                        cmd.Parameters.AddWithValue("BankClientCode", society.BankClientCode);
                        cmd.Parameters.AddWithValue("OnlineGateway", society.OnlineGateway);
                        cmd.Parameters.AddWithValue("hashSequence", society.hashSequence);
                        cmd.Parameters.AddWithValue("URL", society.URL);
                        cmd.Parameters.AddWithValue("SALT", society.SALT);
                        cmd.Parameters.AddWithValue("MerchantID", society.MerchantID);
                        cmd.Parameters.AddWithValue("EncryptKey", society.EncryptKey);
                        cmd.Parameters.AddWithValue("VerifyURL", society.VerifyURL);
                        cmd.Parameters.AddWithValue("SuccessURL", society.SuccessURL);
                        cmd.Parameters.AddWithValue("FailureURL", society.FailureURL);
                        cmd.Parameters.AddWithValue("CancelURL", society.CancelURL);
                        cmd.Parameters.AddWithValue("RegFee", society.RegFee);
                        cmd.Parameters.AddWithValue("TallyURL", society.TallyURL);
                        cmd.Parameters.AddWithValue("TallyCompany", society.TallyURL);
                        cmd.Parameters.AddWithValue("IsSettlementProcess", society.IsSettlementProcess);
                        cmd.Parameters.AddWithValue("SettlementURL", society.SettlementURL);
                        cmd.Parameters.AddWithValue("GatewayType", society.GatewayType);
                        cmd.Parameters.AddWithValue("GatewayTypeURL", society.GatewayTypeURL);
                        cmd.Parameters.AddWithValue("IsTuitionFeeEditable", society.IsTuitionFeeEditable);
                        cmd.Parameters.AddWithValue("@IsValid", society.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", society.CreatedBy);
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

   
        public async Task<int> DeleteSociety(int sid)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE V3M_Mst_Society
                       SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE SocietyId = @SocietyId";
                return await con.ExecuteAsync(sql, new { SocietyId = sid });
            }
            catch (Exception)
            {
                throw;
            }
        }

        
        public async Task<IEnumerable<SocietyModel>> GetSociety()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "select * from V3M_Mst_Society with(nolock)";
                var result = await con.QueryAsync<SocietyModel>(sql);
                return result.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
