using Microsoft.EntityFrameworkCore;
using Production_Erp_Web_App.DbApp;

namespace Production_Erp_Web_App.DIConfiguratoin
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'Default' is missing. Set it in appsettings.Development.json " +
                    "for local development, or via environment variable / user-secrets / Key Vault in " +
                    "other environments — never commit real production credentials to source control.");
            }

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sql =>
                    sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            return services;
        }
    }
}
