using DomainModel.SchoolMaster;

namespace ApplicationInterface.SchoolMaster
{
    public interface IDocumentRepository
    {

        public Task<IEnumerable<DocumentModel>> GetAllAsync();

        // public Task<int> AddUpdateDocument(DocumentModel objDocument);
        public Task<string> AddUpdateDocument(DocumentModel objDocument);

        public Task<int> DeleteDocumentData(int docId);
    }
}
