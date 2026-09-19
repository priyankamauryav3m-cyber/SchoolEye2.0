using DomainModel.FinanceMNGT;
using MyApp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ApplicationInterface.FinanceMNGT.FeeMNGT
{
    public interface IStudentFeeRepository
    {
        public Task<IEnumerable<StudentDetailsForFee>> GetStudentDetailsForFeeAsync( StudentDetailsForFeeRequest request);
        public Task<IEnumerable<FeeCollectionDuesNew>> GetStudentDetailsForFeeDue(GetStudentFeeHeadDuesForAdjustmentRequest request);
        public Task<AdjustStudentFeeHeadWiseResponse> AdjustStudentFeeHeadWise(AdjustStudentFeeHeadWiseRequest request);
        public Task<IEnumerable<StudentLedgerChallanMiniDetailsResponse>> GetStudentLedgerChallanMiniDetails(StudentLedgerChallanMiniDetailsRequest request);
        public Task<IEnumerable<StudentChallanResponse>> GetStudentChallanDetails(StudentChallanRequest request);

    }
}
