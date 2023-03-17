// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Data;
using System.Linq;
using DataImport.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace DataImport.Models
{
    public class DataImportDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ILogger _logger;
        private IDbContextTransaction _currentTransaction;
        protected readonly string ConnectionString;

        protected string DatabaseVersionSql { get; set; }

        public DataImportDbContext(ILogger logger, DbContextOptions<DataImportDbContext> dbOptions)
        : base(dbOptions)
        {
            _logger = logger;
        }

        protected DataImportDbContext(ILogger logger, DbContextOptions dbOptions, IOptions<ConnectionStrings> options = null)
            : base(dbOptions)
        {
            _logger = logger;
            ConnectionString = options?.Value?.DefaultConnection;
        }

        public override void Dispose() => base.Dispose();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Resource>()
                .HasIndex(m => new { m.Path, m.ApiVersionId })
                .IsUnique();

            modelBuilder.Entity<IngestionLog>()
                .HasIndex(m => m.Result);

            modelBuilder.Entity<ApplicationLog>()
                .HasIndex(m => m.Logged);

            modelBuilder.Entity<IngestionLog>()
                .HasIndex(m => m.Date);

            modelBuilder.Entity<File>()
                .HasIndex(m => m.CreateDate);

            modelBuilder.Entity<ApiServer>()
                .HasIndex(m => m.Name)
                .IsUnique();

            modelBuilder.Entity<ApiVersion>()
                .HasIndex(m => m.Version)
                .IsUnique();

            modelBuilder.Entity<Lookup>()
                .HasIndex(m => new
                {
                    m.SourceTable,
                    m.Key
                })
                .IsUnique();

            modelBuilder.Entity<BootstrapDataAgent>()
                .HasKey(bc => new { bc.BootstrapDataId, bc.AgentId });

            modelBuilder.Entity<BootstrapDataAgent>()
                .HasOne(bc => bc.Agent)
                .WithMany(b => b.BootstrapDataAgents)
                .HasForeignKey(bc => bc.AgentId);

            modelBuilder.Entity<BootstrapDataAgent>()
                .HasOne(bc => bc.BootstrapData)
                .WithMany(c => c.BootstrapDataAgents)
                .HasForeignKey(bc => bc.BootstrapDataId);

            modelBuilder.Entity<BootstrapDataApiServer>()
                .HasKey(bc => new { bc.BootstrapDataId, bc.ApiServerId });

            modelBuilder.Entity<BootstrapDataApiServer>()
                .HasOne(bc => bc.ApiServer)
                .WithMany(b => b.BootstrapDataApiServers)
                .HasForeignKey(bc => bc.ApiServerId);

            modelBuilder.Entity<BootstrapDataApiServer>()
                .HasOne(bc => bc.BootstrapData)
                .WithMany(c => c.BootstrapDataApiServers)
                .HasForeignKey(bc => bc.BootstrapDataId);

            modelBuilder.Entity<Script>()
                .Property(x => x.ScriptType)
                .HasConversion(new EnumToStringConverter<ScriptType>());

            modelBuilder.Entity<Script>()
                .HasIndex(x => new { x.Name, x.ScriptType }).IsUnique();

            modelBuilder.Entity<AdminView>().ToView(nameof(Models.AdminView))
                .HasNoKey();

            modelBuilder.Entity<DatabaseVersion>().ToSqlQuery(DatabaseVersionSql)
                .HasNoKey();

            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
                fk.DeleteBehavior = DeleteBehavior.Restrict;

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Agent> Agents { get; set; }
        public DbSet<AgentSchedule> AgentSchedules { get; set; }

        public DbSet<BootstrapData> BootstrapDatas { get; set; }
        public DbSet<Configuration> Configurations { get; set; }
        public DbSet<DataMap> DataMaps { get; set; }
        public DbSet<DataMapAgent> DataMapAgents { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<IngestionLog> IngestionLogs { get; set; }
        public DbSet<ApplicationLog> ApplicationLogs { get; set; }
        public DbSet<Lookup> Lookups { get; set; }
        public DbSet<ApiServer> ApiServers { get; set; }
        public DbSet<JobStatus> JobStatus { get; set; }
        public DbSet<ApiVersion> ApiVersions { get; set; }
        public DbSet<AdminView> AdminView { get; set; }
        public DbSet<BootstrapDataAgent> BootstrapDataAgents { get; set; }
        public DbSet<BootstrapDataApiServer> BootstrapDataApiServers { get; set; }
        public DbSet<Script> Scripts { get; set; }
        public DbSet<DatabaseVersion> DatabaseVersion { get; set; }

        public TEntity EnsureSingle<TEntity>() where TEntity : Entity, new()
        {
            var single = Set<TEntity>().SingleOrDefault();

            if (single == null)
            {
                single = new TEntity();
                Set<TEntity>().Add(single);
            }

            return single;
        }

        public void BeginTransaction()
        {
            if (_currentTransaction != null)
                return;

            _currentTransaction = Database.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public void CloseTransaction()
        {
            CloseTransaction(exception: null);
        }

        public void CloseTransaction(Exception exception)
        {
            try
            {
                if (_currentTransaction != null && exception != null)
                {
                    _currentTransaction.Rollback();
                    return;
                }

                SaveChanges();

                _currentTransaction?.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown while attempting to close a transaction.");

                if (_currentTransaction?.GetDbTransaction().Connection != null)
                {
                    _currentTransaction.Rollback();
                }

                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }
    }
}
