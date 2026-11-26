using DomainModel.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationInterface.Admin
{
    // DPDP: Student Sensitive Details page - the only place where unmasked student / parent
    // sensitive data is read and changed.
    public interface IStudentSensitiveDetailsRepository
    {
        // role has IsPII = 1 on /StudentSensitiveDetails and the user belongs to the group / branch
        Task<PiiAccessResult> CheckPiiAccessAsync(int userSid, string pageUrl, string groupCode, string branchCode);

        Task<StudentSensitiveDetailsResponse?> GetDetailsAsync(StudentSensitiveDetailsRequest request);

        Task<StudentSensitiveUpdateResult> UpdateAsync(StudentSensitiveUpdateRequest request, string updatedBy, string? ipAddress);

        Task<IEnumerable<StudentSensitiveHistoryRow>> GetHistoryAsync(StudentSensitiveDetailsRequest request);
    }
}
