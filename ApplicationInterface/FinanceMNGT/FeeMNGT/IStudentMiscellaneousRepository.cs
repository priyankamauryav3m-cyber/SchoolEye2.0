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

        public Task<IEnumerable<StudentByFeeTemplateResponse>> GetStudentByFeeTemplate(StudentByFeeTemplateRequest request);

        // returns the SP @Result: 1 = success, -1 = error
        public Task<int> GenerateMiscChallanWithMonths(MiscChallanWithMonthsRequest request);

    }
}
