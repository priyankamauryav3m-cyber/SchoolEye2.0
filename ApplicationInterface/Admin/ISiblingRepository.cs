using DomainModel.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationInterface.Admin
{
    public interface ISiblingRepository
    {
        public Task<IEnumerable<SiblingListResponse>> GetSiblingList(SiblingListRequest request);
        public Task<IEnumerable<SiblingListResponse>> GetSiblings(SiblingListRequest request);

    }
}
