using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.SchoolMaster
{
    public interface ISourceOfEnquiryRepository
    {
        Task<IEnumerable<SourceOfEnquiryModel>> GetAllAsync();
        Task<int> DeleteSourceOfEnquiry(int sourceId);
        Task<string> AddUpdateSourceOfEnquiry(SourceOfEnquiryModel model);
    }
}
