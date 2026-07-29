using System.ComponentModel.DataAnnotations;
namespace DomainModel.User
{
    public class UserModels
    {
        [Required(ErrorMessage = "Username is required")]
        public string? Username { get; set; } //during Login 

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
        public string? CapchaCode { get; set; }
        public string? Result { get; set; }
        public int UserId { get; set; }
        public string? EmailId { get; set; }
        public string? LoginName { get; set; }
        public string? GroupCode { get; set; }
        public string? BranchCode { get; set; }
        public int UserType { get; set; }
        public string? MobileNo { get; set; }
        public bool IsSecondLevel { get; set; }
        public string? RoleId { get; set; }
        public string? RoleName { get; set; }
        public string? UserCode { get; set; }
        public string? DisplayName { get; set; }
        public string? Token { get; set; }
        public string? RoleDashBoard { get; set; }
        public bool RequiresTwoFactor { get; set; }
        public Guid DeviceToken { get; set; }

    }

    public class AuthenticateRequest
    {
        public string? Username { get; set; } = default!;
        public string? Password { get; set; } = default!;
    }
    public class AuthenticateResponse
    {
        public string? Id { get; set; }
        public string? FirstName { get; set; }
        public string?LastName { get; set; }
        public string?UserID { get; set; }
        public string?UserName { get; set; }
        public string?UserType { get; set; }
        public string?Token { get; set; } = default!;
        public string?HospitalId { get; set; }
        public string? VCID { get; set; }
    }

    public class UserDetails
    {
        public int UM_RecordId { get; set; } = default!;

        [Required(ErrorMessage = "Please select a group.")]
        public int GroupId { get; set; } = default!;
        public string GroupCode { get; set; } = default!;
        public int HospitalId { get; set; } = default!;
        public int VCID { get; set; } = default!;
        public string HospitalName { get; set; } = default!;
        public string VisionCenterName { get; set; } = default!;
        public string UserTypeId { get; set; } = default!;
        public string LoginId { get; set; } = default!;
        public string LoginName { get; set; } = default!;
        public string LoginPassword { get; set; } = default!;
        public string Initial { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Gender { get; set; } = default!;
        public int UserAge { get; set; }
        public string EmployeeCode { get; set; } = default!;
        public int EmployeeQualification { get; set; } = default!;
        public string EmployeeDesignation { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string EmployeeEmail { get; set; } = default!;
        public int IdentityProof { get; set; } = default!;
        public string IdentityProofNo { get; set; } = default!;
        public string EmployeeAddress { get; set; } = default!;
        public string EmployeePinCode { get; set; } = default!;
        public string EmployeeImagePath { get; set; } = default!;
        public int IsActive { get; set; } = default!;
        public string EntryBy { get; set; } = default!;
        public string EntryDate { get; set; } = default!;
        public string UserType { get; set; } = default!;
        public string EmployeeRole { get; set; } = default!;

    }

    public class AuthenticatedUserDto
    {
        public int UserId { get; set; }
        public string UserCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool MustResetPassword { get; set; }
        public string Token { get; set; }
    }
    public class VerifyOtpRequestDto
    {
        [Required]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Please enter the 6-digit verification code.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "The verification code must be exactly 6 digits.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "The verification code must contain digits only.")]
        public string OtpCode { get; set; } = string.Empty;
        public Guid DeviceToken { get; set; }
    }
    public class ResendOtpRequestDto
    {
        [Required]
        public int UserId { get; set; }
        public string? SendTo { get; set; }
    }
    public class AuthUserRecord
    {
        public int UserId { get; set; }
        public string UserCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string PasswordSalt { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockoutEndUtc { get; set; }
        public bool MustResetPassword { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }

    // Generic result returned by usp_Auth_VerifyOtp
    public class CommandResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Please enter your username or email.")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your password.")]
        public string Password { get; set; } = string.Empty;
    }

    // Returned after STEP 1 (password check) - no token yet if 2FA is required
    public class LoginResultDto
    {
        public bool RequiresTwoFactor { get; set; }
        public int UserId { get; set; }
        public string? MaskedEmail { get; set; }
    }
    public class SendOtpRequest
    {
        public int UserId { get; set; }

        public string? Email { get; set; }

        public string? Mobile { get; set; }

        public string? SendTo { get; set; } 
    }
    public class TrustedDeviceModel
    {
        public int UserId { get; set; }

        public Guid DeviceToken { get; set; }

        public DateTime ExpiryUtc { get; set; }
    }
}
