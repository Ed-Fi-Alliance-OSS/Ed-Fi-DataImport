// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;
using static DataImport.Web.Tests.Testing;

namespace DataImport.Web.Tests
{
    [SetUpFixture]
    public class SetUpFixture
    {

        [OneTimeSetUp]
        public async Task GlobalSetUp()
        {
            // Migrations MUST run first, before any logging attempts,
            // so that database logging can be properly initialized.

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            using (var context = Testing.Services.GetRequiredService<SqlDataImportDbContext>())
            {
                if (RunningUnderTeamCity())
                    context.Database.EnsureDeleted();

                context.Database.Migrate();
            }

            Log.Information(Assembly.GetExecutingAssembly().GetName().Name + " Starting");

            await ConfigureForOdsApiV311();
        }

        private static bool RunningUnderTeamCity()
            => Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null;
    }
}