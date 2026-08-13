using DomainModel.FinanceMNGT;
using DomainModel.SchoolMaster;

namespace ApplicationInterface.SchoolMaster
{
    public interface IClassDocumentRepository
    {

        public Task<IEnumerable<ClassDocumentModel>> GetAllAsync(SearchAnyRequestModel request);
        Task<int> UpdateMandatoryAsync(UpdateClassDocumentRequest request);
        public Task<int> MapDocumentWithClass(ClassDocumentModel objMapping);

        public Task<int> DeleteClassDocumentData(UpdateClassDocumentRequest request);
    }
}
