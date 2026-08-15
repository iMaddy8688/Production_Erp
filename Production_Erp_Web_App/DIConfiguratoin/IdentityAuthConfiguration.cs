using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Production_Erp_Web_App.DbApp;
using Production_Erp_Web_App.Domain.Entities;
using Production_Erp_Web_App.Models;
using Production_Erp_Web_App.Services;
using System.Text;

namespace Production_Erp_Web_App.DIConfiguratoin
{
    public static class IdentityAuthConfiguration
    {
      
            /// <summary>
            /// Registers ASP.NET Core Identity (for password hashing, lockout,
            /// user/role storage in SQL Server) + JWT bearer authentication,
            /// wired so the JWT is read from an HttpOnly cookie instead of an
            /// Authorization header — this is a server-rendered Razor app, not
            /// an API, so there is no JS client attaching bearer headers.
            /// </summary>
            public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
            {
                services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
                var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()
                    ?? throw new InvalidOperationException("Missing 'Jwt' section in appsettings.json.");

                // AddIdentityCore (not the full AddIdentity) deliberately —
                // AddIdentity auto-registers its own cookie authentication
                // scheme as the default, which would silently take over from
                // the JWT bearer scheme we configure below.
                services.AddIdentityCore<ApplicationUser>(options =>
                {
                    options.Password.RequiredLength = 6;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                    options.User.RequireUniqueEmail = true;
                })
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddSignInManager()
                    .AddDefaultTokenProviders();

                services.AddScoped<ITokenService, TokenService>();

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                        ClockSkew = TimeSpan.Zero,
                    };

                    options.Events = new JwtBearerEvents
                    {
                        // Browsers don't send "Authorization: Bearer ..." headers
                        // on normal page navigations, so read the JWT out of the
                        // access_token cookie instead.
                        OnMessageReceived = context =>
                        {
                            if (context.Request.Cookies.TryGetValue("access_token", out var token) &&
                                !string.IsNullOrWhiteSpace(token))
                            {
                                context.Token = token;
                            }
                            return Task.CompletedTask;
                        },

                        // JwtBearerHandler normally just writes a bare 401/403
                        // response, which is correct for an API but useless for
                        // a browser. If the request looks like a normal page
                        // load (Accept: text/html), redirect to the Login page
                        // instead.
                        OnChallenge = context =>
                        {
                            if (WantsHtml(context.Request))
                            {
                                context.HandleResponse();
                                var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
                                context.Response.Redirect($"/Account/Login?returnUrl={returnUrl}");
                            }
                            return Task.CompletedTask;
                        },
                        OnForbidden = context =>
                        {
                            if (WantsHtml(context.Request))
                            {
                                context.Response.Redirect("/Account/AccessDenied");
                                return Task.CompletedTask;
                            }
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            return Task.CompletedTask;
                        },
                    };
                });

                // Secure by default: every action requires an authenticated
                // user unless it (or its controller) has [AllowAnonymous].
                services.AddAuthorization(options =>
                {
                    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                });

                return services;
            }

            private static bool WantsHtml(HttpRequest request)
            {
                var accept = request.Headers["Accept"].ToString();
                return string.IsNullOrEmpty(accept) || accept.Contains("text/html");
            }
        }
    }

