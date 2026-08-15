using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Production_Erp_Web_App.Domain.Entities;
using Production_Erp_Web_App.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Production_Erp_Web_App.Services
{
    public class TokenService:ITokenService
    {
        private readonly JwtSettings _settings;
        public TokenService(IOptions<JwtSettings> options)
        {
            _settings = options.Value;
        }

        public string GenerateAccessToken(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("fullName", user.FullName ?? string.Empty),
                // ClaimTypes.Name is what User.Identity.Name reads from in
                // Razor views — set it to the email so "@User.Identity.Name"
                // works in _Layout.cshtml without extra lookups.
                new(ClaimTypes.Name, user.Email ?? string.Empty),
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
              issuer: _settings.Issuer,
              audience: _settings.Audience,
              claims: claims,
              expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes),
              signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        public string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        public string HashToken(string rawToken)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToHexString(bytes);
        }
    }
}
