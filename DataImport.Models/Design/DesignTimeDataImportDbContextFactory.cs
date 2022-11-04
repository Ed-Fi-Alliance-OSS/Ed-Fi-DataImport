// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace DataImport.Models.Design
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/ef/core/cli/dbcontext-creation?tabs=dotnet-core-cli#from-a-design-time-factory
    /// </summary>
    public class DesignTimePostgreSqlDataImportDbContextFactory : DesignTimeDataImportDbContextFactoryBase, IDesignTimeDbContextFactory<PostgreSqlDataImportDbContext>
    {
        public PostgreSqlDataImportDbContext CreateDbContext(string[] args)
        {
            Validate(args);

            var dbType = DatabaseType(args);

            if (dbType.StartsWith("PostgreSql", StringComparison.InvariantCultureIgnoreCase))
            {  
                var optionsBuilder = new DbContextOptionsBuilder<PostgreSqlDataImportDbContext>();
                optionsBuilder.UseNpgsql(args[0]);
                return new PostgreSqlDataImportDbContext(Logger(), optionsBuilder.Options);
            }
            else
            {
                throw new Exception($"Unsupported provider: {dbType}.");
            }
        }
    }

    public class DesignTimeSqlDataImportDbContextFactory : DesignTimeDataImportDbContextFactoryBase, IDesignTimeDbContextFactory<SqlDataImportDbContext>
    {
        public SqlDataImportDbContext CreateDbContext(string[] args)
        {
            Validate(args);

            var dbType = DatabaseType(args);

            if (dbType.StartsWith("SqlServer", StringComparison.InvariantCultureIgnoreCase))
            {
                var optionsBuilder = new DbContextOptionsBuilder<SqlDataImportDbContext>();
                optionsBuilder.UseSqlServer(args[0]);
                return new SqlDataImportDbContext(Logger(), optionsBuilder.Options);               
            }
            else
            {
                throw new Exception($"Unsupported provider: {dbType}.");
            }
        }
    }

    public class DesignTimeDataImportDbContextFactoryBase
    {
        protected static ILogger Logger()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("DataImport", LogLevel.Debug)
                    .AddConsole();
            });

            return loggerFactory.CreateLogger("Database context");
        }

        protected static string DatabaseType(string[] args)
        {
            return args[1];
        }

        protected void Validate(string[] args)
        {
            var dbTypes = new List<string>{ "sqlserver", "postgresql" };

            if (args.Length == 0)
                throw new ArgumentException("A connection string and database type must be provided");

            if (string.IsNullOrWhiteSpace(args[0]))
            {
                throw new ArgumentException("A connection string must be provided");
            }

            try
            {
                new DbConnectionStringBuilder().ConnectionString = args[0];
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    "A valid connection string must be provided as the first argument when using EF database tools with this DbContext.",
                    ex);
            }

            if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]) || !dbTypes.Contains(args[1].ToLower()))
            {
                throw new ArgumentException("A valid database type must be provided. Valid database types: SqlServer, PostgreSql");
            }
        }
    }
}