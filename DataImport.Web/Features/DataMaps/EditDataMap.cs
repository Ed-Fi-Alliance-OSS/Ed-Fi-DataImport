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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.DataMaps
{
    public class EditDataMap
    {
        public class Query : IRequest<AddEditDataMapViewModel>
        {
            public int Id { get; set; }
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
                var mapId = request.Id;

                var columnHeaders = request.SourceCsvHeaders;

                var dataMap = _database.DataMaps.Include(x => x.ApiVersion).Single(x => x.Id == mapId);

                if (columnHeaders.IsNullOrEmpty() && !dataMap.ColumnHeaders.IsNullOrEmpty())
                    columnHeaders = JsonConvert.DeserializeObject<string[]>(dataMap.ColumnHeaders);

                var resourceMetadata = ResourceMetadata.DeserializeFrom(dataMap);

                return Task.FromResult(new AddEditDataMapViewModel
                {
                    DataMapId = mapId,
                    ColumnHeaders = columnHeaders,
                    FieldsViewModel = new DataMapperFieldsViewModel
                    {
                        DataSources = DataMapperFields.MapDataSourcesTypesToViewModel(),
                        SourceTables = DataMapperFields.MapLookupTablesToViewModel(_database),
                        SourceColumns = DataMapperFields.MapCsvHeadersToSourceColumns(columnHeaders),
                        ResourceMetadata = dataMap.IsDeleteByNaturalKey
                            ? resourceMetadata.Where(r => r.Required).ToList()
                            : resourceMetadata,
                        Mappings = dataMap.IsDeleteOperation
                            ? new DeleteDataMapSerializer(dataMap).Deserialize(dataMap.Map)
                            : new DataMapSerializer(dataMap).Deserialize(dataMap.Map),
                    },

                    MapName = dataMap.Name,
                    ResourcePath = dataMap.ResourcePath,
                    ResourceName = dataMap.ToResourceName(),
                    MetadataIsIncompatible = !dataMap.IsDeleteOperation && dataMap.MetadataIsIncompatible(_database),
                    ApiVersion = dataMap.ApiVersion.Version,
                    ApiVersionId = dataMap.ApiVersionId,
                    PreprocessorId = dataMap.FileProcessorScriptId,
                    Preprocessors = _preprocessorSelectListProvider.GetCustomFileProcessors(),
                    Attribute = dataMap.Attribute,
                    IsDeleteOperation = dataMap.IsDeleteOperation,
                    IsDeleteByNaturalKey = dataMap.IsDeleteByNaturalKey
                });
            }
        }

        public class Command : IRequest<ToastResponse>
        {
            public int DataMapId { get; set; }
            public string MapName { get; set; }
            public string ResourcePath { get; set; }
            public DataMapper[] Mappings { get; set; }
            public string[] ColumnHeaders { get; set; }
            public int? PreprocessorId { get; set; }
            public string Attribute { get; set; }
            public bool IsDeleteOperation { get; set; }
            public bool IsDeleteByNaturalKey { get; set; }
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

                RuleFor(x => x.Attribute).Must(BePopulatedIfRequiredByPreprocessor).WithMessage("Preprocessor '{PreprocessorName}' requires a map attribute.");
            }

            private bool BePopulatedIfRequiredByPreprocessor(Command command, string attribute, ValidationContext<Command> context)
            {
                return ValidatorHelper.BePopulatedIfRequiredByPreprocessor(command.PreprocessorId, attribute, context, _dbContext);
            }

            private bool BeAUniqueName(Command command, string candidateName) =>
                EditingWithoutChangingAgentName(command, candidateName) || NewNameDoesNotAlreadyExist(command, candidateName);

            private bool EditingWithoutChangingAgentName(Command command, string candidateName) =>
                _dbContext.DataMaps.FirstOrDefault(dataMap => dataMap.Id == command.DataMapId)?.Name == candidateName;

            private bool NewNameDoesNotAlreadyExist(Command command, string candidateName) =>
                _dbContext.DataMaps.FirstOrDefault(dataMap => dataMap.Name == candidateName && dataMap.Id != command.DataMapId) == null;
        }

        public class CommandHandler : IRequestHandler<Command, ToastResponse>
        {
            private readonly ILogger _logger;
            private readonly DataImportDbContext _dataImportDbContext;

            public CommandHandler(ILogger<EditDataMap> logger, DataImportDbContext dataImportDbContext)
            {
                _logger = logger;
                _dataImportDbContext = dataImportDbContext;
            }

            public Task<ToastResponse> Handle(Command request, CancellationToken cancellationToken)
            {
                var map = _dataImportDbContext.DataMaps.FirstOrDefault(x => x.Id == request.DataMapId) ?? new DataMap();

                map.Name = request.MapName;
                map.Map = request.IsDeleteOperation
                ? request.IsDeleteByNaturalKey
                    ? new DataMapSerializer(map).Serialize(request.Mappings)
                    : new DeleteDataMapSerializer(map).Serialize(request.Mappings)
                : new DataMapSerializer(map).Serialize(request.Mappings);
                map.ColumnHeaders = JsonConvert.SerializeObject(request.ColumnHeaders);
                map.UpdateDate = DateTimeOffset.Now;
                map.FileProcessorScriptId = request.PreprocessorId;
                map.Attribute = request.Attribute;
                map.IsDeleteOperation = request.IsDeleteOperation;
                map.IsDeleteByNaturalKey = request.IsDeleteOperation && request.IsDeleteByNaturalKey;
                _logger.Modified(map, m => m.Name);

                return Task.FromResult(new ToastResponse
                {
                    Message = $"Data Map '{map.Name}' was modified."
                });
            }
        }
    }
}
