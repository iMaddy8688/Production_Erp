using Production_Erp_Web_App.Domain.Entities;

namespace Production_Erp_Web_App.Services
{
    public interface ITokenService
    {
        /// <summary>Creates a signed JWT access token for this user.</summary>
        string GenerateAccessToken(ApplicationUser user, IList<string> roles);

        /// <summary>Creates a new random opaque refresh token (raw value — caller stores only its hash).</summary>
        string GenerateRefreshToken();

        /// <summary>SHA-256 hashes a raw token value for safe storage/comparison.</summary>
        string HashToken(string rawToken);
    }
}
