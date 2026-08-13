using DomainModel.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.Admin
{
    public interface IViewPublishListRepository
    {
        public Task<IEnumerable<PublishingListResponse>> GetPublishingList(PublishingListRequest model);

    }
}
