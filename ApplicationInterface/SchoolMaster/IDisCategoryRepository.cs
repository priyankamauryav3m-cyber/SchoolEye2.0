using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.SchoolMaster
{
    public interface IDisCategoryRepository
    {
        public Task<string> AddUpdateDisCategory(DisCategoryModel objCategory);
        public Task<int> DeleteDisCategoryData(int categoryId);
        public Task<IEnumerable<DisCategoryModel>> GetDisCategoryData(int sessionId);
    }
}
