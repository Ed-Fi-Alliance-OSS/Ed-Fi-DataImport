// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using AutoMapper;
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
    public class EditPreprocessor
    {
        public class Query : IRequest<AddEditPreprocessorViewModel>
        {
            public int Id { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, AddEditPreprocessorViewModel>
        {
            private readonly DataImportDbContext _database;
            private readonly IMapper _mapper;

            public QueryHandler(DataImportDbContext database, IMapper mapper)
            {
                _database = database;
                _mapper = mapper;
            }

            public async Task<AddEditPreprocessorViewModel> Handle(Query request, CancellationToken cancellationToken)
            {
                var script = await _database.Scripts.SingleAsync(x => x.Id == request.Id, cancellationToken);

                return _mapper.Map<AddEditPreprocessorViewModel>(script);
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
            private readonly ILogger<EditPreprocessor> _logger;
            private readonly DataImportDbContext _database;
            private readonly IMapper _mapper;

            public CommandHandler(ILogger<EditPreprocessor> logger, DataImportDbContext database, IMapper mapper)
            {
                _logger = logger;
                _database = database;
                _mapper = mapper;
            }

            public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {
                var script = await _database.Scripts.SingleAsync(x => x.Id == request.ViewModel.Id, cancellationToken);
                _mapper.Map(request.ViewModel, script);

                _logger.Modified(script, x => x.Name);

                return new Response
                {
                    PreprocessorId = script.Id,
                    Message = $"Preprocessor '{script.Name}' was modified."
                };
            }

        }
    }
}
