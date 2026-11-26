using ApplicationInterface.Admin;
using DomainModel.Admin;
using DomainModel.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using ServerWebAPI.Authorization;
using System.Security.Claims;

namespace ServerWebAPI.Addmission.Controllers.Admin
{
    // DPDP: Student Sensitive Details.
    // The ONLY API that returns a student's / parent's sensitive data unmasked and the ONLY API that changes it.
    // Every call re-checks in the database that the logged-in user (UserSid from the verified JWT) has
    // IsPII = 1 on /StudentSensitiveDetails for the requested group / branch - the role, menu and
    // group / branch the browser sends are never trusted on their own.
    [Authorize]
    [ApiExplorerSettings(GroupName = "Admission")]
    [Route("api/[controller]")]
    [ApiController]
    public class StudentSensitiveDetailsController : ControllerBase
    {
        private readonly IStudentSensitiveDetailsRepository _service;

        public StudentSensitiveDetailsController(IStudentSensitiveDetailsRepository service)
        {
            _service = service;
        }

        // page open: may this user use the page for this branch?
        [HttpPost("CheckAccess")]
        public async Task<IActionResult> CheckAccess([FromBody] StudentSensitiveAccessRequest request)
        {
            try
            {
                var access = await GetAccessAsync(request.GroupCode, request.BranchCode);
                return Ok(ApiResponse<bool>.Ok(access?.HasAccess == true, access?.HasAccess == true ? "Access granted." : "Access denied."));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<bool>.Fail("Unable to check the access right now."));
            }
        }

        [HttpPost("GetDetails")]
        public async Task<IActionResult> GetDetails([FromBody] StudentSensitiveDetailsRequest request)
        {
            try
            {
                var denied = await DenyIfNoAccessAsync(request.GroupCode, request.BranchCode);
                if (denied != null) return denied;

                var data = await _service.GetDetailsAsync(request);
                if (data == null)
                    return NotFound(ApiResponse<StudentSensitiveDetailsResponse>.Fail("Student record was not found."));

                NoStore();
                return Ok(ApiResponse<StudentSensitiveDetailsResponse>.Ok(data, "Student sensitive details retrieved successfully.", code: 1));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<StudentSensitiveDetailsResponse>.Fail("Unable to load the student details right now."));
            }
        }

        [HttpPost("GetHistory")]
        public async Task<IActionResult> GetHistory([FromBody] StudentSensitiveDetailsRequest request)
        {
            try
            {
                var denied = await DenyIfNoAccessAsync(request.GroupCode, request.BranchCode);
                if (denied != null) return denied;

                if (!string.IsNullOrWhiteSpace(request.FieldKey) && SensitiveFieldCatalog.Find(request.FieldKey.Trim()) == null)
                    return BadRequest(ApiResponse<IEnumerable<StudentSensitiveHistoryRow>>.Fail("Unknown field."));

                var rows = await _service.GetHistoryAsync(request);
                NoStore();
                return Ok(ApiResponse<IEnumerable<StudentSensitiveHistoryRow>>.Ok(rows, "Change history retrieved successfully.", code: 1));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<IEnumerable<StudentSensitiveHistoryRow>>.Fail("Unable to load the change history right now."));
            }
        }

        [HttpPost("Update")]
        public async Task<IActionResult> Update([FromBody] StudentSensitiveUpdateRequest request)
        {
            try
            {
                var access = await GetAccessAsync(request.GroupCode, request.BranchCode);
                if (access == null) return Unauthorized(ApiResponse<StudentSensitiveUpdateResult>.Fail("Unable to identify the current user."));
                if (!access.HasAccess) return Forbidden<StudentSensitiveUpdateResult>();

                // every changed field: known key, clean value, valid for its type
                var errors = new List<string>();
                var seen = new HashSet<string>(StringComparer.Ordinal);
                foreach (var change in request.Changes)
                {
                    var field = SensitiveFieldCatalog.Find(change.FieldKey?.Trim());
                    if (field == null)
                    {
                        errors.Add("Unknown field.");
                        continue;
                    }
                    if (!seen.Add(field.Key))
                    {
                        errors.Add($"{field.Group} - {field.Label} is sent twice.");
                        continue;
                    }
                    change.FieldKey = field.Key;
                    change.NewValue = SensitiveFieldCatalog.Normalize(field, change.NewValue);
                    var error = SensitiveFieldCatalog.Validate(field, change.NewValue);
                    if (error != null) errors.Add($"{field.Group} - {error}");
                }
                if (errors.Count > 0)
                    return BadRequest(ApiResponse<StudentSensitiveUpdateResult>.Fail("Please correct the highlighted fields.", errors.ToArray()));

                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                var result = await _service.UpdateAsync(request, access.UserName ?? "", ip);

                return result.Result switch
                {
                    1 => Ok(ApiResponse<StudentSensitiveUpdateResult>.Ok(result,
                            $"{result.ChangedCount} field(s) updated successfully.", code: 1)),
                    3 => Ok(new ApiResponse<StudentSensitiveUpdateResult>
                            { Success = false, Status = 200, Code = 3, Message = "Nothing to update. The values are the same as saved.", Data = result }),
                    -1 => NotFound(ApiResponse<StudentSensitiveUpdateResult>.Fail("Student record was not found.")),
                    -3 => BadRequest(ApiResponse<StudentSensitiveUpdateResult>.Fail("Enter the reason for the change.")),
                    -2 or -4 => BadRequest(ApiResponse<StudentSensitiveUpdateResult>.Fail("Some values are not valid. Please check and try again.")),
                    _ => StatusCode(StatusCodes.Status500InternalServerError,
                            ApiResponse<StudentSensitiveUpdateResult>.Fail("Unable to save the changes right now. Please try again."))
                };
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<StudentSensitiveUpdateResult>.Fail("Unable to save the changes right now. Please try again."));
            }
        }

        // ---------------------------------------------------------------- helpers

        // null = user not identified
        private async Task<PiiAccessResult?> GetAccessAsync(string groupCode, string branchCode)
        {
            var userSid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userSid, out var sid) || sid <= 0)
                return null;
            return await _service.CheckPiiAccessAsync(sid, SensitiveFieldCatalog.PageUrl, groupCode.Trim(), branchCode.Trim());
        }

        private async Task<IActionResult?> DenyIfNoAccessAsync(string groupCode, string branchCode)
        {
            var access = await GetAccessAsync(groupCode, branchCode);
            if (access == null) return Unauthorized(ApiResponse<object>.Fail("Unable to identify the current user."));
            if (!access.HasAccess) return Forbidden<object>();
            return null;
        }

        private ObjectResult Forbidden<T>() =>
            StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<T>.Fail("Access denied. You are not allowed to view or change student sensitive details."));

        private void NoStore()
        {
            Response.Headers.CacheControl = "no-store, no-cache";
            Response.Headers.Pragma = "no-cache";
        }
    }
}
