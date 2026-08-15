using Microsoft.AspNetCore.Identity;
using Production_Erp_Web_App.Domain.Entities;

namespace Production_Erp_Web_App.DbApp
{
    public static class ApplicationDbContextSeed
    {
        public const string DefaultAdminEmail = "admin@production-erp.local";
        public const string DefaultAdminPassword = "Admin@12345";
        public static async Task SeedAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            foreach (var roleName in new[] { "Admin", "User" })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var existingAdmin = await userManager.FindByEmailAsync(DefaultAdminEmail);
            if (existingAdmin == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = DefaultAdminEmail,
                    Email = DefaultAdminEmail,
                    EmailConfirmed = true,
                    FullName = "Default Admin",
                    CreatedAtUtc = DateTime.UtcNow,
                };

                var result = await userManager.CreateAsync(admin, DefaultAdminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}
