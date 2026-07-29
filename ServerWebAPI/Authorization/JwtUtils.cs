using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ServerWebAPI.Authorization;

public interface IJwtUtils
{
    public string GenerateToken(string user);
    public string? ValidateToken(string token);
}
public class JwtUtils : IJwtUtils
{
    private readonly IConfiguration _config;

    public JwtUtils(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(string user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user)
            }),
            Expires = DateTime.UtcNow.AddDays(1),

            Issuer = _config["Jwt:Issuer"],     // ✅ FIXED
            Audience = _config["Jwt:Audience"], // ✅ FIXED

            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),

                ValidateIssuer = true,
                ValidateAudience = true,

                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            //return jwtToken.Claims
            //    .First(x => x.Type == ClaimTypes.NameIdentifier)
            //    .Value;
        
            var userId = jwtToken.Claims
            .FirstOrDefault(x => x.Type == "nameid");
            return userId?.Value;
        }
            
        catch
        {
            return null;
        }

    }
}

//public class JwtUtils : IJwtUtils
//{
//    private readonly string _appSettings;

//    //public JwtUtils(IOptions<AppSettings> appSettings)
//    //{
//    //    _appSettings = appSettings.Value;
//    //}
//    public JwtUtils(IConfiguration appSettings)
//    {
//        _appSettings = appSettings["Jwt:Key"];
//    }
//    public string GenerateToken(string user)
//    {
//        // generate token that is valid for 7 days
//        var tokenHandler = new JwtSecurityTokenHandler();
//        var key = Encoding.ASCII.GetBytes(_appSettings);
//        var tokenDescriptor = new SecurityTokenDescriptor
//        {
//            Subject = new ClaimsIdentity(new[] { new Claim("id", user.ToString()) }),
//            Expires = DateTime.UtcNow.AddDays(1),
//            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
//        };
//        var token = tokenHandler.CreateToken(tokenDescriptor);
//        return tokenHandler.WriteToken(token);
//    }

//    public string? ValidateToken(string token)
//    {
//        if (token == null)
//            return null;

//        var tokenHandler = new JwtSecurityTokenHandler();
//        var key = Encoding.ASCII.GetBytes(_appSettings);
//        try
//        {
//            tokenHandler.ValidateToken(token, new TokenValidationParameters
//            {
//                ValidateIssuerSigningKey = true,
//                IssuerSigningKey = new SymmetricSecurityKey(key),
//                ValidateIssuer = false,
//                ValidateAudience = false,
//                // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
//                ClockSkew = TimeSpan.Zero
//            }, out SecurityToken validatedToken);

//            var jwtToken = (JwtSecurityToken)validatedToken;
//            var userId = jwtToken.Claims.First(x => x.Type == "id").Value;

//            // return user id from JWT token if validation successful
//            return userId;
//        }
//        catch
//        {
//            // return null if validation fails
//            return null;
//        }
//    }
//}

public class AppSettings
{
    public string Secret { get; set; }
}