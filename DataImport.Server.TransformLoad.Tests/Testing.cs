// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common;
using DataImport.Common.Enums;
using DataImport.Models;
using DataImport.Server.TransformLoad.Tests.Features.FileTransport;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using File = DataImport.Models.File;

namespace DataImport.Server.TransformLoad.Tests
{
    public static class Testing
    {
        public static readonly IServiceProvider Services;

        static Testing()
        {
            ConfigureEnvironmentShareNameBeforeCreatingDefaultBuilder();

            var host = Host.CreateDefaultBuilder()
                           .ConfigureServices(
                               (context, services) => services.ConfigureTransformLoadServices(context.Configuration))
                           .ConfigureServices(ConfigureTestSpecificIocRules)
                           .Build().ConfigureStaticGlobals();

            Services = host.Services;

            static void ConfigureEnvironmentShareNameBeforeCreatingDefaultBuilder()
            {
                var shareName = Path.Combine(
                    Path.GetDirectoryName(
                        Assembly.GetExecutingAssembly()
                                .Location) ?? Path.GetTempPath(),
                    "TestShareFolder");

                Environment.SetEnvironmentVariable("AppSettings__ShareName", shareName, EnvironmentVariableTarget.Process);
            }
        }

        private static void ConfigureTestSpecificIocRules(IServiceCollection services)
        {
            services.AddTransient<TestFtpsServer>();
            services.AddTransient<TestSftpServer>();
            services.AddTransient<ResolveFileServer>(serviceProvider => key =>
            {
                return key switch
                {
                    AgentTypeCodeEnum.Ftps => serviceProvider.GetRequiredService<TestFtpsServer>(),
                    AgentTypeCodeEnum.Sftp => serviceProvider.GetRequiredService<TestSftpServer>(),
                    _ => throw new KeyNotFoundException(),
                };
            });

            services.AddTransient<IFileService, TestLocalFileService>();
            services.AddTransient<TestLocalFileService>();
            services.AddTransient<ResolveFileService>(serviceProvider => key =>
            {
                return key switch
                {
                    FileModeEnum.Local => serviceProvider.GetRequiredService<TestLocalFileService>(),
                    _ => throw new KeyNotFoundException(),
                };
            });

            services.AddDbContext<DataImportDbContext, SqlDataImportDbContext>((s, options) =>
                   options.UseSqlServer(
                           s.GetRequiredService<IOptions<ConnectionStrings>>()
                               .Value.DefaultConnection));
        }

        public static void Transaction(Action<DataImportDbContext> action)
        {
            using (var scope = Services.CreateScope())
            {
                using var database = scope.ServiceProvider.GetRequiredService<DataImportDbContext>();
                try
                {
                    database.BeginTransaction();
                    action(database);
                    database.CloseTransaction();
                }
                catch (Exception exception)
                {
                    database.CloseTransaction(exception);
                    throw;
                }
            }
        }

        public static TResult Query<TResult>(Func<DataImportDbContext, TResult> query)
        {
            var result = default(TResult);

            Transaction(database =>
            {
                result = query(database);
            });

            return result;
        }

        public static async Task Send(IRequest message)
        {
            using (var scope = Services.CreateScope())
            {
                await scope.ServiceProvider.GetRequiredService<IMediator>().Send(message);
            }
        }

        public static async Task<TResponse> Send<TResponse>(IRequest<TResponse> message)
        {
            TResponse response;

            using (var scope = Services.CreateScope())
            {
                response = await scope.ServiceProvider.GetRequiredService<IMediator>().Send(message);
            }

            return response;
        }

        public static string SampleString(string name = null)
        {
            return name == null
                ? Guid.NewGuid().ToString()
                : $"{name}-{Guid.NewGuid()}";
        }

        public static ApiServer GetDefaultApiServer()
        {
            return Query(d => d.ApiServers.OrderBy(y => y.Id).First());
        }

        public static int AddAgent(Agent agent)
        {
            return Add<Agent>(agent);
        }

        public static int AddAgentFile(File file)
        {
            return Add(file);
        }

        public static int Add<T>(T entity) where T : Entity
        {
            using (var scope = Services.CreateScope())
            {
                using (var dbContext = scope.ServiceProvider.GetRequiredService<DataImportDbContext>())
                {
                    dbContext.Set<T>().Add(entity);
                    dbContext.SaveChanges();
                    return entity.Id;
                }
            }
        }

        public static IEnumerable<File> GetLoggedAgentFiles(int agentId)
        {
            List<File> files;
            using (var scope = Services.CreateScope())
            {
                using (var dbContext = scope.ServiceProvider.GetRequiredService<DataImportDbContext>())
                {
                    files = dbContext.Files.Where(f => f.AgentId == agentId).ToList();
                }
            }
            return files;
        }

        public static ResolveFileService GetRegisteredFileServices()
        {
            return Services.GetService<ResolveFileService>();
        }
    }
}
