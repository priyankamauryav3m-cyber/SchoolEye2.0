using DomainModel.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using MyApp.Common;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;


namespace AuthUI
{
    public static class AccountEndpoints
    {
        private const string PendingCookieName = "SchoolEye.PendingTwoFactor";
        private const string ProtectorPurpose = "SchoolEye.Web.TwoFactorPending";

        private readonly static UserModels userModels = new();
        public static void Map(WebApplication app)
        {
            var group = app.MapGroup("/account").AllowAnonymous();

            group.MapPost("/login", async (HttpContext http, IHttpClientFactory httpFactory, IDataProtectionProvider dp) =>
            {
                var form = await http.Request.ReadFormAsync();
                var usernameOrEmail = form["Username"].ToString();
                var password = form["Password"].ToString();
                userModels.Username = usernameOrEmail;
                userModels.Password = password;

                var client = httpFactory.CreateClient("ServerWebAPI");
                var response = await client.PostAsJsonAsync("api/users/login", userModels);

                var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserModels>>();

                if (result is not { Success: true, Data: not null })
                {
                    var msg = result?.Message ?? "Unable to sign in. Please try again.";
                    return Results.Redirect($"/login?error={Uri.EscapeDataString(msg)}");
                }

                //if (!result.Data.RequiresTwoFactor)
                //{
                //    // 2FA disabled for this account — normally shouldn't happen with seed data,
                //    // but handled for completeness. We still need profile info to sign in, so we
                //    // route through the same OTP-less path by treating this like a passed OTP check.
                //    var verify = await client.PostAsJsonAsync("api/auth/verify-otp", new VerifyOtpRequestDto
                //    {
                //        UserId = result.Data.UserId,
                //        OtpCode = "000000"
                //    });
                //    // Falls through to the standard invalid-OTP message if the account unexpectedly
                //    // still required a code; in that case we simply route to /verify-otp instead.
                //    var verifyResult = await verify.Content.ReadFromJsonAsync<ApiResponse<AuthenticatedUserDto>>();
                //    if (verifyResult is { Success: true, Data: not null })
                //    {
                //        await SignInAsync(http, verifyResult.Data);
                //        return Results.Redirect("/");
                //    }
                //}

                var protector = dp.CreateProtector(ProtectorPurpose);
                var payload = protector.Protect($"{result.Data.UserId}|{DateTimeOffset.UtcNow.Add(TimeSpan.FromMinutes(6)):O}");
                http.Response.Cookies.Append(PendingCookieName, payload, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    MaxAge = TimeSpan.FromMinutes(6)
                });

                return Results.Redirect("/verify-otp");
            });

            group.MapPost("/verify-otp", async (HttpContext http, IHttpClientFactory httpFactory, IDataProtectionProvider dp) =>
            {
                var userId = TryReadPendingUserId(http, dp);
                if (userId is null)
                    return Results.Redirect("/login?error=" + Uri.EscapeDataString("Your session expired. Please sign in again."));

                var form = await http.Request.ReadFormAsync();
                var otpCode = form["OtpCode"].ToString();

                var client = httpFactory.CreateClient("AAFT.LMS.Api");
                var response = await client.PostAsJsonAsync("api/auth/verify-otp", new VerifyOtpRequestDto
                {
                    UserId = userId.Value,
                    OtpCode = otpCode
                });
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthenticatedUserDto>>();

                if (result is not { Success: true, Data: not null })
                {
                    var msg = result?.Message ?? "Verification failed. Please try again.";
                    return Results.Redirect($"/verify-otp?error={Uri.EscapeDataString(msg)}");
                }

                http.Response.Cookies.Delete(PendingCookieName);
                await SignInAsync(http, result.Data);
                return Results.Redirect("/");
            });

            group.MapPost("/resend-otp", async (HttpContext http, IHttpClientFactory httpFactory, IDataProtectionProvider dp) =>
            {
                var userId = TryReadPendingUserId(http, dp);
                if (userId is null)
                    return Results.Redirect("/login?error=" + Uri.EscapeDataString("Your session expired. Please sign in again."));

                var client = httpFactory.CreateClient("AAFT.LMS.Api");
                var response = await client.PostAsJsonAsync("api/auth/resend-otp", new ResendOtpRequestDto { UserId = userId.Value });
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();

                var query = result is { Success: true }
                    ? $"info={Uri.EscapeDataString(result.Message ?? "A new code has been sent.")}"
                    : $"error={Uri.EscapeDataString(result?.Message ?? "Could not resend the code.")}";
                return Results.Redirect($"/verify-otp?{query}");
            });

            group.MapPost("/logout", async (HttpContext http) =>
            {
                await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Results.Redirect("/login");
            });
        }

        private static async Task SignInAsync(HttpContext http, AuthenticatedUserDto user)
        {
            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.RoleName),
            new("UserCode", user.UserCode)
        };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }

        private static int? TryReadPendingUserId(HttpContext http, IDataProtectionProvider dp)
        {
            if (!http.Request.Cookies.TryGetValue(PendingCookieName, out var cookieValue) || string.IsNullOrEmpty(cookieValue))
                return null;

            try
            {
                var protector = dp.CreateProtector(ProtectorPurpose);
                var payload = protector.Unprotect(cookieValue);
                var parts = payload.Split('|');
                if (parts.Length != 2) return null;

                var expiry = DateTimeOffset.Parse(parts[1]);
                if (expiry < DateTimeOffset.UtcNow) return null;

                return int.Parse(parts[0]);
            }
            catch
            {
                return null;
            }
        }
    }

    public class VerifyOtpRequestDto
    {
        [Required]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Please enter the 6-digit verification code.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "The verification code must be exactly 6 digits.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "The verification code must contain digits only.")]
        public string OtpCode { get; set; } = string.Empty;
    }
}
