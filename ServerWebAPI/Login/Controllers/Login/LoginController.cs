using ApplicationInterface.SchoolMaster;
using ApplicationInterface.User;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using DomainModel.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;
using ServerWebAPI.Authorization;
using System.Security.Claims;
using IJwtUtils = ServerWebAPI.Authorization.IJwtUtils;

namespace ServerWebAPI.Login.Controllers.Login
{
    [ApiExplorerSettings(GroupName = "Login")]
    //[Authorize]
    [ApiController]
    [Route("api/users")]

    public class LoginController : ControllerBase
    {
        private readonly IUser _userService;
        private readonly IJwtUtils _ijwtUtils;
        private readonly IAuthService _authService;
        public LoginController(IUser userService, IJwtUtils jwtUtils, IAuthService authService)
        {
            _userService = userService;
            _ijwtUtils = jwtUtils;
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("Userlogin")]
        public async Task<IActionResult> Login([FromBody] UserModels userModel)
        {
            try
            {
                var user = await _userService.AuthenticateUser(userModel);
                if (user == null)
                {
                    return Unauthorized();
                }
                if (user.Username == null)
                {
                    return Ok(user);
                }
                else
                {
                    user.Token = _ijwtUtils.GenerateToken(user.UserId.ToString());
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("loginData")]
        public async Task<IActionResult> Logins([FromBody] UserModels userModel)
        {
            try
            {
                var user = await _userService.AuthenticateUser(userModel);

                if (user == null)
                {
                    return Ok(new ApiResponse<UserModels>
                    {
                        Success = false,
                        Message = "Invalid username or password.",
                        Data = null
                    });
                }

                if (!string.IsNullOrEmpty(user.Username))
                {
                    user.Token = _ijwtUtils.GenerateToken(user.UserId.ToString());
                }

                return Ok(new ApiResponse<UserModels>
                {
                    Success = true,
                    Message = "Login successful.",
                    Data = user
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<UserModels>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        [AllowAnonymous]
        [Route("UserloginData")]
        [HttpPost]
        public async Task<IActionResult> LoginData([FromBody] UserModels userModel)
        {
            try
            {
                var user = await _userService.AuthenticateUser(userModel);

                if (user == null)
                    return Unauthorized();

                if (user.Username == null)
                    return Ok(user);
                if (user.RequiresTwoFactor)
                {
                    bool trusted = await _userService.IsTrustedDeviceAsync(user.UserId, userModel.DeviceToken);
                    if (!trusted)
                    {
                        user.Token = null;
                        user.RequiresTwoFactor = false;
                        return Ok(user);
                    }
                    user.Token = _ijwtUtils.GenerateToken(user.UserId.ToString());
                    user.RequiresTwoFactor = true;
                    return Ok(user);
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [AllowAnonymous]
        [Route("SendOtp")]
        [HttpPost]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
        {
            try
            {
                var result = await _userService.GenerateAndSendOtpAsync(request.UserId, request.SendTo);

                switch (result)
                {
                    case 1:
                        return Ok(new ApiResponse<string>
                        {
                            Success = true,
                            Code = 1,
                            Message = $"OTP sent successfully to {request.SendTo}."
                        });

                    case 2:
                        return Ok(new ApiResponse<string>
                        {
                            Success = false,
                            Code = 2,
                            Message = "User not found."
                        });

                    default:
                        return Ok(new ApiResponse<string>
                        {
                            Success = false,
                            Code = 0,
                            Message = "Unable to send OTP."
                        });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Code = 500,
                    Message = ex.Message
                });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("VerifyOtp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto request)
        {
            try
            {
                var verifyResult = await _userService.VerifyOtpAsync(request.UserId, request.OtpCode);

                switch (verifyResult)
                {
                    case 1:

                        var user = await _userService.GetUserByIdAsync(request.UserId);

                        if (user == null)
                        {
                            return Unauthorized(new ApiResponse<UserModels>
                            {
                                Success = false,
                                Message = "User not found."
                            });
                        }

                        if (request.DeviceToken != Guid.Empty)
                        {
                            await _userService.SaveTrustedDeviceAsync(request.UserId, request.DeviceToken);
                        }

                        user.Token = _ijwtUtils.GenerateToken(user.UserId.ToString());

                        return Ok(new ApiResponse<UserModels>
                        {
                            Success = true,
                            Message = "OTP verified successfully.",
                            Data = user,
                            Code=1

                        });

                    case 2:
                        return Ok(new ApiResponse<UserModels>
                        {
                            Success = false,
                            Message = "OTP not found. Please resend OTP.",
                            Code=2
                        });

                    case 3:
                        return Ok(new ApiResponse<UserModels>
                        {
                            Success = false,
                            Message = "OTP has expired. Please resend OTP.",
                            Code=3
                        });

                    case 4:
                        return Ok(new ApiResponse<UserModels>
                        {
                            Success = false,
                            Message = "Invalid OTP.",
                            Code=4
                        });

                    default:
                        return Ok(new ApiResponse<UserModels>
                        {
                            Success = false,
                            Message = "OTP verification failed."
                        });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<UserModels>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("ResendOtp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequestDto request)
        {
            try
            {
                //var user = await _userService.GetUserByIdAsync(request.UserId);

                if (request == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        Code = 2,
                        Message = "User not found."
                    });
                }

                var result = await _userService.GenerateAndSendOtpAsync(request.UserId, request.SendTo);
                if (result == 1)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = true,
                        Code = 1,
                        Message = "OTP sent successfully."
                    });
                }

                return Ok(new ApiResponse<string>
                {
                    Success = false,
                    Code = result,
                    Message = "Unable to send OTP."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Code = 500,
                    Message = ex.Message
                });
            }
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("LoginGoogle")]
        public async Task<IActionResult> LoginGoogleFacebook([FromBody] GoogleLoginRequest UserEmail)
        {
            try
            {
                var user = await _userService.AuthenticateUserEmail(UserEmail.Email);
                if (user == null)
                {
                    return Unauthorized();
                }
                if (user.Username == null)
                {
                    return Unauthorized();
                    //return Ok(user);
                }
                else
                {
                    user.Token = _ijwtUtils.GenerateToken(user.UserId.ToString());
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("LoginV3M");
            return Ok();
        }
    }
    public class GoogleLoginRequest
    {
        public string Email { get; set; }
    }
}
