using DomainModel.SchoolMaster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.Admin
{
    public interface IBookMarkRepository
    {
        public Task<string> AddOrUpdateBookMarksData(BookMarkModel objbook);
        public Task<int> DeleteBookMarksData(int bookMarkId);
        public Task<IEnumerable<BookMarkModel>> GetBookMarksData(string createdby);
    }
}
