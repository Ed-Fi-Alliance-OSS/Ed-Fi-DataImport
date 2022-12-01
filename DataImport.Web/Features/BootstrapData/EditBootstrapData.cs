// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.Web.Helpers;
using DataImport.Web.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DataImport.Web.Features.BootstrapData
{
    public class EditBootstrapData
    {
        public class Query : IRequest<ViewModel>
        {
            public int BootstrapDataId { get; set; }
        }

        public class QueryHandler : RequestHandler<Query, ViewModel>
        {
            private readonly DataImportDbContext _database;

            public QueryHandler(DataImportDbContext database)
            {
                _database = database;
            }

            protected override ViewModel Handle(Query request)
            {
                var bootstrapData = _database.BootstrapDatas.Include(x => x.ApiVersion).AsNoTracking()
                    .Single(x => x.Id == request.BootstrapDataId);

                return new ViewModel
                {
                    Id = bootstrapData.Id,
                    Name = bootstrapData.Name,
                    Data = bootstrapData.Data,
                    ResourcePath = bootstrapData.ResourcePath,
                    ResourceName = bootstrapData.ToResourceName(),
                    MetadataIsIncompatible = bootstrapData.MetadataIsIncompatible(_database),
                    ApiVersion = bootstrapData.ApiVersion.Version
                };
            }
        }

        public class Command : IRequest<ToastResponse>
        {
            public int Id { get; set; }

            [Display(Name = "Bootstrap Name")]
            public string Name { get; set; }

            [DataType(DataType.MultilineText)]
            public string Data { get; set; }
        }

        public class ViewModel : Command
        {
            [Display(Name = "Resource")]
            public string ResourceName { get; set; }
            public string ResourcePath { get; set; }
            public bool MetadataIsIncompatible { get; set; }
            public string ApiVersion { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            private readonly DataImportDbContext _database;

            public Validator(ILogger<EditBootstrapData> logger, DataImportDbContext database, IJsonValidator jsonValidator)
            {
                _database = database;

                RuleFor(x => x.Name)
                    .NotEmpty()
                    .Must(BeAUniqueName).WithMessage(model => $"A Bootstrap Data named '{model.Name}' already exists. Bootstraps must have unique names.")
                    .WithName("Bootstrap Name");

                RuleFor(x => x.Data)
                    .Must(x => jsonValidator.IsValidJson(x))
                    .WithMessage("Please enter valid JSON.")
                    .DependentRules(() =>
                    {
                        RuleFor(x => x)
                            .SafeCustom(logger, (command, context) =>
                            {
                                var bootstrap = database.BootstrapDatas.Single(x => x.Id == command.Id);
                                var resource = database.Resources.SingleOrDefault(x => x.Path == bootstrap.ResourcePath && x.ApiVersionId == bootstrap.ApiVersionId);

                                if (resource == null)
                                {
                                    context.AddFailure($"Resource '{bootstrap.ResourcePath}' does not exist in the configured target ODS.");
                                }
                                else
                                {
                                    if (!JToken.Parse(command.Data).IsCompatibleWithResource(resource, MetadataCompatibilityLevel.Bootstrap, out string exceptionMessage))
                                    {
                                        context.AddFailure(
                                            $"Bootstrap JSON is not compatible with your definition of resource '{bootstrap.ResourcePath}'. {exceptionMessage}");
                                    }
                                }
                            });
                    });
            }

            private bool BeAUniqueName(Command command, string candidateName) =>
                EditingWithoutChangingAgentName(command, candidateName) || NewNameDoesNotAlreadyExist(command, candidateName);

            private bool EditingWithoutChangingAgentName(Command command, string candidateName) =>
                _database.BootstrapDatas.FirstOrDefault(bootstrap => bootstrap.Id == command.Id)?.Name == candidateName;

            private bool NewNameDoesNotAlreadyExist(Command command, string candidateName) =>
                _database.BootstrapDatas.FirstOrDefault(bootstrap => bootstrap.Name == candidateName && bootstrap.Id != command.Id) == null;
        }

        public class CommandHandler : RequestHandler<Command, ToastResponse>
        {
            private readonly ILogger<EditBootstrapData> _logger;
            private readonly DataImportDbContext _database;

            public CommandHandler(ILogger<EditBootstrapData> logger, DataImportDbContext database)
            {
                _logger = logger;
                _database = database;
            }

            protected override ToastResponse Handle(Command request)
            {
                var bootstrapData = _database.BootstrapDatas.Single(x => x.Id == request.Id);

                bootstrapData.Name = request.Name;
                bootstrapData.Data = request.Data;
                bootstrapData.UpdateDate = DateTimeOffset.Now;

                _logger.Modified(bootstrapData, b => b.Name);

                return new ToastResponse
                {
                    Message = $"Bootstrap Data '{bootstrapData.Name}' was modified."
                };
            }
        }
    }
}
