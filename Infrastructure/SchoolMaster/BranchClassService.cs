using ApplicationInterface.SchoolMaster;
using Dapper;
using DomainModel.Admin;
using DomainModel.SchoolMaster;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.SchoolMaster
{
    public class BranchClassService:IBranchClassRepository
    {
        private readonly string _connectionString;

        public BranchClassService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
            ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<BranchClassModel>> GetBranchesAsync()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT ClassId,GroupCode,BranchCode,ClassCode,ClassName,TCDisplayName,IsValid,CreatedDate,CreatedBy,TallyGroupName,IsShowOnlineReg FROM MstBranchClass with(nolock) where GroupCode='01' AND BranchCode='01'";
                return await con.QueryAsync<BranchClassModel>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
    }
}
