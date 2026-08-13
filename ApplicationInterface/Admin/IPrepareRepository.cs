using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.Admin
{
    public interface IPrepareRepository
    {
        public Task<IEnumerable<RegistrationInfoListResponse>> GetRegistrationInfoList(RegistrationInfoListRequest model);
        public Task<IEnumerable<PublishListModel>> GetAllAsync(SearchAnyRequestModel searchAnyRequest);
        public Task<int> AddPublishList(PublishListModel model);

        public Task<IEnumerable<RegistrationInfoListRequest>> GetListStatusData(PublishListModel request);
       public Task<int> AddStudentInListAsync(AddStudentInListRequest request);
       public Task<int> DeleteStudentInListAsync(AddStudentInListRequest request);
       public Task<int> PublishStudentInListAsync(AddStudentInListRequest request);
        public Task<IEnumerable<RegistrationInfoListResponse>> GetPublishingListDetails(RegistrationInfoListRequest model);

    }
}
