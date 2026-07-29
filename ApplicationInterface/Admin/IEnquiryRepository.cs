using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.Admin
{
    public interface IEnquiryRepository
    {

        public Task<List<EnquiryListResponse>> GetEnquiryListofData(EnquiryRequestDto request);
        public Task<string> SubmitEnquiryData(EnquiryListResponse model);
        public Task<List<FollowupDetailsResponse>> GetFollowupDetails(SearchAnyRequestModel searchAnyRequest);
        public Task<string> AddFollowupDetails(AddFollowupRequest request);
        Task<DashboardResponse> GetDashboardAsync(
       int currentSessionId,
       int previousSessionId);


    }
}
