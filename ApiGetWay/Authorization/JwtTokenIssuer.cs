using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ApiGetWay.Authorization
{
    public sealed class JwtTokenIssuer
    {
        private readonly JwtOptions _options;
        private readonly UserManager<AuthenticateUser> _userManager;

        public JwtTokenIssuer(IOptions<JwtOptions> options, UserManager<AuthenticateUser> userManager)
        {
            _options = options.Value; 
            _userManager = userManager;
        }

        public async Task<string> CreateAccessTokenAsync(AuthenticateUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
    public sealed class JwtOptions
    {
        public string Issuer { get; init; } = "";
        public string Audience { get; init; } = "";
        public string SigningKey { get; init; } = "";
        public int AccessTokenMinutes { get; init; } = 10;
    }
}
