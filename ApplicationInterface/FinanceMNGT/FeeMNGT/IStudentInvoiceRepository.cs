using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.FinanceMNGT.FeeMNGT
{
    public interface IStudentInvoiceRepository
    {
        public Task<IEnumerable<StudentFeeInvoiceResponseModel>> GetStudentForInvoiceGenerate(StudentForInvoiceRequestModel requestModel);
        public Task<int> SaveStudentChallanGenerateData(StudentInvoice request);
        public Task<List<InvoiceTypeModel>> GetInvoiceTypeList();
        public Task<IEnumerable<StudentClassModal>> GetStudentsByClassAsync(SearchAnyRequestModel searchAnyRequest);
        public Task<IEnumerable<StudentDuesModel>> GetStudentInvoiceDuesData(StudentInvoiceDuesRequest request);
        public Task<IEnumerable<SearchStudentBalanceDto>> GetStudentAdvanceBalanceData(SearchStudentBalanceDto request);
        public Task<int> StudentUpdateChallanDueDate(ChallanDueDateModal request);

        Task<IEnumerable<InvoiceDetailsResponse>> GetInvoiceDetailsAsync(SearchAnyRequestModel RequestModel);
        public Task<IEnumerable<FeeHeadDropdownModel>> GetFeeHeadDropdown(SearchAnyRequestModel searchAnyRequest);
        public Task<IEnumerable<TransportSelectMonthModel>> GetMonthWithTranspoet(SearchAnyRequestModel searchAnyRequest);
        public Task<int> AddFeeHeadToStudentChallanData(FeeHeadToStudentChallan request);
        public Task<int> RemoveFeeHeadToStudentChallanData(FeeHeadToStudentChallan request);

    }
}
