using ApplicationInterface.User;
using Dapper;
using DomainModel.User;
using Infrastructure.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.User
{

    public class UserService : IUser
    {
        SqlCommand cmd;
        StringBuilder sqlQry;
        DataAccessLayer objDAL;
        DataTable objTable;

        private readonly IConfiguration configuration;
        private readonly IConfiguration _configuration;
        private readonly IOtpSenderService _otpSender;
        private readonly string _connectionString;
        private readonly string _jwtKey;
        private readonly string _jwtIssuer;
        public AuthenticateResponse User { get; private set; }
        public UserService(IConfiguration configuration, IOtpSenderService otpSender)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _connectionString = configuration.GetValue<string>("DatabaseSettings1:ConnectionString")
                ?? throw new ArgumentNullException("DatabaseSettings1:ConnectionString");
            _jwtKey = configuration["Jwt:Key"]
                ?? throw new ArgumentNullException("Jwt:Key");
            _jwtIssuer = configuration["Jwt:Issuer"]
                ?? throw new ArgumentNullException("Jwt:Issuer");
            _otpSender = otpSender;
        }
        public async Task<bool> UserExists(string username)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT COUNT(*) FROM MstUsers WHERE Username = @Username", connection);
                command.Parameters.AddWithValue("@Username", username);
                var count = (int)await command.ExecuteScalarAsync();
                return count > 0;
            }
        }
        public async Task<UserModels> AuthenticateUser(UserModels request)
        {
            try
            {
                var passwordHash = HashPassword(request.Password);

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@UserName", request.Username);
                parameters.Add("@PasswordHash", passwordHash);
                var result = await connection.QueryFirstOrDefaultAsync<UserModels>(
                    "SP_AuthenticateUserLogin",
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 120   // ✅ PREVENTS TaskCanceledException
                );
                if (result == null)
                {
                    return new UserModels { Result = "-1" };
                }

                // If authentication failed
                if (result.Result != "1")
                {
                    return result;
                }
                // Success
                return result;
            }
            catch (TaskCanceledException)
            {
                // Timeout / cancellation
                return new UserModels { Result = "-2" };
            }
            catch (Exception ex)
            {
                // Optional: log ex
                return new UserModels { Result = "-1" };
            }
        }
        public async Task<UserModels> AuthenticateUserEmail(string request)
        {
            try
            {


                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@UserName", request);
                parameters.Add("@PasswordHash", null);
                var result = await connection.QueryFirstOrDefaultAsync<UserModels>(
                                    "SP_AuthenticateUserLogin",
                                    parameters,
                                    commandType: CommandType.StoredProcedure,
                                    commandTimeout: 120   // ✅ PREVENTS TaskCanceledException
                                );
                if (result == null)
                {
                    return new UserModels { Result = "-1" };
                }

                // If authentication failed
                if (result.Result != "1")
                {
                    return result;
                }
                // Success
                return result;
            }
            catch (TaskCanceledException)
            {
                // Timeout / cancellation
                return new UserModels { Result = "-2" };
            }
            catch (Exception ex)
            {
                // Optional: log ex
                return new UserModels { Result = "-1" };
            }
        }
        public async Task<int> GenerateAndSendOtpAsync(int userId, string email)
        {
            var otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            var expiry = DateTime.UtcNow.AddMinutes(2);
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@OtpCode", otp);
            parameters.Add("@ExpiryUtc", expiry);
            parameters.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
            await connection.ExecuteAsync("usp_Auth_SetOtp",parameters,
                commandType: CommandType.StoredProcedure);

            int result = parameters.Get<int>("@ReturnValue");

            if (result == 1)
            {
                await _otpSender.SendAsync(email, otp);
            }

            return result;
        }

        // ===================== NEW: OTP verify =====================
        public async Task<int> VerifyOtpAsync(int userId, string otpCode)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@OtpCode", otpCode);
                parameters.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
               await connection.QueryFirstOrDefaultAsync<CommandResult>("usp_Auth_VerifyOtp",
                    parameters, commandType: CommandType.StoredProcedure, commandTimeout: 120 );


                int result = parameters.Get<int>("@ReturnValue");
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error verifying OTP: {ex.Message}", ex);
            }
        }

        // ===================== NEW: user profile dobara laana (OTP success ke baad) =====================
        public async Task<UserModels?> GetUserByIdAsync(int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                var result = await connection.QueryFirstOrDefaultAsync<UserModels>(
                    "usp_Auth_GetById",
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 120
                );

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching user: {ex.Message}", ex);
            }
        }

        public async Task SaveTrustedDeviceAsync(int userId, Guid deviceToken)
        {
            using var con = new SqlConnection(_connectionString);

            await con.ExecuteAsync("usp_Auth_SaveTrustedDeviceNilesh",
                new
                {
                    UserId = userId,
                    DeviceToken = deviceToken,
                    ExpiryUtc = DateTime.UtcNow.AddYears(1)
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> IsTrustedDeviceAsync(int userId, Guid deviceToken)
        {
            using var con = new SqlConnection(_connectionString);

            var result = await con.QueryFirstOrDefaultAsync<CommandResult>(
                "usp_Auth_CheckTrustedDeviceNilesh",
                new
                {
                    UserId = userId,
                    DeviceToken = deviceToken
                },
                commandType: CommandType.StoredProcedure);

            return result?.Success ?? false;
        }

        public async Task RemoveTrustedDeviceAsync(int userId, Guid deviceToken)
        {
            using var con = new SqlConnection(_connectionString);

            await con.ExecuteAsync(
                "usp_Auth_RemoveTrustedDeviceNilesh",
                new
                {
                    UserId = userId,
                    DeviceToken = deviceToken
                },
                commandType: CommandType.StoredProcedure);
        }

        // ===================== EXISTING helper =====================

        private string HashPassword(string password)
        {
            return EncryptPassword(password, true);
            // Implement a secure hashing algorithm here, e.g., BCrypt
            // return password; // Example only, replace with a real hashing function
        }
        public string DecryptPassword(string cipherString, bool useHashing)
        {
            cipherString = cipherString.Replace("M3V", "/").Replace("V3M", "+");
            byte[] keyArray;
            //get the byte code of the string
            byte[] toEncryptArray = Convert.FromBase64String(cipherString);
            //System.Configuration.AppSettingsReader settingsReader = new System.Configuration.AppSettingsReader();
            ////Get your key from config file to open the lock!
            //string key = (string)settingsReader.GetValue("SecurityKey", typeof(String));
            string key = "VMMM";
            if (useHashing)
            {
                //if hashing was used get the hash code with regards to your key
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                //release any resource held by the MD5CryptoServiceProvider
                hashmd5.Clear();
            }
            else
            {
                //if hashing was not implemented get the byte code of the key
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
            }

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            //mode of operation. there are other 4 modes. 
            //We choose ECB(Electronic code Book)
            tdes.Mode = CipherMode.ECB;
            //padding mode(if any extra byte added)
            tdes.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(
            toEncryptArray, 0, toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor                
            tdes.Clear();
            //return the Clear decrypted TEXT
            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        public string EncryptPassword(string toEncryptPassword, bool useHashing)
        {
            byte[] keyArray;
            byte[] encryptedPasswordArray = UTF8Encoding.UTF8.GetBytes(toEncryptPassword);

            //System.Configuration.AppSettingsReader settingsReader = new System.Configuration.AppSettingsReader();
            //// Get the key from config file
            //string key = (string)settingsReader.GetValue("SecurityKey",typeof(String));
            string key = "VMMM";
            //If hashing use get hashcode regards to your key
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));

                //Always release the resources and flush data
                // of the Cryptographic service provide. Best Practice
                hashmd5.Clear();
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;

            //mode of operation. there are other 4 modes.
            //We choose ECB(Electronic code Book)
            tdes.Mode = CipherMode.ECB;

            //padding mode(if any extra byte added)
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            //transform the specified region of bytes array to resultArray

            byte[] resultArray = cTransform.TransformFinalBlock(encryptedPasswordArray, 0, encryptedPasswordArray.Length);
            //Release resources held by TripleDes Encryptor

            tdes.Clear();

            //Return the encrypted data into unreadable string format
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public async Task<UserModels?> GetUser(string loginId)
        {
            UserModels response = null;
            DataAccessLayer dbClass = new DataAccessLayer(_connectionString);
            StringBuilder sbSQL = new StringBuilder();


            SqlCommand oCmd = new SqlCommand();
            oCmd.Parameters.AddWithValue("@UserSid", loginId);
            DataTable oTable = dbClass.GetV3MSyncDataTable("select UserSid,UserName,GroupCode,BranchCode " +
                        "from MSTUSERS where upper(UserSid)=@UserSid ", oCmd);

            if (oTable != null && oTable.Rows.Count > 0)
            {
                response = new UserModels();
                response.UserId = int.Parse(oTable.Rows[0]["UserSid"].ToString() + "");
                response.LoginName = oTable.Rows[0]["UserName"].ToString();
                response.GroupCode = oTable.Rows[0]["GroupCode"].ToString() + "";
                response.BranchCode = oTable.Rows[0]["BranchCode"].ToString() + "";

            }
            return response;
        }

        public List<UserDetails> GetUserDetails(string UserTypeId, int HospitalId, int VCID, int IsActive, string LoginName, string GroupCode)
        {
            List<UserDetails> objUserlst = new List<UserDetails>();
            UserDetails oUserfields = null;
            DataAccessLayer dbClass = new DataAccessLayer(_connectionString);

            try
            {
                sqlQry = new StringBuilder();
                cmd = new SqlCommand();
                sqlQry.Append(" Select mu.UM_RecordId,mu.GroupId,mu.HospitalId,mu.VCID, mh.HospitalName,mvc.VisionCenterName, " +
                    " mu.UserTypeId,mu.LoginId,mu.LoginName,dbo.SQLV3MDecrypt(mg.GroupCode, mu.LoginPassword) as 'LoginPassword' ,mu.FirstName,ISNULL(mu.LastName,'') as 'LastName', " +
                    " mu.Gender,mu.UserAge,mu.EmployeeCode,isnull(EmployeeQualification,'') as 'EmployeeQualification' , isnull(Initial,'') as 'Initial'," +
                    " isnull(EmployeeDesignation,'') as 'EmployeeDesignation',isnull(RoleId,'') as 'RoleId',ISNULL(mu.PhoneNumber,'') as 'PhoneNumber', " +
                    " isnull(mu.EmployeeEmail,'') as 'EmployeeEmail',ISNULL(IdentityProof,'') as 'IdentityProof',ISNULL(IdentityProofNo,'')  as 'IdentityProofNo'," +
                    " isnull(EmployeeAddress,'') as 'EmployeeAddress',ISNULL(EmployeePinCode,'') as 'EmployeePinCode',mu.IsActive,mu.EntryBy , " +
                    " mut.UserTypeName " +
                    " from MS_User mu " +
                    " inner join MS_UserType mut on mu.UserTypeId=mut.UTM_UserTypeId " +
                    " inner join MS_Group mg on mg.GroupId=mu.GroupId" +
                    " left join MS_Hospital mh on mh.HospitalId=mu.HospitalId " +
                    " left join MS_VisionCenter mvc on mvc.VisionCenterId=mu.VCID " +
                     "where 1 = 1");
                if (UserTypeId != "0" && UserTypeId != "" && UserTypeId != null)
                {
                    sqlQry.Append(" and UserTypeId=@UserTypeId ");
                    cmd.Parameters.AddWithValue("@UserTypeId", UserTypeId);
                }
                if (HospitalId != 0)
                {
                    sqlQry.Append(" and mu.HospitalId=@HospitalId ");
                    cmd.Parameters.AddWithValue("@HospitalId", HospitalId);
                }
                if (VCID != 0)
                {
                    sqlQry.Append(" and mu.VCID=@VCID ");
                    cmd.Parameters.AddWithValue("@VCID", VCID);
                }
                if (LoginName != "" && LoginName != "0" && LoginName != null && !String.IsNullOrEmpty(LoginName.Trim()))
                {
                    sqlQry.Append(" and LoginName like '%' +@LoginName+ '%' ");
                    cmd.Parameters.AddWithValue("@LoginName", LoginName);
                }
                if (IsActive == 1 || IsActive == 0)
                {
                    sqlQry.Append(" and mu.IsActive=@IsActive ");
                    cmd.Parameters.AddWithValue("@IsActive", IsActive);
                }
                if (!string.IsNullOrEmpty(GroupCode) && GroupCode != "0")
                {
                    sqlQry.Append(" and mg.GroupCode=@GroupCode ");
                    cmd.Parameters.AddWithValue("@GroupCode", GroupCode);
                }
                objTable = new DataTable();
                objTable = dbClass.GetV3MSyncDataTable(sqlQry.ToString(), cmd);
                if (objTable != null && objTable.Rows.Count > 0)
                {
                    for (int index = 0; index < objTable.Rows.Count; index++)
                    {
                        oUserfields = new UserDetails();
                        oUserfields.UM_RecordId = int.Parse(objTable.Rows[index]["UM_RecordId"].ToString() + "");
                        oUserfields.GroupId = int.Parse(objTable.Rows[index]["GroupId"].ToString() + "");
                        oUserfields.HospitalId = int.Parse(objTable.Rows[index]["HospitalId"].ToString() + "");
                        oUserfields.VCID = int.Parse(objTable.Rows[index]["VCID"].ToString() + "");
                        oUserfields.HospitalName = objTable.Rows[index]["HospitalName"].ToString() + "";
                        oUserfields.VisionCenterName = objTable.Rows[index]["VisionCenterName"].ToString() + "";
                        oUserfields.UserTypeId = objTable.Rows[index]["UserTypeId"].ToString() + "";
                        oUserfields.LoginId = objTable.Rows[index]["LoginId"].ToString() + "";
                        oUserfields.LoginName = objTable.Rows[index]["LoginName"].ToString() + "";
                        oUserfields.LoginPassword = objTable.Rows[index]["LoginPassword"].ToString() + "";
                        oUserfields.FirstName = objTable.Rows[index]["FirstName"].ToString() + "";
                        oUserfields.LastName = objTable.Rows[index]["LastName"].ToString() + "";
                        oUserfields.Gender = objTable.Rows[index]["Gender"].ToString() + "";
                        oUserfields.UserAge = int.Parse(objTable.Rows[index]["UserAge"].ToString() + "");
                        oUserfields.EmployeeCode = objTable.Rows[index]["EmployeeCode"].ToString() + "";
                        oUserfields.EmployeeQualification = int.Parse(objTable.Rows[index]["EmployeeQualification"].ToString() + "");
                        oUserfields.Initial = objTable.Rows[index]["Initial"].ToString() + "";
                        oUserfields.EmployeeDesignation = objTable.Rows[index]["EmployeeDesignation"].ToString() + "";
                        oUserfields.EmployeeRole = objTable.Rows[index]["RoleId"].ToString() + "";
                        oUserfields.PhoneNumber = objTable.Rows[index]["PhoneNumber"].ToString() + "";
                        oUserfields.EmployeeEmail = objTable.Rows[index]["EmployeeEmail"].ToString() + "";
                        oUserfields.IdentityProof = int.Parse(objTable.Rows[index]["IdentityProof"].ToString() + "");
                        oUserfields.IdentityProofNo = objTable.Rows[index]["IdentityProofNo"].ToString() + "";
                        oUserfields.EmployeeAddress = objTable.Rows[index]["EmployeeAddress"].ToString() + "";
                        oUserfields.EmployeePinCode = objTable.Rows[index]["EmployeePinCode"].ToString() + "";
                        oUserfields.IsActive = Convert.ToInt32(objTable.Rows[0]["IsActive"]);
                        oUserfields.EntryBy = objTable.Rows[index]["EntryBy"].ToString() + "";
                        oUserfields.UserType = objTable.Rows[index]["UserTypeName"].ToString() + "";
                        objUserlst.Add(oUserfields);
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return objUserlst;
        }
    }
}
