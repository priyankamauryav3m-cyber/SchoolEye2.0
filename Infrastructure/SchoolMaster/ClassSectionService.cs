using ApplicationInterface.SchoolMaster;
using Dapper;
using DomainModel.FinanceMNGT;
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
    public  class ClassSectionService: IClassSectionRepository
    {
        private readonly string _connectionString;
        public ClassSectionService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async Task<IEnumerable<ClassSectionModal>> GetClassSectionData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "select SectionId,SectionName from MstClassSection with(nolock) where IsValid=1";
                return await con.QueryAsync<ClassSectionModal>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<IEnumerable<CategoryModal>> GetCategoryData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "select * from V3M_MstStudentCategory with(nolock)  where IsValid=1";
                return await con.QueryAsync<CategoryModal>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<IEnumerable<MstDistance>> GetDistanceData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "select DistanceId,DistanceName from ADM_DistanceMaster with(nolock) where IsValid=1";
                return await con.QueryAsync<MstDistance>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
      
        public async Task<IEnumerable<MotherTongue>> GetMotherTongueData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT TongueId, TongueName FROM MstMotherTongue with(nolock) where IsValid=1";
                return await con.QueryAsync<MotherTongue>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<VisaType>> GetVisaTypeData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT VisaTypeId,VisaTypeName FROM MstVisaType with(nolock) where IsValid=1";
                return await con.QueryAsync<VisaType>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<PassportName>> GetPassportTypeNameData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT PassportTypeID,PassportTypeName FROM MstPassportType with(nolock) where IsValid=1";
                return await con.QueryAsync<PassportName>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<BranchNameMst>> GetBranchNameData()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT BranchId,BranchCode,BranchName FROM MstBranchMaster  with(nolock)";
                return await con.QueryAsync<BranchNameMst>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<List<SectionModel>> GetClassSection(SearchAnyRequestModel model)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", model.GroupCode ?? "");
                parameters.Add("@BranchCode", model.BranchCode ?? "");
                parameters.Add("@ClassCode", model.RequestName ?? "");

                var result = await connection.QueryAsync<SectionModel>(
                    "Usp_GetClassSection",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
    }
}
