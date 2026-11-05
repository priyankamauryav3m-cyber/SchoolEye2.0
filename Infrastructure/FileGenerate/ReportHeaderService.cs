using ApplicationInterface.GenerateFile;
using Dapper;
using DomainModel.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Threading.Tasks;

namespace Infrastructure.FileGenerate
{
    // V3M_MstReportHeader -> header of every PDF / Excel (SP V3M_GetReportHeader).
    // Kept in memory for a short time so a report does not hit the DB again and again;
    // a change in the table shows on the reports within CacheMinutes.
    public class ReportHeaderService : IReportHeaderService
    {
        private const int CacheMinutes = 2;
        private static readonly ConcurrentDictionary<string, (ReportHeaderInfo Header, DateTime LoadedAt)> Cache = new();

        private readonly string _connectionString;

        public ReportHeaderService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString") ??
                throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }

        public async Task<ReportHeaderInfo> GetReportHeaderAsync(string? groupCode, string? branchCode)
        {
            var key = $"{groupCode?.Trim()}|{branchCode?.Trim()}";
            if (Cache.TryGetValue(key, out var cached) && DateTime.UtcNow - cached.LoadedAt < TimeSpan.FromMinutes(CacheMinutes))
                return cached.Header;

            try
            {
                using var con = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@GroupCode", groupCode?.Trim());
                param.Add("@BranchCode", branchCode?.Trim());

                var header = await con.QueryFirstOrDefaultAsync<ReportHeaderInfo>(
                    "V3M_GetReportHeader", param, commandType: CommandType.StoredProcedure);

                header ??= new ReportHeaderInfo();
                Cache[key] = (header, DateTime.UtcNow);
                return header;
            }
            catch (Exception ex)
            {
                // header must never stop a report: an old cached header or a blank one is used
                Console.WriteLine($"Error loading report header: {ex.Message}");
                return cached.Header ?? new ReportHeaderInfo();
            }
        }
    }
}
