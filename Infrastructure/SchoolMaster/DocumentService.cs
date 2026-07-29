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
    public class DocumentService : IDocumentRepository
    {
        private readonly string _connectionString;

        public DocumentService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<DocumentModel>> GetAllAsync()
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = "SELECT DocId,GroupCode,BranchCode,DocumentName,DocumentType,DisplayOrder,Remarks,IsValid,CreatedDate,CreatedBy FROM ADM_MstDocument ORDER BY DocId ASC";
                return await con.QueryAsync<DocumentModel>(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<int> DeleteDocumentData(int docId)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);
                string sql = @"UPDATE ADM_MstDocument SET IsValid = CASE WHEN IsValid = 1 THEN 0 ELSE 1 END
                       WHERE DocId = @DocId";
                return await con.ExecuteAsync(sql, new { DocId = docId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
        public async Task<string> AddUpdateDocument(DocumentModel objDocument)
        {
            try
            {
                string returnValue;
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("V3M_InsertUpdate_Document", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@DocId", objDocument.DocId);
                        cmd.Parameters.AddWithValue("@GroupCode", objDocument.GroupCode);
                        cmd.Parameters.AddWithValue("@BranchCode", objDocument.BranchCode);
                        cmd.Parameters.AddWithValue("@DocumentName", objDocument.DocumentName);
                        cmd.Parameters.AddWithValue("@DocumentType", objDocument.DocumentType);
                        cmd.Parameters.AddWithValue("@DisplayOrder", objDocument.DisplayOrder);
                        cmd.Parameters.AddWithValue("@Remarks", (object)objDocument.Remarks ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsValid", objDocument.IsValid);
                        cmd.Parameters.AddWithValue("@CreatedBy", objDocument.CreatedBy);
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
                throw new Exception("Error while inserting/updating Document", ex);
            }
        }
    }
}
