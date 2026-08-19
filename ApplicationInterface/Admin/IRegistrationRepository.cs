using DomainModel.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ApplicationInterface.SuperAdmin
{
    public interface IRegistrationRepository
    {

        public Task<string> AddStudentRegistration(RegistrationModal res);
        public Task<IEnumerable<RegistrationDto>> SearchAsync(RegistrationSearchDto search);
        public Task<string> RegistrationChildDetails(ChildDetails dto);
        public Task<ParentsDetails> GetRegistrationParentsChildById(string groupCode, string branchCode, long sessionId, long registrationId);
        public Task<string> RegistrationAddressInformation(AddressDetails add);
        public Task<string> RegistrationAdditionalInformation(PointsCriteria cre);
        public Task<ChildDetails> GetRegistrationChildData(string groupCode, string branchCode, long sessionId, long registrationId);
        public Task<PointsCriteria> GetRegistrationChildotherInformation(string groupCode, string branchCode, long sessionId, long registrationId);
        public Task<AddressDetails> GetRegistrationChildAddressInformation(string groupCode, string branchCode, long sessionId, long registrationId, string AddressType);
        public Task<string?> SubmitFamilyInfoDetails(FamilyDetails res);
        public Task<List<FamilyDetails>> GetRegistrationFamilyDetails(string groupCode, string branchCode, long sessionId, long registrationId);
        public Task<string> RegistrationParentDetails(ParentsDetails pd);
        public Task<IEnumerable<StudentAdmissionModel>> GetStudentDetailsData(string groupCode, string branchCode, string? studentNo, string? sessionName);
        public Task<GetRegistrationModel> GetStudentDetailsData(string groupCode, string branchCode, long SessionId, string RegistrationId, string AddressType);
        public Task<IEnumerable<StudentListResponse>> AdmintStudentListData(StudentListRequest model);
        // public  Task<string> StudentDirectAdmissionData(StudentDirectAdmissionModel model);

        public Task<string> StudentDirectAdmissionData(StudentDirectAdmissionModel model);
        public Task<SiblingDetailResponse?> GetSiblingDetail(string groupCode, string branchCode, int SessionId, string siblingID = "");

        public Task<IEnumerable<CommonDomain>> GetFeeHeadConcession(string groupCode, string branchCode, int SessionId, string concessionId, int isMappedOnly);
        public Task<string> InsertOnlineRegistration(OnlineRegistration online);
        public Task<string> RegistrationCancel(RegistrationStatus online);
        public Task<IEnumerable<CommonDomainLarge>> RFM_GetRegFormatTypeDate(FormateType formateType);
        public Task<string> RFM_AddUpdateRegFormat(CommonDomainLarge commonDomainLarge);
        public Task<int> RFM_Active(int Id);
       public Task<IEnumerable<RegistrationDto>> GetRegistrationDetails(RegistrationSearchDto model);

        public Task<int> UpdateRegistrationStatus(UpdateRegistrationStatusModel model);
        public Task<string> DirectAdmissionStatusData(RegistrationStatus online);
        public Task<StudentListResponse> AdmitChildAsync(AdmitChildRequest request);
        public Task<string> VerifyAndTakeDocument(StudentDocumentModel model);
        public Task<string> DeActivateDocumentData(StudentDocumentModel model);

        public Task<IEnumerable<ClassRegistrationDocumentsResponse>> GetClassRegistrationDocumentsAsync(ClassRegistrationDocumentsRequest request);
        public Task<IEnumerable<RegistrationStatusResponse>> GetRegistrationStatusData(RegistrationRequest request);


    }
}
