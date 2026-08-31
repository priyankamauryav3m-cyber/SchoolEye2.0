using ApplicationInterface.Admin;
using Dapper;
using DomainModel.Admin;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Admin
{
    public class ClassGenderService:IClassGenderRepository
    {
        private readonly string _connectionString;

        public ClassGenderService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<GenderResponse>> GetClassGenderWiseReport(GenderRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", request.GroupCode);
                param.Add("@BranchCode", request.BranchCode);
                param.Add("@SessionId", request.SessionId);
                param.Add("@IsNewAdmission", request.IsNewAdmission);
                param.Add("@TillDate", request.TillDate);
                param.Add("@ClassCode", request.ClassCode);
                param.Add("@SectionId", request.SectionId);
                return await con.QueryAsync<GenderResponse>(
                    "V3M_Usp_GetClassGenderWiseReport",
                    param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

    }
}
