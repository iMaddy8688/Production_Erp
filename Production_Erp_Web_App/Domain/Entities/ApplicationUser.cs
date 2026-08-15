using Microsoft.AspNetCore.Identity;

namespace Production_Erp_Web_App.Domain.Entities
{
    public class ApplicationUser: IdentityUser
    {
        public string FullName { get; set; } = default!;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
