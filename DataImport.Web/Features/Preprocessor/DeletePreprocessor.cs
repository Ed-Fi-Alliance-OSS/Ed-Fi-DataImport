// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.Web.Helpers;
using DataImport.Web.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.Preprocessor
{
    public class DeletePreprocessor
    {
        public class Command : IRequest<ToastResponse>
        {
            public int Id { get; set; }
        }

        public class CommandHandler : IRequestHandler<Command, ToastResponse>
        {
            private readonly ILogger _logger;
            private readonly DataImportDbContext _database;

            public CommandHandler(ILogger<DeletePreprocessor> logger, DataImportDbContext database)
            {
                _logger = logger;
                _database = database;
            }

            public async Task<ToastResponse> Handle(Command request, CancellationToken cancellationToken)
            {
                // A script cannot be referenced by both Agent and DataMap
                if (await _database.DataMaps.AnyAsync(x => x.FileProcessorScriptId == request.Id, cancellationToken))
                {
                    return new ToastResponse
                    {
                        IsSuccess = false,
                        Message = "Preprocessor cannot be deleted because there is at least one Data Map using it."
                    };
                }

                if (await _database.Agents.AnyAsync(x => x.FileGeneratorScriptId == request.Id || x.RowProcessorScriptId == request.Id, cancellationToken))
                {
                    return new ToastResponse
                    {
                        IsSuccess = false,
                        Message = "Preprocessor cannot be deleted because there is at least one Agent using it."
                    };
                }

                var script = await _database.Scripts.SingleAsync(x => x.Id == request.Id, cancellationToken);

                _database.Scripts.Remove(script);
                _logger.Deleted(script, a => a.Name);

                return new ToastResponse
                {
                    Message = $"Preprocessor '{script.Name}' was deleted."
                };
            }
        }
    }
}
