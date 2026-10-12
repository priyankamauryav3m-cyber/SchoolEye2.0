using DomainModel.Admin;
using DomainModel.FinanceMNGT;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolEye.ApplicationInterface.Admin
{
    public interface IAdmitChildRepository
    {
       public  Task<AdmitChildResult> AdmitChild(AdmitChildsRequest request);
        Task<RegistrationDetailsResponse> GetRegistrationDetails(RegistrationDetailsRequest request);
        Task<IEnumerable<ClassMapTemplateFeeHeadResponse>> GetClassMapTemplateFeeHead(ClassMapTemplateFeeHeadRequest request);
        Task<AdmitChildWithReceiptResponse> AdmitChildWithReceipt(AdmitChildWithReceiptRequest request);
    }
}
