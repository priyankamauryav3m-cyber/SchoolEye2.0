using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.Admin
{
    public interface IAdminDashboard
    {
        public Task<AdminDashboardModal> GetAdminDashboardData(SearchAnyRequestModel model);
       public Task<List<FeeHeadCollectionDto>> GetFeeHeadCollectionSummary(SearchAnyRequestModel model);
        public Task<AdmissionDashboardModel> GetAdmissionData(SearchAnyRequestModel model);



    }
}
