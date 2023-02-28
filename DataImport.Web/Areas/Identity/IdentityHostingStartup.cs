// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.Models.Identity;
using DataImport.Web.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataImport.Web.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

[assembly: HostingStartup(typeof(DataImport.Web.Areas.Identity.IdentityHostingStartup))]
namespace DataImport.Web.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                var appSettings = context.Configuration.GetSection("AppSettings").Get<AppSettings>();
                var identitySettings = context.Configuration.GetSection("IdentitySettings").Get<IdentitySettings>();

                if (identitySettings.Type.Equals(IdentitySettingsConstants.EntityFrameworkIdentityType))
                    ConfigureForEntityFrameworkIdentity(services, appSettings);
                else if (identitySettings.Type.Equals(IdentitySettingsConstants.OpenIdIdentityType))
                    ConfigureForOpenIdConnectIdentity(services, identitySettings);
                else
                    throw new Exception($"Unsupported identity type: {identitySettings.Type}. Valid options are {IdentitySettingsConstants.EntityFrameworkIdentityType} or {IdentitySettingsConstants.OpenIdIdentityType}");
            });
        }

        private static void ConfigureForEntityFrameworkIdentity(IServiceCollection services, AppSettings appSettings)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>(
                    opt =>
                    {
                        opt.User.RequireUniqueEmail = true;
                        opt.SignIn.RequireConfirmedEmail = false;
                    })
                .AddEntityFrameworkStores<DataImportDbContext>()
                //.AddDefaultUI() //Restore this line to re-include default Identity Pages
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                // Default Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);

                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireUppercase = true;
            });

            services.ConfigureApplicationCookie(config =>
            {
                config.Cookie.Name = "DataImportCookie";
                config.LoginPath = "/Account/Login"; // User defined login path
                config.ExpireTimeSpan = TimeSpan.FromMinutes(appSettings.LoginTimeoutInMinutes);
            });

            //Configure RazorPages (Identity) to be hosted off of root and handle disabling routes.
            services.AddRazorPages(o => o.Conventions.Add(new IdentityToRootPageRouteModelConvention()));
        }

        private void ConfigureForOpenIdConnectIdentity(IServiceCollection services, IdentitySettings identitySettings)
        {
            var openIdSettings = identitySettings.OpenIdSettings;
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = openIdSettings.AuthenticationScheme;
            })
            .AddCookie(
                options =>
                {
                    options.LoginPath = "/OpenIdConnect/Login";
                    options.LogoutPath = "/OpenIdConnect/Logout";
                    options.AccessDeniedPath = "/OpenIdConnect/Login";

                    options.Events.OnRedirectToAccessDenied = context =>
                    {
                        context.HttpContext.Response.StatusCode = 401;
                        return Task.FromResult(Task.CompletedTask);
                    };
                })
            .AddOpenIdConnect(openIdSettings.AuthenticationScheme, options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                options.Authority = openIdSettings.Authority;

                options.ClientId = openIdSettings.ClientId;
                options.ClientSecret = openIdSettings.ClientSecret;
                options.ResponseType = openIdSettings.ResponseType;

                options.Scope.Clear();
                foreach (var scope in openIdSettings.Scopes)
                    options.Scope.Add(scope);

                options.SaveTokens = openIdSettings.SaveTokens;
                options.RequireHttpsMetadata = openIdSettings.RequireHttpsMetadata;
                options.GetClaimsFromUserInfoEndpoint = openIdSettings.GetClaimsFromUserInfoEndpoint;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = openIdSettings.ClaimTypeMappings.NameClaimType,
                    RoleClaimType = openIdSettings.ClaimTypeMappings.RoleClaimType
                };

                options.Events.OnTicketReceived = async context => await TranslateOidcClaims(context);
            });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireClaim(ClaimTypes.Role, IdentitySettingsConstants.RoleClaimValue)
                    .Build();
            });

            Task TranslateOidcClaims(TicketReceivedContext context)
            {
                var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;

                bool IsReservedClaim(string claimType) => new[] { ClaimTypes.NameIdentifier, ClaimTypes.Name, ClaimTypes.Email, ClaimTypes.Role }.Contains(claimType);
                void ReplaceClaimIfNotNull(string oidcClaimType, string diClaimType)
                {
                    var claim = claimsIdentity.Claims.FirstOrDefault(m => m.Type == oidcClaimType);
                    if (oidcClaimType != diClaimType && claim != null && !string.IsNullOrEmpty(claim.Value))
                    {
                        claimsIdentity.AddClaim(new Claim(diClaimType, claim.Value));
                        if (!IsReservedClaim(claim.Type))
                            claimsIdentity.RemoveClaim(claim);
                    }
                }

                if (claimsIdentity != null)
                {
                    ReplaceClaimIfNotNull(openIdSettings.ClaimTypeMappings.IdentifierClaimType, ClaimTypes.NameIdentifier);
                    ReplaceClaimIfNotNull(openIdSettings.ClaimTypeMappings.NameClaimType, ClaimTypes.Name);
                    ReplaceClaimIfNotNull(openIdSettings.ClaimTypeMappings.EmailClaimType, ClaimTypes.Email);

                    var roleClaims = claimsIdentity.Claims.Where(m => m.Type == openIdSettings.ClaimTypeMappings.RoleClaimType).ToList();
                    if (roleClaims.Any(r => r.Value == IdentitySettingsConstants.RoleClaimValue))
                    {
                        foreach (var otherClaim in roleClaims) { claimsIdentity.RemoveClaim(otherClaim); }
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, IdentitySettingsConstants.RoleClaimValue));
                    }
                }

                return Task.CompletedTask;
            }
        }
    }
}
