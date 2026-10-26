using DomainModel.FinanceMNGT;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace SchoolEye.ApplicationInterface.Admin
{
    public interface IStudentMiscellaneousRepository
    {
        public Task<IEnumerable<MiscellaneousResponse>> GetStudentMiscellaneous(MiscellaneousRequest request);

        public Task<IEnumerable<FeeHeadForComboResponse>> GetFeeHeadForCombo(FeeHeadForComboRequest request);

        public Task<IEnumerable<GenerateMiscChallanResponse>> GenerateMiscellaneousChallan(GenerateMiscChallanRequest request);
        
    }
}
