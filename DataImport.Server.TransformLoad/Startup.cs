// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common;
using DataImport.Common.Enums;
using DataImport.Common.ExtensionMethods;
using DataImport.Common.Helpers;
using DataImport.Common.Preprocessors;
using DataImport.Models;
using DataImport.Server.TransformLoad.Features.FileTransport;
using DataImport.Server.TransformLoad.Features.LoadResources;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace DataImport.Server.TransformLoad
{
    public delegate IFileServer ResolveFileServer(object key);
    public delegate IFileService ResolveFileService(object key);

    public class NoLoggingCategoryPlaceHolder { }

    public static class Startup
    {
        public static IHost ConfigureStaticGlobals(this IHost host)
        {
            FileExtensions.SetConnectionStringsOptions(host.Services.GetRequiredService<IOptions<ConnectionStrings>>());
            ScriptExtensions.SetAppSettingsOptions(host.Services.GetRequiredService<IPowerShellPreprocessSettings>());
            return host;
        }

        public static IServiceCollection ConfigureTransformLoadServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
            services.Configure<ConcurrencySettings>(configuration.GetSection("Concurrency"));
            services.Configure<ConnectionStrings>(configuration.GetSection("ConnectionStrings"));
            services.Configure<ExternalPreprocessorOptions>(configuration.GetSection("ExternalPreprocessors"));

            services.AddTransient<IFileSettings>(sp => sp.GetService<IOptions<AppSettings>>().Value);
            services.AddTransient<IPowerShellPreprocessSettings>(sp => sp.GetService<IOptions<AppSettings>>().Value);
            services.AddTransient<IEncryptionKeySettings>(sp => sp.GetService<IOptions<AppSettings>>().Value);
            services.AddTransient<IEncryptionKeyResolver, OptionsEncryptionKeyResolver>();
            services.AddSingleton<ILogger>(sp => sp.GetService<ILogger<NoLoggingCategoryPlaceHolder>>());
            var databaseEngine = configuration.GetSection("AppSettings")["DatabaseEngine"];

            if(DatabaseEngineEnum.Parse(databaseEngine).Equals(DatabaseEngineEnum.PostgreSql))
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

            services.AddHttpContextAccessor();

            services.AddSingleton<FileProcessor>();
            services.AddScoped<IHostedService, Application>();

            services.AddScoped<IFileHelper, FileHelper>();
            services.AddTransient<FtpsServer>();
            services.AddTransient<SftpServer>();
            services.AddTransient<ResolveFileServer>(serviceProvider => key =>
            {
                return key switch
                {
                    AgentTypeCodeEnum.FTPS => serviceProvider.GetService<FtpsServer>(),
                    AgentTypeCodeEnum.SFTP => serviceProvider.GetService<SftpServer>(),
                    _ => throw new KeyNotFoundException(),
                };
            });

            services.AddTransient<AzureFileService>();
            services.AddTransient<LocalFileService>();
            services.AddTransient<ResolveFileService>(serviceProvider => key =>
            {
                return key switch
                {
                    FileModeEnum.Azure => serviceProvider.GetService<AzureFileService>(),
                    FileModeEnum.Local => serviceProvider.GetService<LocalFileService>(),
                    _ => throw new KeyNotFoundException(),
                };
            });

            services.AddSingleton<IPowerShellPreprocessorService, PowerShellPreprocessorService>();
            services.AddTransient<PowerShellPreprocessorOptionsResolver>();
            services.AddSingleton<PowerShellPreprocessorOptions>(ctx =>
            {
                var resolver = ctx.GetRequiredService<PowerShellPreprocessorOptionsResolver>();
                return resolver.Resolve();
            });
            services.AddTransient<IExternalPreprocessorService, ExternalPreprocessorService>();
            services.AddTransient<IOAuthRequestWrapper, OAuthRequestWrapper>();

            services.AddMediatR(typeof(Startup));

            return services;
        }
    }
}
