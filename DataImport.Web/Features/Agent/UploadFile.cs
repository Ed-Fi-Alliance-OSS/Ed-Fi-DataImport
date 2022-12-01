// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common;
using DataImport.Models;
using DataImport.Web.Helpers;
using DataImport.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.Agent
{
    public class UploadFile
    {
        public class Query : IRequest<Command>
        {
            public int AgentId { get; set; }
        }

        public class QueryHandler : RequestHandler<Query, Command>
        {
            protected override Command Handle(Query request)
            {
                return new Command
                {
                    AgentId = request.AgentId
                };
            }
        }

        public class Command : IRequest<ToastResponse>
        {
            [Accept(".csv")]
            public IFormFile File { get; set; }
            public int AgentId { get; set; }
        }

        public class CommandHandler : IRequestHandler<Command, ToastResponse>
        {
            private readonly ILogger _logger;
            private readonly DataImportDbContext _dataImportDbContext;
            private readonly IFileService _fileService;

            public CommandHandler(ILogger<UploadFile> logger, IOptions<AppSettings> options, DataImportDbContext dataImportDbContext, ResolveFileService fileServices)
            {
                _logger = logger;
                _dataImportDbContext = dataImportDbContext;
                _fileService = fileServices(options.Value.FileMode);
            }

            public async Task<ToastResponse> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    if (request.File.Length <= 0)
                    {
                        _logger.LogError("Cannot upload an empty file");
                        return null;
                    }

                    var agent = _dataImportDbContext.Agents.Single(x => x.Id == request.AgentId);

                    await _fileService.Upload(request.File.FileName, request.File.OpenReadStream(), agent);

                    return new ToastResponse
                    {
                        Message = $"File was uploaded to Agent '{agent.Name}'."
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error Manually Uploading File to Agent");

                    return null;
                }
            }
        }
    }
}
