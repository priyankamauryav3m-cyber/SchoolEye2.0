using ApplicationInterface.FinanceMNGT.FeeMNGT;
using Dapper;
using DomainModel.FinanceMNGT;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

public class PromoteConStudentService : IPromoteConStRepository
{
    private readonly string _connectionString;
    public PromoteConStudentService(IConfiguration configuration)
    {
        _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
            ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
    }
    public async Task<IEnumerable<StudentResponse>> GetPromotionConcessionStudent(PromoteStudent requestModel)
    {
        try
        {
            using var con = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@GroupCode", requestModel.GroupCode);
            parameters.Add("@BranchCode", requestModel.BranchCode);
            parameters.Add("@SessionId", requestModel.SessionId);
            parameters.Add("@ConcessionId", requestModel.ConcessionId);
            parameters.Add("@Concessiontype", requestModel.Concessiontype);
            var result = await con.QueryAsync<StudentResponse>(
            "USP_GetPromotionConcessionStudent",
                parameters,
                commandType: CommandType.StoredProcedure
            );
            return result.ToList();
        }
        catch (Exception ex)
        {
            throw new Exception("Error while fetching Promotion list", ex);
        }
    }

    public async Task<PromoteConcessionResponse> PromoteStudentConcession(PromoteConcessionRequest request)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            DataTable dt = new DataTable();
            dt.Columns.Add("StudentId", typeof(long));
            foreach (var item in request.StudentIds)
            {
                dt.Rows.Add(item);
            }
            var parameters = new DynamicParameters();
            parameters.Add("@GroupCode", request.GroupCode);
            parameters.Add("@BranchCode", request.BranchCode);
            parameters.Add("@FromSessionId", request.FromSessionId);
            parameters.Add("@ToSessionId", request.ToSessionId);
            parameters.Add("@CreatedBy", request.CreatedBy);
            parameters.Add("@Remarks", request.Remarks);
            parameters.Add( "@StudentIds", dt.AsTableValuedParameter("StudentIdTableType") );
            parameters.Add(
                "@ReturnValue",
                dbType: DbType.Int32,
                direction: ParameterDirection.ReturnValue
            );

            using var multi = await connection.QueryMultipleAsync("V3M_FIN_UspPromoteStudentConcession",commandType: CommandType.StoredProcedure);
            var missingStudents =(await multi.ReadAsync<MissingStudentModel>()).ToList();
            int resultCode = parameters.Get<int>("@ReturnValue");
            return new PromoteConcessionResponse
            {
                ResultCode = resultCode,
                MissingStudents = missingStudents
            };
        }
        catch (Exception ex)
        {
            throw new Exception("Error while fetching Promotion list", ex);
        }
    }
}
