using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInterface.FinanceMNGT
{
    public interface IViewStudentRepository
    {
        public Task<IEnumerable<ViewStudentModal>> GetViewStudentListData(StudentRequest request);
        public Task<IEnumerable<GetSearchedViewStudentModel>> GetSearchedStudentListData(GetSearchedStudentRequestModel request);
        public Task<StudentViewDetailsModel?> GetStudentDetailsAsync(SearchAnyRequestModel requestModel);
        public Task<StudentParentDetailsModel> GetStudentParentDetails(SearchAnyRequestModel requestModel);
        public Task<StudentPassportVisaModel> GetStudentOtherDetails(SearchAnyRequestModel requestModel);
        public Task<SiblingDetailsModel> GetSiblingDetailsData(SearchAnyRequestModel request);
        public Task<IEnumerable<SiblingDetailsModel>> GetSiblingListData(SearchAnyRequestModel request);
        public Task<int> AddUpdateSiblingAsync(AddSiblingRequest request);
        public Task<bool> UpdateSiblingData(SearchAnyRequestModel model);
        public Task<StudentAddressDetailsModel> GetStudentAddressDetails(SearchAnyRequestModel request);
        public Task<int> AddOrUpdateStudentPersonalDetails(StudentViewDetailsModel model);
        public Task<int> SaveStudentParentDetailsData(StudentParentDetailsModel request);
        Task<int> SaveStudentAddressData(StudentAddressDetailsModel request);
        public Task<int> SavePassportDetailsData(StudentPassportVisaModel request);
        public Task<StudentVisitorsModel> GetStudentVisitorsData(SearchAnyRequestModel model);
        public Task<int> SaveStudentVisitorsData(StudentVisitorsModel res);
        public Task<int> RemoveProfileImageData(ProfileImageModal model);
        public Task<string> VerifyAndTakeDocument(StudentDocumentModel model);
       // public Task<IEnumerable<ClassStudentDocumentModel>> GetClassStudentDocuments(ClassStudentDocumentRequest request);
        public Task<int> DeactivateStudentDocument(ClassStudentDocumentRequest request);
        public Task<string> HandoverStudentDocumentData(StudentDocumentModel model);
        public Task<int> UnMapStudentHandoverDocumentData(ClassStudentDocumentRequest request);





    }
}
