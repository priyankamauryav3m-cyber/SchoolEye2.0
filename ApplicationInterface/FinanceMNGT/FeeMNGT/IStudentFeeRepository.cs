using DomainModel.FinanceMNGT;
using MyApp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DomainModel.FinanceMNGT.FeeCollectionDuesNew;

namespace ApplicationInterface.FinanceMNGT.FeeMNGT
{
    public interface IStudentFeeRepository
    {
        Task<IEnumerable<StudentDetailsForFee>> GetStudentDetailsForFeeAsync( StudentDetailsForFeeRequest request);
        Task<IEnumerable<FeeCollectionDuesNew>> GetStudentDetailsForFeeDue(GetStudentFeeHeadDuesForAdjustmentRequest request);
    }
}
