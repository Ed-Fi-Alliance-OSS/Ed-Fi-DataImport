// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace DataImport.Models
{
    public class PostgreSqlDataImportDbContext : DataImportDbContext
    {
        public PostgreSqlDataImportDbContext(ILogger logger, DbContextOptions<PostgreSqlDataImportDbContext> dbOptions, IOptions<ConnectionStrings> options = null)
            : base(logger, dbOptions, options)
        {
            DatabaseVersionSql = "SELECT version() as VersionString";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                if (ConnectionString == default)
                    throw new ConfigurationErrorsException(
                        $"{nameof(PostgreSqlDataImportDbContext)} was not configured and a default connection string was not provided via {nameof(IOptions<ConnectionStrings>)}.");

                options.UseNpgsql(ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var dateTimeOffSetConverter = new ValueConverter<DateTimeOffset, DateTimeOffset>(
                v => v.ToUniversalTime(), v => v.ToLocalTime());

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTimeOffset) || property.ClrType == typeof(DateTimeOffset?))
                        property.SetValueConverter(dateTimeOffSetConverter);
                }
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}
