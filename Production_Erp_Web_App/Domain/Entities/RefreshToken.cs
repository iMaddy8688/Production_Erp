namespace Production_Erp_Web_App.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string UserId { get; set; } = default!;

        public string TokenHash { get; set; } = default!;

        public DateTime ExpiresAtUtc { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public string? CreatedByIp { get; set; }

        public DateTime? RevokedAtUtc { get; set; }

        /// <summary>
        /// Set when this token was rotated out for a newer one, so a reused
        /// (already-rotated) refresh token can be detected as a possible
        /// theft and the whole chain revoked.
        /// </summary>
        public string? ReplacedByTokenHash { get; set; }

        public bool IsActive => RevokedAtUtc == null && DateTime.UtcNow < ExpiresAtUtc;
    }
}
