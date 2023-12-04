// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.IO;
using System.Reflection;

namespace DataImport.Models.Tests
{
    [SetUpFixture]
    public class SetUpFixture
    {
        [OneTimeSetUp]
        public void GlobalSetUp()
        {
            var configuration = new ConfigurationBuilder()
                  .SetBasePath(Directory.GetCurrentDirectory())
                  .AddJsonFile("appsettings.json", false)
                  .AddEnvironmentVariables()
                  .Build();

            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("DataImport", LogLevel.Debug)
                    .AddConsole();
            });
            var logger = loggerFactory.CreateLogger<SetUpFixture>();
            var dbContextLogger = loggerFactory.CreateLogger<DataImportDbContext>();

            var connectionStrings = configuration.GetSection("ConnectionStrings").Get<ConnectionStrings>();
            var optionsBuilder = new DbContextOptionsBuilder<SqlDataImportDbContext>();
            optionsBuilder.UseSqlServer(connectionStrings.DefaultConnection);
            var connectionStringsOptions = Microsoft.Extensions.Options.Options.Create(connectionStrings);

            // Migrations MUST run first, before any logging attempts,
            // so that database logging can be properly initialized.
            using (var context = new SqlDataImportDbContext(dbContextLogger, optionsBuilder.Options))
            {
                context.Database.EnsureDeleted();
                context.Database.Migrate();
            }

            logger.LogInformation("{assembly} Starting", Assembly.GetExecutingAssembly().GetName().Name);
        }
    }
}
