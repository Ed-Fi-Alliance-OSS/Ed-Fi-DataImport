// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common;
using DataImport.Common.Helpers;
using DataImport.Models;
using DataImport.Web.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using DataImport.Common.Preprocessors;
using DataImport.Web.Services;
using NUglify.Css;
using NUglify.JavaScript;
using Serilog;
using DataImport.Common.Enums;
using DataImport.Web.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using DataImport.Web.Middleware;
using Microsoft.AspNetCore.Http.Features;
using System.Net.Http;

namespace DataImport.Web
{
    public delegate IFileService ResolveFileService(object key);
    public class NoLoggingCategoryPlaceHolder { }

    public class Startup
    {
        private IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration
        {
            get { return _configuration; }
            set
            {
                _configuration = value;
                var connectionStrings = Configuration.GetSection("ConnectionStrings").Get<ConnectionStrings>();
                var connectionStringsOptions = Options.Create(connectionStrings);
                Common.ExtensionMethods.FileExtensions.SetConnectionStringsOptions(connectionStringsOptions);

                var appSettings = Configuration.GetSection("AppSettings").Get<AppSettings>();
                ScriptExtensions.SetAppSettingsOptions(appSettings);
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.Configure<IdentitySettings>(Configuration.GetSection("IdentitySettings"));
            services.Configure<ConnectionStrings>(Configuration.GetSection("ConnectionStrings"));
            services.Configure<ExternalPreprocessorOptions>(_configuration.GetSection("ExternalPreprocessors"));
            services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
            services.Configure<FormOptions>(Configuration.GetSection("FormOptions")); services.AddSingleton<Microsoft.Extensions.Logging.ILogger>(sp => sp.GetService<ILogger<NoLoggingCategoryPlaceHolder>>());
            services.AddTransient<IFileSettings>(sp => sp.GetService<IOptions<AppSettings>>().Value);
            services.AddTransient<IPowerShellPreprocessSettings>(sp => sp.GetService<IOptions<AppSettings>>().Value);
            services.AddTransient<IEncryptionKeySettings>(sp => sp.GetService<IOptions<AppSettings>>().Value);
            services.AddTransient<IEncryptionKeyResolver, OptionsEncryptionKeyResolver>();

            services.AddAutoMapper(typeof(Startup));
            services.AddMediatR(typeof(Startup));
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            var databaseEngine = Configuration["AppSettings:DatabaseEngine"];

            if (DatabaseEngineEnum.Parse(databaseEngine).Equals(DatabaseEngineEnum.PostgreSql))
            {
                services.AddDbContext<DataImportDbContext, PostgreSqlDataImportDbContext>((s, options) =>
                 options.UseNpgsql(
                         s.GetRequiredService<IOptions<ConnectionStrings>>()
                             .Value.DefaultConnection));
            }
            else if (DatabaseEngineEnum.Parse(databaseEngine).Equals(DatabaseEngineEnum.SqlServer))
            {
                services.AddDbContext<DataImportDbContext, SqlDataImportDbContext>((s, options) =>
                  options.UseSqlServer(
                          s.GetRequiredService<IOptions<ConnectionStrings>>()
                              .Value.DefaultConnection));
            }

            services.AddTransient<IOAuthRequestWrapper, OAuthRequestWrapper>();

            //Configure MVC Razor Views under "FeatureFolder" and with compilation
            services.Configure<RazorViewEngineOptions>(options => options.ViewLocationExpanders.Add(new FeatureViewLocationExpander()))
                    .AddControllersWithViews(options =>
                    {
                        options.Filters.Add<MvcTransactionFilter>();
                        options.Filters.Add<MinimumRequiredSetupValidationFilter>();
                        options.Filters.Add<ValidatorActionFilter>();
                    })
                    .AddRazorRuntimeCompilation();

            services.AddSingleton<IHtmlHelperService, HtmlHelperService>();
            services.AddScoped<EdFiServiceManager>();
            services.AddScoped<IEnumerable<EdFiServiceBase>>(sp =>
            {
                return new EdFiServiceBase[] { sp.GetRequiredService<EdFiServiceV25>(), sp.GetRequiredService<EdFiServiceV311>() };
            });
            services.AddScoped<EdFiServiceV25>();
            services.AddScoped<EdFiServiceV311>();
            services.AddScoped<IConfigurationService, ConfigurationService>();
            services.AddScoped<Features.Shared.SelectListProviders.ResourceSelectListProvider>();
            services.AddScoped<Features.Shared.SelectListProviders.ApiServerSelectListProvider>();
            services.AddScoped<Features.Shared.SelectListProviders.ApiVersionSelectListProvider>();
            services.AddScoped<Services.Swagger.ISwaggerWebClient, Services.Swagger.SwaggerWebClient>();
            services.AddScoped<Services.Swagger.ISwaggerMetadataFetcher, Services.Swagger.SwaggerMetadataFetcher>();
            services.AddScoped<Services.Swagger.ISwaggerMetadataProcessor, Services.Swagger.SwaggerMetadataProcessorV1>();
            services.AddScoped<Services.Swagger.ISwaggerMetadataProcessor, Services.Swagger.SwaggerMetadataProcessorV2>();
            services.AddScoped<Services.Swagger.ISwaggerMetadataProcessor, Services.Swagger.SwaggerMetadataProcessorV3>();
            services.AddTransient<Features.Agent.AgentSelectListProvider>();
            services.AddScoped<IEncryptionService, EncryptionService>();
            services.AddSingleton<IClock, Clock>();
            services.AddScoped<IFileHelper, FileHelper>();
            services.AddTransient<AzureFileService>();
            services.AddTransient<LocalFileService>();
            services.AddTransient<ResolveFileService>(serviceProvider => key =>
            {
                return key switch
                {
                    Common.Enums.FileModeEnum.Azure => serviceProvider.GetService<AzureFileService>(),
                    Common.Enums.FileModeEnum.Local => serviceProvider.GetService<LocalFileService>(),
                    _ => throw new KeyNotFoundException(),
                };
            });
            services.AddScoped<Features.Shared.SelectListProviders.ScriptTypeSelectListProvider>();
            services.AddScoped<Features.Shared.SelectListProviders.PreprocessorSelectListProvider>();
            services.AddScoped<IPowerShellPreprocessorService, PowerShellPreprocessorService>();
            services.AddScoped<IExternalPreprocessorService, ExternalPreprocessorService>();
            services.AddScoped<Features.Share.SharingPreprocessorValidationService>();

            AssemblyScanner.FindValidatorsInAssembly(Assembly.GetExecutingAssembly())
                .ForEach(result =>
                {
                    services.AddScoped(result.InterfaceType, result.ValidatorType);
                });

            services.AddSingleton<Helpers.IJsonValidator, Helpers.JsonValidator>();
            services
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(Features.Shared.ApiServerSpecificRequestHandler<,>));
            services
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(Features.Shared.ApiVersionSpecificRequestHandler<,>));

            services.AddTransient<PowerShellPreprocessorOptionsResolver>();
            services.AddScoped(ctx =>
            {
                var resolver = ctx.GetService<PowerShellPreprocessorOptionsResolver>();
                return resolver.Resolve();
            });
            services.AddTransient(ctx =>
            {
                var fileMode = ctx.GetService<IOptions<AppSettings>>().Value.FileMode;
                var fileService = ctx.GetService<ResolveFileService>()(fileMode);
                var dbContext = ctx.GetService<DataImportDbContext>();
                return new PreprocessorMigration(fileService, dbContext);
            });

            services.AddMvc()
                .AddSessionStateTempDataProvider();
            services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();

            services.AddSession();

            services.AddWebOptimizer(pipeline =>
            {
                var minifyJsSettings = new CodeSettings
                {
                    LocalRenaming = LocalRenaming.CrunchAll,
                    MinifyCode = true
                };

                var minifyCssSettings = new CssSettings
                {
                    MinifyExpressions = true
                };

                pipeline.AddJavaScriptBundle("/bundles/jquery.min.js", minifyJsSettings, "/lib/jquery/dist/jquery.js");
                pipeline.AddJavaScriptBundle("/bundles/jqueryval.min.js", minifyJsSettings, "/lib/**/jquery.validate*");
                pipeline.AddJavaScriptBundle("/bundles/modernizr.min.js", minifyJsSettings, "/js/modernizr-2.8.3.js");
                pipeline.AddJavaScriptBundle("/bundles/bootstrap.min.js", minifyJsSettings, "/js/bootstrap.js", "/js/footable.js", "/js/respond.js");
                pipeline.AddJavaScriptBundle("/bundles/lodash.min.js", minifyJsSettings, "/js/lodash.js");
                pipeline.AddCssBundle("/content/dataimport.min.css", minifyCssSettings, "/css/footable.bootstrap.css", "/css/site.css");
                pipeline.AddJavaScriptBundle("/bundles/toastr.min.js", minifyJsSettings, "/js/toastr.js");
                pipeline.AddCssBundle("/content/toastr.min.css", "/css/toastr.min.css");
            });
            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DataImportDbContext dataImportDbContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePages(ctx =>
            {
                var response = ctx.HttpContext.Response;
                if (response.StatusCode == (int) HttpStatusCode.Unauthorized || response.StatusCode == (int) HttpStatusCode.Forbidden)
                {
                    var basePath = response.HttpContext.Request.PathBase;
                    var userUnauthorizedPath = "/Home/UserUnauthorized";
                    if (basePath.HasValue)
                    {
                        userUnauthorizedPath = $"{basePath}/Home/UserUnauthorized";
                    }
                    response.Redirect(userUnauthorizedPath);
                }
                return Task.CompletedTask;
            });

            var pathBase = Configuration.GetValue<string>("AppSettings:PathBase");
            if (!string.IsNullOrEmpty(pathBase))
            {
                app.UsePathBase("/" + pathBase.Trim('/'));
            }

            Log.Logger.Information("Migrating database");
            dataImportDbContext.Database.Migrate();

            app.UseHttpsRedirection();
            app.UseWebOptimizer();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();
            app.UseMiddleware<LoggingMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute().RequireAuthorization();
                endpoints.MapRazorPages();
            });
        }
    }
}
