using ApplicationInterface.SchoolMaster;
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
    public class IPAddressService:IPAddressRepository
    {

        private readonly string _connectionString;
        public IPAddressService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<string> AddUpdateIPAddress(AllowedIPModel objip)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var cmd = new SqlCommand("V3M_InsertUpdate_IPAddress", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@AllowedIPId", SqlDbType.Int).Value = objip.AllowedIPId;
                cmd.Parameters.Add("@GroupCode", SqlDbType.VarChar, 5).Value = objip.GroupCode;
                cmd.Parameters.Add("@BranchCode", SqlDbType.VarChar, 5).Value = objip.BranchCode;
                cmd.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = objip.UserId;
                cmd.Parameters.Add("@ModuleId", SqlDbType.Int).Value = objip.ModuleId;
                cmd.Parameters.Add("@Ip", SqlDbType.VarChar, 20).Value = objip.Ip;
                cmd.Parameters.Add("@ValidUpto", SqlDbType.DateTime).Value = objip.ValidUpto;
                var returnValueParam = new SqlParameter("@ReturnValue", SqlDbType.VarChar, 50)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(returnValueParam);
                await cmd.ExecuteNonQueryAsync();
                return returnValueParam.Value?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to insert/update IP Address", ex);
            }
        }
    }
}
