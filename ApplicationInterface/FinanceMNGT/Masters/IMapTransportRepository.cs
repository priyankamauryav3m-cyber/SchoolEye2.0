using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.FinanceMNGT.FeeMNGTMasters
{
    public interface IMapTransportRepository
    {
        public Task<List<TransportStudentDataModel>> GetTransportStudentDataAsync(TransportSearchModel model);
        public Task<IEnumerable<StudentTransportMappedModel>> GetStudentTransportData(SearchAnyRequestModel searchAnyRequestModel);
        public Task<IEnumerable<TransportRoute>> GetStudentTransporRoutetData(SearchAnyRequestModel searchAnyRequestModel);
        public Task<IEnumerable<TransportRoutePoint>> GetBoardingPoints(SearchAnyRequestModel model);
        public Task<bool> AddOrUpdateTransportMapMonthData(TransportRequestModel transport);

    }
}
