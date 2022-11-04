// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.IO;
using System.Reflection;
using DataImport.Common.Enums;
using DataImport.Common.Logging;
using DataImport.Web.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace DataImport.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = DefaultLogger.Build();

            try
            {
                Run(args);
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

        private static void Run(string[] args)
        {
            Log.Information("Building host");
            var host = CreateHostBuilder(args).Build();       

            Log.Information("Starting host");
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(ConfigureAppConfig)
                .UseSerilog(ConfigureLogger)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static void ConfigureAppConfig(HostBuilderContext context, IConfigurationBuilder config)
        {
            var runPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            var loggingConfigFile = Path.Combine(runPath ?? "./", "logging.json");
            var env = context.HostingEnvironment;
            config.AddJsonFile(loggingConfigFile, optional: false);

            var dbEngine = config.Build().GetValue<string>("AppSettings:DatabaseEngine");
            if (DatabaseEngineEnum.Parse(dbEngine).Equals(DatabaseEngineEnum.PostgreSql))
            {
                config.AddJsonFile(Path.Combine(runPath ?? "./", "logging_PgSql.json"), optional: false);
            }
            else if (DatabaseEngineEnum.Parse(dbEngine).Equals(DatabaseEngineEnum.SqlServer))
            {
                config.AddJsonFile(Path.Combine(runPath ?? "./", "logging_Sql.json"), optional: false);
            }

            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                  .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

            config.AddEnvironmentVariables();
        }

        private static void ConfigureLogger(HostBuilderContext context, LoggerConfiguration config)
        {
            config.ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.With<RequestInfoEnricher>()
                .Enrich.With<LevelShortNameEnricher>()
                .Enrich.WithMachineName();
        }
    }
}
