// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.IO;
using DataImport.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DataImport.Common.Logging;
using Microsoft.Extensions.Configuration;
using Serilog;
using DataImport.Common.Enums;

namespace DataImport.Server.TransformLoad
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = DefaultLogger.Build();

            try
            {
                await Run(args);
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static async Task Run(string[] args)
        {
            Log.Information("Building host");
            var host = CreateHostBuilder(args);
            host.ConfigureServices(
                (context, services) => services.ConfigureTransformLoadServices(context.Configuration));

            host.UseConsoleLifetime();

            using var builtHost = host.Build().ConfigureStaticGlobals();

            var cancellationTokenSource = new CancellationTokenSource();

            Log.Information("Migrating database");
            var context = builtHost.Services.GetRequiredService<DataImportDbContext>();
            await context.Database.MigrateAsync(cancellationToken: cancellationTokenSource.Token);

            var assembly = Assembly.GetExecutingAssembly();
            var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                               ?.InformationalVersion;

            //Use full logger after DB migration
            var logger = builtHost.Services.GetService<ILogger<Program>>();
            logger.LogInformation("{name} {version} Starting", assembly.GetName().Name, informationalVersion);

            // Force TLS 1.2, resolving an error in Azure where all calls between DataImport and ODS API fail
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Not using run because this process is done when this returns.
            //If you do the host wait, it deadlocks since the host already completed and nothing signals the wait task.
            Log.Information("Starting host");
            await builtHost.StartAsync(cancellationTokenSource.Token);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host
                .CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(ConfigureAppConfig)
                .UseSerilog(ConfigureLogger);

        private static void ConfigureAppConfig(HostBuilderContext context, IConfigurationBuilder config)
        {
            var runPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            var loggingConfigFile = Path.Combine(runPath ?? "./", "logging.json");
            var env = context.HostingEnvironment;
            config.Sources.Clear();

            config.AddJsonFile(loggingConfigFile, optional: false)
                  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                  .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

            var dbEngine = config.Build().GetValue<string>("AppSettings:DatabaseEngine");
            if (DatabaseEngineEnum.Parse(dbEngine).Equals(DatabaseEngineEnum.PostgreSql))
            {
                config.AddJsonFile(Path.Combine(runPath ?? "./", "logging_PgSql.json"), optional: false);
            }
            else if (DatabaseEngineEnum.Parse(dbEngine).Equals(DatabaseEngineEnum.SqlServer))
            {
                config.AddJsonFile(Path.Combine(runPath ?? "./", "logging_Sql.json"), optional: false);
            }

            config.AddEnvironmentVariables();
        }

        private static void ConfigureLogger(HostBuilderContext context, LoggerConfiguration config)
        {
            config.ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.With<EmptyRequestInfoEnricher>()
                .Enrich.With<LevelShortNameEnricher>()
                .Enrich.With<IngestionLogEnricher>()
                .Enrich.WithMachineName();
        }
    }
}
