using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.Admin
{
    public interface IStudentPromotionRepository
    {
        public Task<IEnumerable<StudentNotPromotedModel>> GetAllNotPromotedStudent(SearchAnyRequestModel searchAny);
        public Task<IEnumerable<ClassWiseStudentForPromotion>> GetClassWiseStudentPromotion(SearchAnyRequestModel searchAny);
        public Task<List<string>> PromoteStudentClass(List<PromoteClassModel> promoteList);
    }
}
