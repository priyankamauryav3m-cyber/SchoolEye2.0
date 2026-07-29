using ApplicationInterface.FinanceMNGT;
using Dapper;
using DomainModel.FinanceMNGT;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Infrastructure.FinanceMNGT
{
    public class BankAccountService : IBankAccountRepository
    {
        private readonly string _connectionString;
        public BankAccountService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddUpdateBankAccount(BankAccountModel account)
        {
            try
            {
                string returnValue;
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("MNGT_InsertUpdate_BankAccount", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@DetBankAcId", account.DetBankAcId);
                        cmd.Parameters.AddWithValue("@GroupCode", account.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", account.BranchCode);
                        cmd.Parameters.AddWithValue("@BankId", account.BankId);
                        cmd.Parameters.AddWithValue("@SocietyId", account.SocietyId);
                        cmd.Parameters.AddWithValue("@AccountNo", account.AccountNo);
                        cmd.Parameters.AddWithValue("@DisplayName", account.DisplayName);
                        cmd.Parameters.AddWithValue("@AccountType", account.AccountType);
                        cmd.Parameters.AddWithValue("@Freeze", account.Freeze);
                        cmd.Parameters.AddWithValue("@ForFee", account.ForFee);
                        cmd.Parameters.AddWithValue("@ForInventory", account.ForInventory);
                        cmd.Parameters.AddWithValue("@ForSalary", account.ForSalary);
                        cmd.Parameters.AddWithValue("@ForOthers", account.ForOthers);
                        cmd.Parameters.AddWithValue("@OpeningBalance", account.OpeningBalance);
                        cmd.Parameters.AddWithValue("@AvailableBalance", account.AvailableBalance);
                        cmd.Parameters.AddWithValue("@CommitedAmount", account.CommitedAmount);
                        cmd.Parameters.AddWithValue("@Signatory1", account.Signatory1);
                        cmd.Parameters.AddWithValue("@IsSig1Mandatory", account.IsSig1Mandatory);
                        cmd.Parameters.AddWithValue("@Signatory2", account.Signatory2);
                        cmd.Parameters.AddWithValue("@IsSig2Mandatory", account.IsSig2Mandatory);
                        cmd.Parameters.AddWithValue("@Signatory3", account.Signatory3);
                        cmd.Parameters.AddWithValue("@IsSig3Mandatory", account.IsSig3Mandatory);
                        cmd.Parameters.AddWithValue("@Signatory4", account.Signatory4);
                        cmd.Parameters.AddWithValue("@IsSig4Mandatory", account.IsSig4Mandatory);
                        cmd.Parameters.AddWithValue("@Signatory5", account.Signatory5);
                        cmd.Parameters.AddWithValue("@IsSig5Mandatory", account.IsSig5Mandatory);
                        cmd.Parameters.AddWithValue("@IsValid", account.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", account.CreatedBy);
                        cmd.Parameters.AddWithValue("@CreatedDate", account.CreatedDate);
                        cmd.Parameters.AddWithValue("@LastTransDate", account.LastTransDate);
                        cmd.Parameters.AddWithValue("@LedgerId", account.LedgerId);
                        SqlParameter outputParam = new SqlParameter
                        {
                            ParameterName = "@ReturnValue",
                            SqlDbType = SqlDbType.VarChar,
                            Size = 50,
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outputParam);

                        await cmd.ExecuteNonQueryAsync();
                        returnValue = outputParam.Value?.ToString();
                    }
                }
                return returnValue;
            }
            catch (Exception ex)
            {
              
                throw;
            }
        }

  

        public async Task<IEnumerable<BankAccountModel>> GetBankAccountData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "select * from V3M_FIN_MstBankAccount with(nolock)";
                return await con.QueryAsync<BankAccountModel>(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<int> DeleteBankAccountData(int detBankAcId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE V3M_FIN_MstBankAccount
                       SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE DetBankAcId = @detBankAcId";
                return await con.ExecuteAsync(sql, new { DetBankAcId = detBankAcId });
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
