using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.FinanceMNGT.FeeMNGT
{
    public interface IPromoteConStRepository
    {
        public Task<IEnumerable<StudentResponse>> GetPromotionConcessionStudent(PromoteStudent requestModel);

      //  public Task<int> PromoteStudentConcession(PromoteStudent model);
        public Task<PromoteConcessionResponse> PromoteStudentConcession(PromoteConcessionRequest request);
    }
}
