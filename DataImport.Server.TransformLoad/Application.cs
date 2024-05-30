// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Helpers;
using DataImport.Models;
using DataImport.Server.TransformLoad.Features.Events;
using DataImport.Server.TransformLoad.Features.FileGeneration;
using DataImport.Server.TransformLoad.Features.FileTransport;
using DataImport.Server.TransformLoad.Features.LoadResources;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static DataImport.Common.Encryption;

namespace DataImport.Server.TransformLoad
{
    public interface IApplication
    {
        Task Run();
    }

    public class Application : IApplication, IHostedService
    {
        private readonly ILogger<Application> _logger;
        private readonly IOptions<AppSettings> _options;
        private readonly DataImportDbContext _dbContext;
        private readonly IMediator _mediator;
        private readonly string _encryptionKey;

        public Application(ILogger<Application> logger, IOptions<AppSettings> options, DataImportDbContext dbContext, IEncryptionKeySettings encryptionKeySettings, IMediator mediator)
        {
            _logger = logger;
            _options = options;
            _dbContext = dbContext;
            _mediator = mediator;
            _encryptionKey = encryptionKeySettings.EncryptionKey;
        }

        public async Task Run()
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                var watch = Stopwatch.StartNew();

                await _mediator.Send(new JobStarted.Command());

                foreach (var odsConfig in GetApiConfigs())
                {
                    await _mediator.Send(new FileTransporter.Command { ApiServerId = odsConfig.ApiServerId });
                    await _mediator.Send(new FileGenerator.Command { ApiServerId = odsConfig.ApiServerId });

                    var odsApi = new OdsApi(_logger, odsConfig, _options);

                    var bootstrapResponse = await _mediator.Send(new PostBootstrapData.Command { OdsApi = odsApi });

                    if (bootstrapResponse.Success)
                        await _mediator.Send(new FileProcessor.Command { OdsApi = odsApi });
                    // Remove files from the temp folder if FileAction is Always Delete
                    await _mediator.Send(new FileTransporter.Command { ApiServerId = odsConfig.ApiServerId, CleanUpProcess = true });
                }

                watch.Stop();

                _logger.LogInformation("Time Elapsed: {time}", watch.Elapsed);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Process Exception: " + ex);

                _logger.LogError(ex, "Unexpected error in TransformLoad service");
            }
            finally
            {
                await _mediator.Send(new JobCompleted.Command());
            }
        }

        private IList<ApiConfig> GetApiConfigs()
        {
            var apiServers = _dbContext.ApiServers.Include(x => x.ApiVersion).OrderBy(x => x.Id).ToList();

            if (apiServers.Count == 0)
                throw new Exception("No api server configured");

            return apiServers.Select(x =>

                new ApiConfig
                {
                    ApiServerId = x.Id,
                    ApiUrl = x.Url,
                    AuthorizeUrl = x.AuthUrl,
                    AccessTokenUrl = x.TokenUrl,
                    ClientId = Decrypt(x.Key, _encryptionKey),
                    ClientSecret = Decrypt(x.Secret, _encryptionKey),
                    ApiVersion = x.ApiVersion.Version,
                    Name = x.Name
                }).ToList();
        }


        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.LogInformation("An unhandled exception has occurred: {ex}", e.ExceptionObject);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Run();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
