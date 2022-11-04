// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;

namespace DataImport.Server.TransformLoad.Tests
{
    [SetUpFixture]
    public class SetUpFixture
    {
        [OneTimeSetUp]
        public void GlobalSetUp()
        {
            // Migrations MUST run first, before any logging attempts,
            // so that database logging can be properly initialized.
            using (var scope = Testing.Services.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetRequiredService<DataImportDbContext>())
                {
                    context.Database.Migrate();

                    //Populate data for tests.
                    if (!context.ApiServers.Any())
                    {
                        context.ApiServers.Add(
                            new ApiServer
                            {
                                Name = "Default API Connection",
                                Key = Guid.NewGuid()
                                          .ToString(),
                                Secret = "TestData",
                                TokenUrl = "https://testtestest/oauth/token/",
                                Url = "https://testestest/",
                                ApiVersion = new ApiVersion { Version = "VersionTest" }
                            });
                        context.SaveChanges();
                    }
                }
            }

            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("DataImport", LogLevel.Debug)
                    .AddConsole();
            });
            var logger = loggerFactory.CreateLogger<SetUpFixture>();
            logger.LogInformation("{assembly} Starting", Assembly.GetExecutingAssembly().GetName().Name);
        }
    }
}
