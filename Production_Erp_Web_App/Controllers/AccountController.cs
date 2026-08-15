using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Production_Erp_Web_App.DbApp;
using Production_Erp_Web_App.Domain.Entities;
using Production_Erp_Web_App.Models;
using Production_Erp_Web_App.Services;

namespace Production_Erp_Web_App.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly ApplicationDbContext _db;
        private readonly JwtSettings _jwtSettings;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            ApplicationDbContext db,
            Microsoft.Extensions.Options.IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _db = db;
            _jwtSettings = jwtSettings.Value;
        }
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            // CheckPasswordSignInAsync only verifies the password and
            // enforces lockout — it does NOT sign the user in via cookies.
            // We issue our own JWT + refresh token below instead.
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "This account is locked due to multiple failed login attempts. Try again later.");
                return View(model);
            }

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            await IssueTokensAsync(user);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }
            return RedirectToAction("Index", "Dashboard");
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
            {
                ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                CreatedAtUtc = DateTime.UtcNow,
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // TEMPORARY: until the roles/permissions module exists, every
            // self-registered user is made an Admin so the app is usable.
            // Lock this down (e.g. default to a low-privilege "User" role,
            // with an Admin approving upgrades) once there's a real Roles
            // screen — see the design documents, Authorization section.
            await _userManager.AddToRoleAsync(user, "Admin");

            await IssueTokensAsync(user);

            return RedirectToAction("Index", "Dashboard");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            if (Request.Cookies.TryGetValue("refresh_token", out var rawRefreshToken) &&
                !string.IsNullOrWhiteSpace(rawRefreshToken))
            {
                var hash = _tokenService.HashToken(rawRefreshToken);
                var stored = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash);
                if (stored != null && stored.RevokedAtUtc == null)
                {
                    stored.RevokedAtUtc = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }
            }

            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");

            return RedirectToAction("Login");
        }
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
        private async Task IssueTokensAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var rawRefreshToken = _tokenService.GenerateRefreshToken();

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = _tokenService.HashToken(rawRefreshToken),
                ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays),
                CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
            });
            await _db.SaveChangesAsync();

            var isHttps = Request.IsHttps;

            Response.Cookies.Append("access_token", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = isHttps,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes),
            });

            Response.Cookies.Append("refresh_token", rawRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = isHttps,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenDays),
            });
        }
      
    }
}
