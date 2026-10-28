using DomainModel.FinanceMNGT;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace ApplicationInterface.FinanceMNGT.FeeMNGT
{

    public interface IStudentFeeReceiptRepository
    {
        Task<IEnumerable<StudentFeeReceiptListResponse>> GetStudentFeeReceiptList(StudentFeeReceiptListRequest request);

        Task<IEnumerable<ClassGroupResponse>> GetClassGroup(ClassGroupRequest request);

        Task<StudentReceiptByIdResponse?> GetStudentReceiptById(StudentReceiptByIdRequest request);

        Task<IEnumerable<ReceiptDetailResponse>> GetReceiptDetail(StudentReceiptByIdRequest request);
         Task<List<StudentReceiptHeadWiseResponse>> GetWithModeReceiptHeadWiseDetail(StudentReceiptHeadWiseRequest request);
    }
}
