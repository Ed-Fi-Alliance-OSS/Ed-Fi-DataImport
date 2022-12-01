// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Preprocessors;
using DataImport.Models;
using DataImport.Web.Features.BootstrapData;
using DataImport.Web.Features.DataMaps;
using DataImport.Web.Features.Lookup;
using DataImport.Web.Features.Preprocessor;
using DataImport.Web.Helpers;
using DataImport.Web.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.Share
{
    public abstract class ImportBase
    {
        public class Command : IRequest<Response>
        {
            public SharingModel Import { get; set; }
            public SharingContact Submitter { get; set; }
            public bool OverwritePreprocessors { get; set; }
        }

        public class Response : ToastResponse
        {
            public Response()
            {
                BootstrapIds = new List<int>();
                DataMapIds = new List<int>();
            }

            public List<int> BootstrapIds { get; set; }
            public List<int> DataMapIds { get; set; }
        }

        public class SharingModelValidator : AbstractValidator<SharingModel>
        {
            public SharingModelValidator(ILogger logger, DataImportDbContext database, IPowerShellPreprocessorService powerShellPreprocessorService,
                IJsonValidator jsonValidator, bool overwritePreprocessors = false)
            {
                const string missing = "This template is missing its expected {0}.";
                RuleFor(x => x.Title).NotNull().WithMessage(string.Format(missing, "title"));
                RuleFor(x => x.Description).NotNull().WithMessage(string.Format(missing, "description"));
                RuleFor(x => x.ApiVersion).NotNull().WithMessage(string.Format(missing, "API version"));

                RuleForEach(x => x.Template.Bootstraps)
                    .SafeCustom(logger, (bootstrap, context) =>
                    {
                        var apiVersion = database.ApiVersions.AsNoTracking().SingleOrDefault(x => x.Version == ((SharingModel) context.InstanceToValidate).ApiVersion);
                        if (apiVersion == null)
                        {
                            context.AddFailure($"Could not resolve a bootstrap resource for unrecognized API Version' {((SharingModel) context.InstanceToValidate).ApiVersion}'.");
                            return;
                        }

                        var resource = database.Resources.SingleOrDefault(x => x.Path == bootstrap.ResourcePath && x.ApiVersionId == apiVersion.Id);

                        if (resource == null)
                        {
                            context.AddFailure($"This template contains a bootstrap for unrecognized resource '{bootstrap.ResourcePath}'.");
                            return;
                        }

                        var validator = new AddBootstrapData.Validator(logger, database, jsonValidator);
                        var result = validator.Validate(bootstrap.ToAddCommand(apiVersion.Id));

                        if (!result.IsValid)
                        {
                            var errorMessages = string.Join(" ", result.Errors.Select(x => x.ErrorMessage));
                            context.AddFailure($"This template contains an invalid bootstrap '{bootstrap.Name}': {errorMessages}");
                        }
                    });

                RuleForEach(x => x.Template.Preprocessors)
                    .SafeCustom(logger, (preprocessor, context) =>
                    {
                        var existingPreprocessor = database.Scripts.Include(x => x.DataMaps).AsNoTracking().SingleOrDefault(x => x.Name == preprocessor.Name && (x.ScriptType == ScriptType.CustomFileProcessor || x.ScriptType == ScriptType.ExternalFileProcessor));
                        var validator = new Validator(database, powerShellPreprocessorService);
                        AddEditPreprocessorViewModel addEditPreprocessorViewModel;
                        if (existingPreprocessor == null)
                        {
                            addEditPreprocessorViewModel = preprocessor.ToCustomFileProcessorAddCommand().ViewModel;
                        }
                        else
                        {
                            if (preprocessor.HasConflict(existingPreprocessor))
                            {
                                if (!overwritePreprocessors)
                                {
                                    context.AddFailure($"The existing preprocessor '{existingPreprocessor.Name}' differs from the one to be imported.");
                                    return;
                                }
                            }

                            // Make sure updating the preprocessor does not break existing maps
                            if (preprocessor.HasAttribute != existingPreprocessor.HasAttribute && preprocessor.HasAttribute)
                            {
                                var dataMapsWithoutAttribute = existingPreprocessor.DataMaps.Where(x => string.IsNullOrEmpty(x.Attribute)).ToList();
                                if (dataMapsWithoutAttribute.Count > 0)
                                {
                                    context.AddFailure($"Updating the exiting preprocessor '{preprocessor.Name}' will break the following data maps due to a required Attribute field: {string.Join(", ", dataMapsWithoutAttribute.Select(x => x.Name))}.");
                                    return;
                                }
                            }

                            addEditPreprocessorViewModel = preprocessor.ToCustomFileProcessorEditCommand(existingPreprocessor.Id).ViewModel;
                        }

                        var result = validator.Validate(addEditPreprocessorViewModel);
                        if (!result.IsValid)
                        {
                            var errorMessages = string.Join(" ", result.Errors.Select(x => x.ErrorMessage));
                            context.AddFailure($"This template contains an invalid preprocessor '{preprocessor.Name}': {errorMessages}");
                        }
                    });

                RuleForEach(x => x.Template.Maps)
                    .SafeCustom(logger, (map, context) =>
                    {
                        var apiVersion = database.ApiVersions.AsNoTracking().SingleOrDefault(x => x.Version == ((SharingModel) context.InstanceToValidate).ApiVersion);
                        if (apiVersion == null)
                        {
                            context.AddFailure($"This template contains a map for unrecognized API Version' {((SharingModel) context.InstanceToValidate).ApiVersion}'.");
                            return;
                        }

                        var resource = database.Resources.SingleOrDefault(x => x.Path == map.ResourcePath && x.ApiVersionId == apiVersion.Id);
                        if (resource == null)
                        {
                            context.AddFailure($"This template contains a map for unrecognized resource '{map.ResourcePath}'.");
                            return;
                        }

                        string exceptionMessage;
                        if (!map.Map.IsCompatibleWithResource(resource, MetadataCompatibilityLevel.DataMap, out exceptionMessage))
                        {
                            context.AddFailure(
                                $"This template contains a map '{map.Name}' which is not " +
                                $"compatible with your definition of resource '{map.ResourcePath}'. {exceptionMessage}");
                            return;
                        }

                        var validator = new AddDataMap.Validator(database);

                        List<string> errorMessages = new List<string>();
                        if (!string.IsNullOrEmpty(map.CustomFileProcessor) && string.IsNullOrEmpty(map.Attribute))
                        {
                            var sharingModel = (SharingModel) context.InstanceToValidate;
                            var preprocessor = sharingModel.Template.Preprocessors.Single(x => x.Name == map.CustomFileProcessor);
                            if (preprocessor.HasAttribute)
                            {
                                errorMessages.Add($"Preprocessor '{preprocessor.Name}' requires a map attribute.");
                            }
                        }

                        // validate data maps without preprocessors since they might not be created yet
                        var result = validator.Validate(map.ToAddCommand(resource, null));
                        if (!result.IsValid || errorMessages.Count > 0)
                        {
                            errorMessages.AddRange(result.Errors.Select(x => x.ErrorMessage));
                            var flattenErrors = string.Join(" ", errorMessages);
                            context.AddFailure($"This template contains an invalid map '{map.Name}': {flattenErrors}");
                        }
                    });

                RuleForEach(x => x.Template.Lookups)
                    .SafeCustom(logger, (lookup, context) =>
                    {
                        var hasSourceTableConflict = database.Lookups.Any(x => x.SourceTable == lookup.SourceTable.Trim());

                        if (hasSourceTableConflict)
                        {
                            context.AddFailure($"This template contains a lookup '{lookup.SourceTable}' which conflicts with your existing '{lookup.SourceTable}' lookup.");
                            return;
                        }

                        var validator = new AddLookup.Validator(database);
                        var result = validator.Validate(lookup.ToAddCommand());

                        if (!result.IsValid)
                        {
                            var errorMessages = string.Join(" ", result.Errors.Select(x => x.ErrorMessage));
                            context.AddFailure($"This template contains an invalid lookup '{lookup.SourceTable}': {errorMessages}");
                        }
                    });
            }
        }

        public class CommandHandler : IRequestHandler<Command, Response>
        {
            private readonly IMediator _mediator;

            private readonly DataImportDbContext _database;

            public CommandHandler(IMediator mediator, DataImportDbContext database)
            {
                _mediator = mediator;
                _database = database;
            }

            public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {
                var response = new Response();

                var import = request.Import;

                var resources = ReferencedResources(import);

                var preprocessors = new Dictionary<string, int>();

                var apiVersion = _database.ApiVersions.SingleOrDefault(x => x.Version == request.Import.ApiVersion);
                if (apiVersion == null)
                {
                    apiVersion = new ApiVersion
                    {
                        Version = request.Import.ApiVersion
                    };
                    _database.ApiVersions.Add(apiVersion);
                    await _database.SaveChangesAsync(cancellationToken); // Save explicitly to get ApiServerId
                }

                if (import.Template.Bootstraps != null)
                {
                    foreach (var bootstrap in import.Template.Bootstraps)
                    {
                        var addBootstrapResponse = await _mediator.Send(bootstrap.ToAddCommand(apiVersion.Id));

                        response.BootstrapIds.Add(addBootstrapResponse.BootstrapDataId);
                    }
                }

                if (import.Template.Lookups != null)
                {
                    foreach (var lookup in import.Template.Lookups)
                    {
                        await _mediator.Send(lookup.ToAddCommand());
                    }
                }

                if (import.Template.Preprocessors != null)
                {
                    foreach (var preprocessor in import.Template.Preprocessors)
                    {
                        int preprocessorId;
                        var existingPreprocessor = await _database.Scripts.SingleOrDefaultAsync(x => x.Name == preprocessor.Name && (x.ScriptType == ScriptType.CustomFileProcessor || x.ScriptType == ScriptType.ExternalFileProcessor), cancellationToken);
                        if (existingPreprocessor != null)
                        {
                            if (existingPreprocessor.ScriptContent != preprocessor.ScriptContent && !request.OverwritePreprocessors)
                            {
                                throw new InvalidOperationException($"The existing content for preprocessor '{existingPreprocessor.Name}' differs from the one to be imported. If you want to import it, set {nameof(request.OverwritePreprocessors)} to True for the command.");
                            }

                            var editPreprocessorResponse = await _mediator.Send(preprocessor.ToCustomFileProcessorEditCommand(existingPreprocessor.Id), cancellationToken);
                            preprocessorId = editPreprocessorResponse.PreprocessorId;
                        }
                        else
                        {
                            var addPreprocessorResponse = await _mediator.Send(preprocessor.ToCustomFileProcessorAddCommand(), cancellationToken);
                            preprocessorId = addPreprocessorResponse.PreprocessorId;
                        }

                        preprocessors.Add(preprocessor.Name, preprocessorId);
                    }
                }

                if (import.Template.Maps != null)
                {
                    foreach (var map in import.Template.Maps)
                    {
                        var addMapResponse = await _mediator.Send(map.ToAddCommand(resources[map.ResourcePath], string.IsNullOrEmpty(map.CustomFileProcessor) ? (int?) null : preprocessors[map.CustomFileProcessor]));

                        response.DataMapIds.Add(addMapResponse.DataMapId);
                    }
                }

                response.Message = $"Template '{request.Import.Title}' was imported.";

                return response;
            }

            private Dictionary<string, Resource> ReferencedResources(SharingModel import)
            {
                var allResourcePaths = new List<string>();

                if (import.Template.Bootstraps != null)
                {
                    var bootstrapResourcePaths = import.Template.Bootstraps.Select(x => x.ResourcePath).ToArray();
                    allResourcePaths.AddRange(bootstrapResourcePaths);
                }

                if (import.Template.Maps != null)
                {
                    var mapResourcePaths = import.Template.Maps.Select(x => x.ResourcePath).ToArray();
                    allResourcePaths.AddRange(mapResourcePaths);
                }

                var distinctResourcePaths = allResourcePaths.Distinct().ToArray();

                var apiVersionId = _database.ApiVersions.Where(x => x.Version == import.ApiVersion).Select(x => x.Id).Single();

                return _database.Resources
                    .Where(x => distinctResourcePaths.Contains(x.Path) && x.ApiVersionId == apiVersionId)
                    .ToDictionary(x => x.Path);
            }
        }
    }
}
