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
    public class SubjectNameService : IStudentNameRepository
    {
        private readonly string _connectionString;
        public SubjectNameService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<List<ShareDomain>> GetCompulsorySubjects(string groupCode, string branchCode, string streamCode)
        {
            using var con = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@GroupCode", groupCode);
            param.Add("@BranchCode", branchCode);
            param.Add("@StreamCode", streamCode);
            var result = await con.QueryFirstOrDefaultAsync<string>(
                "USP_GetCompulsorySubjects",
                param,
                commandType: CommandType.StoredProcedure
            );
            List<ShareDomain> list = new List<ShareDomain>();
            if (!string.IsNullOrEmpty(result))
            {
                list.Add(new ShareDomain
                {
                    ValueField = result,
                    TextField = result
                });
            }
            return list;
        }

        public async Task<List<ShareDomain>> ElectiveSubjectsData(string groupCode, string branchCode,string streamCode,string groupId,string firstElement)
        {
            using var con = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@GroupCode", groupCode);
            param.Add("@BranchCode", branchCode);
            param.Add("@StreamCode", streamCode);
            param.Add("@GroupId", groupId);
            var result = (await con.QueryAsync<ElectiveSubjects>(
                "sp_GetElectiveSubjects",
                param,
                commandType: CommandType.StoredProcedure)).ToList();
            List<ShareDomain> list = new();
            if (!string.IsNullOrEmpty(firstElement))
            {
                list.Add(new ShareDomain
                {
                    ValueField = "0",
                    TextField = firstElement
                });
            }
            if (result.Count > 0)
            {
                foreach (var item in result)
                {
                    list.Add(new ShareDomain
                    {
                        ValueField = item.GroupId,
                        TextField = item.SubjectName
                    });
                }
            }
            else
            {
                list.Add(new ShareDomain
                {
                    ValueField = "0",
                    TextField = "No Subjects Found"
                });
            }
            return list;
        }
    }
}
