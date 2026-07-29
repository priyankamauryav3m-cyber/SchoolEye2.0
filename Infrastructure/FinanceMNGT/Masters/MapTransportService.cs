using ApplicationInterface.FinanceMNGT.FeeMNGTMasters;
using Dapper;
using DomainModel.Admin;
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

namespace Infrastructure.FinanceMNGT.FeeMNGTMasters
{
    public class MapTransportService : IMapTransportRepository
    {
        private readonly string _connectionString;
        public MapTransportService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
        }
        public async Task<IEnumerable<TransportRoute>> GetStudentTransporRoutetData(SearchAnyRequestModel model)
        {
            using var con = new SqlConnection(_connectionString);

            const string query = @"
        SELECT RouteId, RouteName
        FROM TPT_RouteMaster
        WHERE IsValid = '1'
          AND GroupCode = @GroupCode
          AND BranchCode = @BranchCode
        ORDER BY RouteName";

            var result = await con.QueryAsync<TransportRoute>(
                query,
                new
                {
                    GroupCode = model.GroupCode,
                    BranchCode = model.BranchCode
                });

            return result;
        }
        public async Task<IEnumerable<TransportRoutePoint>> GetBoardingPoints(SearchAnyRequestModel model)
        {
            using var con = new SqlConnection(_connectionString);

            const string query = @"SELECT BordingPointId, PointName FROM TPT_BoardingPointMaster WHERE IsValid = 1
                          AND GroupCode = @GroupCode AND BranchCode = @BranchCode AND  RouteId = @RouteId ORDER BY PointNo";

            var result = await con.QueryAsync<TransportRoutePoint>(query, new
            {
                GroupCode = model.GroupCode,
                BranchCode = model.BranchCode,
                RouteId = model.RequestId
            });

            return result;
        }
        public async Task<IEnumerable<StudentTransportMappedModel>> GetStudentTransportData(SearchAnyRequestModel searchAnyRequestModel)
        {
            try
            {
                using var con = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@GroupCode", searchAnyRequestModel.GroupCode);
                parameters.Add("@BranchCode", searchAnyRequestModel.BranchCode);
                parameters.Add("@SessionId", searchAnyRequestModel.SessionId);
                parameters.Add("@StudentId", searchAnyRequestModel.RequestName);

                var result = await con.QueryAsync<StudentTransportMappedModel>(
                    "TPT_GetStudentTranportDetail",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
   

        public async Task<List<TransportStudentDataModel>> GetTransportStudentDataAsync(TransportSearchModel model)
        {

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();

                    parameters.Add("@GroupCode", model.GroupCode);
                    parameters.Add("@BranchCode", model.BranchCode);
                    parameters.Add("@SessionId", model.SessionId);
                    parameters.Add("@ClassCode", model.ClassCode);
                    parameters.Add("@SectionId", model.SectionId);
                    parameters.Add("@Gender", model.Gender);
                    parameters.Add("@StudentNo", model.StudentNo);
                    parameters.Add("@FirstName", model.FirstName);
                    parameters.Add("@IsTransportSelected", model.IsTransportSelected);
                    parameters.Add("@RouteDistance", model.RouteDistanceId);

                    var result = await connection.QueryAsync<TransportStudentDataModel>(
                        "V3M_Fee_GetTransportStudentData",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    return result.ToList();
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

        }


        public async Task<bool> AddOrUpdateTransportMapMonthData(TransportRequestModel transport)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@SessionId", transport.SessionId);
                parameters.Add("@StudentId", transport.StudentId);
                parameters.Add("@GroupCode", transport.GroupCode);
                parameters.Add("@BranchCode", transport.BranchCode);
                parameters.Add("@IsTrasportReq", transport.IsTrasportReq);
                parameters.Add("@DistanceId", transport.DistanceId);
                parameters.Add("@TransportAppliedFrom", transport.TransportAppliedFrom);
                parameters.Add("@CreatedBy", transport.CreatedBy);
                parameters.Add("@UpdatedBy", transport.UpdatedBy);
                parameters.Add("@SelectedMonthNo", transport.SelectedMonthNo);
                parameters.Add("@RouteId", transport.RouteId);
                parameters.Add("@BoardingPointId", transport.BoardingPointId);
                parameters.Add("@PassengerType", transport.PassengerType);
                parameters.Add("@DropRouteId", transport.DropRouteId);
                parameters.Add("@DropPointId", transport.DropPointId);
                var result = await connection.ExecuteScalarAsync<int>(
                    "V3M_FIN_TransportMapMonthConfig",
                    parameters,
                    commandType: CommandType.StoredProcedure);
                return result == 1;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database Error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: {ex.Message}", ex);
            }
        }


    }
}
