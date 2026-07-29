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
        public List<UserDetails> GetUserDetails(string UserTypeId, int HospitalId, int VCID, int IsActive, string LoginName, string GroupCode);
        public Task<int> GenerateAndSendOtpAsync(int userId, string mobileNo);   
        Task<int> VerifyOtpAsync(int userId, string otpCode); 
        Task<UserModels?> GetUserByIdAsync(int userId);
        Task SaveTrustedDeviceAsync(int userId, Guid deviceToken);

         public Task<bool> IsTrustedDeviceAsync(int userId, Guid deviceToken);

        Task RemoveTrustedDeviceAsync(int userId, Guid deviceToken);
    }
}
