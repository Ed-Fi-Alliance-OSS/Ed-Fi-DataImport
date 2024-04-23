// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using AutoMapper;
using DataImport.Models;
using DataImport.Web.Features.Shared.SelectListProviders;
using DataImport.Web.Helpers;
using DataImport.Web.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using DataImport.Common.Preprocessors;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace DataImport.Web.Features.Preprocessor
{
    public class AddPreprocessor
    {
        public class Query : IRequest<AddEditPreprocessorViewModel>
        {
        }

        public class QueryHandler : IRequestHandler<Query, AddEditPreprocessorViewModel>
        {
            private readonly ScriptTypeSelectListProvider _scriptTypeSelectListProvider;
            private readonly ExternalPreprocessorOptions _externalPreprocessorSettings;

            public QueryHandler(ScriptTypeSelectListProvider scriptTypeSelectListProvider, IOptions<ExternalPreprocessorOptions> externalPreprocessorSettings)
            {
                _scriptTypeSelectListProvider = scriptTypeSelectListProvider;
                _externalPreprocessorSettings = externalPreprocessorSettings.Value;
            }

            public Task<AddEditPreprocessorViewModel> Handle(Query request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new AddEditPreprocessorViewModel
                {
                    ScriptTypes = _scriptTypeSelectListProvider.GetSelectListItems(),
                    ExternalPreprocessorsEnabled = _externalPreprocessorSettings.Enabled,
                });
            }
        }

        public class Command : IRequest<Response>
        {
            public AddEditPreprocessorViewModel ViewModel { get; set; }
        }

        public class Response : ToastResponse
        {
            public int PreprocessorId { get; set; }
        }

        public class CommandHandler : IRequestHandler<Command, Response>
        {
            private readonly ILogger _logger;
            private readonly DataImportDbContext _database;
            private readonly IMapper _mapper;
            private readonly ExternalPreprocessorOptions _externalPreprocessorSettings;

            public CommandHandler(ILogger<CommandHandler> logger, DataImportDbContext database, IMapper mapper, IOptions<ExternalPreprocessorOptions> externalPreprocessorSettings)
            {
                _logger = logger;
                _database = database;
                _mapper = mapper;
                _externalPreprocessorSettings = externalPreprocessorSettings.Value;
            }

            public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {
                if (request.ViewModel.ScriptType.Value.IsExternal() && !_externalPreprocessorSettings.Enabled)
                    throw new ValidationException("External PreProcessors are disabled. Update application settings to enable them.");

                var script = _mapper.Map<Script>(request.ViewModel);
                _database.Scripts.Add(script);

                await _database.SaveChangesAsync(cancellationToken); // Explicitly call SaveChanges to get Id for the script.

                _logger.Added(script, x => x.Name);

                return new Response
                {
                    PreprocessorId = script.Id,
                    Message = $"Script '{script.Name}' was created."
                };
            }
        }
    }
}
