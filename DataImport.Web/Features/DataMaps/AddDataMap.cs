// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.Web.Features.Shared.SelectListProviders;
using DataImport.Web.Helpers;
using DataImport.Web.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.DataMaps
{
    public class AddDataMap
    {
        public class Query : IRequest<AddEditDataMapViewModel>
        {
            public string[] SourceCsvHeaders { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, AddEditDataMapViewModel>
        {
            private readonly DataImportDbContext _database;
            private readonly PreprocessorSelectListProvider _preprocessorSelectListProvider;

            public QueryHandler(DataImportDbContext database, PreprocessorSelectListProvider preprocessorSelectListProvider)
            {
                _database = database;
                _preprocessorSelectListProvider = preprocessorSelectListProvider;
            }

            public Task<AddEditDataMapViewModel> Handle(Query request, CancellationToken cancellationToken)
            {
                var columnHeaders = request.SourceCsvHeaders;

                return Task.FromResult(new AddEditDataMapViewModel
                {
                    DataMapId = 0,

                    ColumnHeaders = columnHeaders,
                    FieldsViewModel = new DataMapperFieldsViewModel
                    {
                        DataSources = DataMapperFields.MapDataSourcesTypesToViewModel(),
                        SourceTables = DataMapperFields.MapLookupTablesToViewModel(_database),
                        SourceColumns = DataMapperFields.MapCsvHeadersToSourceColumns(columnHeaders),
                        ResourceMetadata = new List<ResourceMetadata>(),
                        Mappings = new List<DataMapper>()
                    },
                    Preprocessors = _preprocessorSelectListProvider.GetCustomFileProcessors()
                });
            }
        }

        public class Command : IRequest<Response>
        {
            [Display(Name = "API Version")]
            public int ApiVersionId { get; set; }
            public string MapName { get; set; }
            [Display(Name = "Map To Resource")]
            public string ResourcePath { get; set; }
            public DataMapper[] Mappings { get; set; }
            public string[] ColumnHeaders { get; set; }
            public int? PreprocessorId { get; set; }
            public string Attribute { get; set; }
            public bool IsDeleteOperation { get; set; }
            public bool IsDeleteByNaturalKey { get; set; }
            public string SelectedIngestionLogEdOrgIdColumn { get; set; }
        }

        public class Response : ToastResponse
        {
            public int DataMapId { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            private readonly DataImportDbContext _dbContext;

            public Validator(DataImportDbContext dbContext)
            {
                _dbContext = dbContext;

                RuleFor(x => x.MapName)
                    .NotEmpty()
                    .Must(BeAUniqueName).WithMessage(model => $"A Data Map named '{model.MapName}' already exists. Data Maps must have unique names.");

                RuleFor(x => x.ResourcePath).NotEmpty().WithName("Map To Resource");
                RuleFor(x => x.ApiVersionId).NotEmpty().WithName("API Version");
                RuleFor(x => x.Attribute).Must(BePopulatedIfRequiredByPreprocessor).WithMessage("Preprocessor '{PreprocessorName}' requires a map attribute.");
            }

            private bool BePopulatedIfRequiredByPreprocessor(Command command, string attribute, ValidationContext<Command> context)
            {
                return ValidatorHelper.BePopulatedIfRequiredByPreprocessor(command.PreprocessorId, attribute, context, _dbContext);
            }

            private bool BeAUniqueName(string candidateName) =>
                _dbContext.DataMaps.FirstOrDefault(map => map.Name == candidateName) == null;
        }

        public class CommandHandler : IRequestHandler<Command, Response>
        {
            private readonly ILogger _logger;
            private readonly DataImportDbContext _database;

            public CommandHandler(ILogger<AddDataMap> logger, DataImportDbContext database)
            {
                _logger = logger;
                _database = database;
            }

            public Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {
                var resource = _database.Resources.Single(x => x.Path == request.ResourcePath && x.ApiVersionId == request.ApiVersionId);

                var dataMap = new DataMap
                {
                    Name = request.MapName,
                    ResourcePath = resource.Path,
                    Map = request.IsDeleteOperation
                    ? request.IsDeleteByNaturalKey
                        ? new DataMapSerializer(resource).Serialize(request.Mappings)
                        : new DeleteDataMapSerializer(resource).Serialize(request.Mappings)
                    : new DataMapSerializer(resource).Serialize(request.Mappings),
                    Metadata = resource.Metadata,
                    CreateDate = DateTimeOffset.Now,
                    UpdateDate = DateTimeOffset.Now,
                    ColumnHeaders =
                        request.ColumnHeaders == null
                            ? null
                            : JsonConvert.SerializeObject(request.ColumnHeaders),
                    ApiVersionId = request.ApiVersionId,
                    FileProcessorScriptId = request.PreprocessorId,
                    Attribute = request.Attribute,
                    IsDeleteOperation = request.IsDeleteOperation,
                    IsDeleteByNaturalKey = request.IsDeleteOperation && request.IsDeleteByNaturalKey,
                    SelectedIngestionLogEdOrgIdColumn = request.SelectedIngestionLogEdOrgIdColumn,
                };

                _database.DataMaps.Add(dataMap);
                _database.SaveChanges();

                _logger.Added(dataMap, d => d.Name);

                return Task.FromResult(new Response
                {
                    DataMapId = dataMap.Id,
                    Message = $"Data Map '{dataMap.Name}' was created."
                });
            }
        }
    }
}
