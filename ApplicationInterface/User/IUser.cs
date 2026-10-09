using DomainModel.User;
namespace ApplicationInterface.User
{
    public interface IUser
    {
        AuthenticateResponse User { get; }
        Task<bool> UserExists(string username);
        Task<UserModels> AuthenticateUser(UserModels userModel);
        Task<UserModels> AuthenticateUserEmail(string request);
        Task<UserModels> GetUser(string loginId);
        // true when the (valid) user has the given (valid) role, e.g. "V3MAdmin"
        Task<bool> IsUserInRoleAsync(string userSid, string roleName);
        // the logged-in user's own profile (safe fields only)
        Task<MyProfileResponse?> GetMyProfileAsync(int userSid);
        public List<UserDetails> GetUserDetails(string UserTypeId, int HospitalId, int VCID, int IsActive, string LoginName, string GroupCode);
        public Task<int> GenerateAndSendOtpAsync(int userId, string mobileNo);   
        Task<int> VerifyOtpAsync(int userId, string otpCode); 
        Task<UserModels?> GetUserByIdAsync(int userId);
        Task SaveTrustedDeviceAsync(int userId, Guid deviceToken);

         public Task<bool> IsTrustedDeviceAsync(int userId, Guid deviceToken);

        Task RemoveTrustedDeviceAsync(int userId, Guid deviceToken);
    }
}
