using Microsoft.AspNetCore.Identity;
using Production_Erp_Web_App.DbApp;
using Production_Erp_Web_App.DIConfiguratoin;
using Production_Erp_Web_App.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

// UseAuthentication MUST come before UseAuthorization — without it,
// [Authorize]/the fallback auth policy never gets to see who the user is,
// because nothing has read the JWT out of the access_token cookie yet.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

// Development-only: seed the Admin/User roles and a default admin login
// (admin@production-erp.local / Admin@12345 — change it after first login).
// Requires the database + Identity tables to already exist, i.e. you've run
// `dotnet ef database update` first. Never runs outside Development.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await ApplicationDbContextSeed.SeedAsync(userManager, roleManager);
}

app.Run();
