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
    public class BranchService : IBranchRepository
    {
        private readonly string _connectionString;

        public BranchService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<BranchModel>> GetAllAsync()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"SELECT BranchId,GroupCode,BranchCode,BranchName,Remarks,ContactPerson,ContactNo,ContactEmailId,
                       LogoPath,ContactPersonImagePath,WebSite,BranchEmailId,AffiliationDetails,SchoolNo,
                       AddressLine1,AddressLine2,DistrictId,StateId,CountryId,PinCode,StartTime,EndTime,IsHO,
                       IsValid,CreatedBy,CreatedDate,LogoPathForPrint,BranchCategory,AffiliationUpto,
                       StatusOfSchool,UDISENo
                       FROM MstBranchMaster ORDER BY BranchId ASC";
                return await con.QueryAsync<BranchModel>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<int> DeleteBranchMasterData(int branchId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE MstBranchMaster SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE BranchId = @BranchId";
                return await con.ExecuteAsync(sql, new { BranchId = branchId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<string> AddUpdateBranchMaster(BranchModel objBranch)
        {
            try
            {
                string returnValue;
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("V3M_InsertUpdate_BranchMaster", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@BranchId", objBranch.BranchId);
                        cmd.Parameters.AddWithValue("@GroupCode", objBranch.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", objBranch.BranchCode);
                        cmd.Parameters.AddWithValue("@BranchName", objBranch.BranchName);
                        cmd.Parameters.AddWithValue("@Remarks", (object)objBranch.Remarks ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ContactPerson", objBranch.ContactPerson);
                        cmd.Parameters.AddWithValue("@ContactNo", objBranch.ContactNo);
                        cmd.Parameters.AddWithValue("@ContactEmailId", (object)objBranch.ContactEmailId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@LogoPath", (object)objBranch.LogoPath ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ContactPersonImagePath", (object)objBranch.ContactPersonImagePath ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@WebSite", (object)objBranch.WebSite ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@BranchEmailId", (object)objBranch.BranchEmailId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@AffiliationDetails", (object)objBranch.AffiliationDetails ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@SchoolNo", (object)objBranch.SchoolNo ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@AddressLine1", objBranch.AddressLine1);
                        cmd.Parameters.AddWithValue("@AddressLine2", (object)objBranch.AddressLine2 ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DistrictId", objBranch.DistrictId);
                        cmd.Parameters.AddWithValue("@StateId", objBranch.StateId);
                        cmd.Parameters.AddWithValue("@CountryId", objBranch.CountryId);
                        cmd.Parameters.AddWithValue("@PinCode", (object)objBranch.PinCode ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@StartTime", (object)objBranch.StartTime ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@EndTime", (object)objBranch.EndTime ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsHO", objBranch.IsHO);
                        cmd.Parameters.AddWithValue("@IsValid", objBranch.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", objBranch.CreatedBy);
                        cmd.Parameters.AddWithValue("@LogoPathForPrint", (object)objBranch.LogoPathForPrint ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@BranchCategory", (object)objBranch.BranchCategory ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@AffiliationUpto", (object)objBranch.AffiliationUpto ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@StatusOfSchool", (object)objBranch.StatusOfSchool ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@UDISENo", (object)objBranch.UDISENo ?? DBNull.Value);
                        SqlParameter returnValueParam = new SqlParameter
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
                throw new Exception("Error while inserting/updating Branch Master", ex);
            }
        }
    }
}
