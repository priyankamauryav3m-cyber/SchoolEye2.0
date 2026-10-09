using System;

namespace DomainModel.User
{
    // Logged-in user's own profile (api/MyProfile).
    // Only safe fields: never password, password hash, OTP or tokens.
    public class MyProfileResponse
    {
        public int UserSid { get; set; }
        public string? UserName { get; set; }
        public string? EmailId { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? MobileNo { get; set; }

        public string? RoleName { get; set; }
        public string? DashboardName { get; set; }

        public string? GroupCode { get; set; }
        public string? GroupName { get; set; }
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }

        public bool IsValid { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool RequiresTwoFactor { get; set; }
        public DateTime? LastVerifiedDateUtc { get; set; }
        public bool IsLockedOut { get; set; }
    }
}
