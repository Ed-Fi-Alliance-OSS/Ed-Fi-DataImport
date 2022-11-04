// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DataImport.Models
{
    public class SqlDataImportDbContext : DataImportDbContext
    {
        public SqlDataImportDbContext(ILogger logger, DbContextOptions<SqlDataImportDbContext> dbOptions, IOptions<ConnectionStrings> options = null)
            : base(logger, dbOptions, options)
        {
            DatabaseVersionSql = "SELECT @@VERSION as VersionString";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                if (ConnectionString == default)
                    throw new ConfigurationErrorsException(
                        $"{nameof(SqlDataImportDbContext)} was not configured and a default connection string was not provided via {nameof(IOptions<ConnectionStrings>)}.");

                options.UseSqlServer(ConnectionString);
            }
        }
    }
}
