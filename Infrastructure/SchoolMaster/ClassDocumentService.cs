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
    public class ClassDocumentService : IClassDocumentRepository
    {
        private readonly string _connectionString;

        public ClassDocumentService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<ClassDocumentModel>> GetAllAsync(SearchAnyRequestModel request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();

                parameters.Add("@GroupCode", request.GroupCode);
                parameters.Add("@BranchCode", request.BranchCode);
                parameters.Add("@RequestName", request.RequestName);

                return await con.QueryAsync<ClassDocumentModel>(
                    "V3M_Get_ClassDocumentMapping",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<int> DeleteClassDocumentData(UpdateClassDocumentRequest request)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();

                parameters.Add("@GroupCode", request.GroupCode);
                parameters.Add("@BranchCode", request.BranchCode);
                parameters.Add("@DocumentId", request.DocumentId);
                parameters.Add("@ClassCode", request.ClassCode);
                parameters.Add("@CreatedBy", request.CreatedBy);

                return await con.ExecuteScalarAsync<int>(
                    "V3M_Delete_ClassDocument",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<int> MapDocumentWithClass(ClassDocumentModel objMapping)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("ADM_MapDocumentWithClass", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@GroupCode", objMapping.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", objMapping.BranchCode);
                        cmd.Parameters.AddWithValue("@DocumentId", objMapping.DocumentId);
                        cmd.Parameters.AddWithValue("@ClassCode", objMapping.ClassCode);
                        cmd.Parameters.AddWithValue("@CreatedBy", objMapping.CreatedBy);
                        SqlParameter resultParam = new SqlParameter
                        {
                            ParameterName = "@Result",
                            SqlDbType = SqlDbType.Int,
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(resultParam);
                        await cmd.ExecuteNonQueryAsync();
                        return (int)resultParam.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while mapping Document with Class", ex);
            }
        }
        public async Task<int> UpdateMandatoryAsync(UpdateClassDocumentRequest request)
        {
            using var con = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();

            parameters.Add("@GroupCode", request.GroupCode);
            parameters.Add("@BranchCode", request.BranchCode);
            parameters.Add("@DocumentId", request.DocumentId);
            parameters.Add("@ClassCode", request.ClassCode);
            parameters.Add("@IsMandatory", request.IsMandatory);
            parameters.Add("@TransType", request.TransType);
            parameters.Add("@CreatedBy", request.CreatedBy);

            return await con.ExecuteScalarAsync<int>(
                "V3M_Update_ClassDocumentMandatory",
                parameters,
                commandType: CommandType.StoredProcedure);
        }
    }
}
