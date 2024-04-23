// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.Web.Features.Shared;
using DataImport.Web.Helpers;
using DataImport.Web.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.BootstrapData
{
    public class AddBootstrapData
    {
        public class Query : IRequest<Command>
        {
        }

        public class QueryHandler : IRequestHandler<Query, Command>
        {
            public Task<Command> Handle(Query request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new Command());
            }
        }

        public class Command : IRequest<Response>, IApiVersionListViewModel
        {
            public int Id { get; set; }

            [Display(Name = "Bootstrap Name")]
            public string Name { get; set; }

            [Display(Name = "Resource")]
            public string ResourcePath { get; set; }

            [DataType(DataType.MultilineText)]
            public string Data { get; set; }

            public List<SelectListItem> ApiVersions { get; set; }

            [Display(Name = "API Version")]
            public int? ApiVersionId { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            private readonly DataImportDbContext _database;

            public Validator(ILogger logger, DataImportDbContext database, IJsonValidator jsonValidator)
            {
                _database = database;

                RuleFor(x => x.ApiVersionId).NotEmpty().WithName("API Version");

                RuleFor(x => x.ResourcePath).NotEmpty().WithName("Resource");

                RuleFor(x => x.Name)
                    .NotEmpty()
                    .WithName("Bootstrap Name");

                RuleFor(x => x.Name)
                    .Must(BeAUniqueName).WithMessage(model => $"A Bootstrap Data named '{model.Name}' already exists. Bootstraps must have unique names.");

                RuleFor(x => x.Data)
                    .Must(x => jsonValidator.IsValidJson(x))
                    .WithMessage("Please enter valid JSON.")
                    .DependentRules(() =>
                    {
                        RuleFor(x => x)
                            .SafeCustom(logger, (command, context) =>
                            {
                                var resource = database.Resources.SingleOrDefault(x => x.Path == command.ResourcePath && x.ApiVersionId == command.ApiVersionId);

                                if (resource != null)
                                {
                                    if (!JToken.Parse(command.Data).IsCompatibleWithResource(resource, MetadataCompatibilityLevel.Bootstrap, out string exceptionMessage))
                                    {
                                        context.AddFailure($"Bootstrap JSON is not compatible with your definition of resource '{command.ResourcePath}'. {exceptionMessage}");
                                    }
                                }
                            });
                    });
            }

            private bool BeAUniqueName(string candidateName) =>
                _database.BootstrapDatas.FirstOrDefault(bootstrap => bootstrap.Name == candidateName) == null;
        }

        public class Response : ToastResponse
        {
            public int BootstrapDataId { get; set; }
        }

        public class CommandHandler : IRequestHandler<Command, Response>
        {
            private readonly ILogger _logger;
            private readonly DataImportDbContext _database;

            public CommandHandler(ILogger<AddBootstrapData> logger, DataImportDbContext database)
            {
                _logger = logger;
                _database = database;
            }

            public Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {
                var resource = _database.Resources.Single(x => x.Path == request.ResourcePath && x.ApiVersionId == request.ApiVersionId);

                var bootstrapData = new DataImport.Models.BootstrapData
                {
                    Name = request.Name,
                    ResourcePath = resource.Path,
                    Data = request.Data,
                    Metadata = resource.Metadata,
                    CreateDate = DateTimeOffset.Now,
                    UpdateDate = DateTimeOffset.Now,
                    ApiVersionId = resource.ApiVersionId
                };

                _database.BootstrapDatas.Add(bootstrapData);
                _database.SaveChanges();

                _logger.Added(bootstrapData, b => b.Name);

                return Task.FromResult(new Response
                {
                    BootstrapDataId = bootstrapData.Id,
                    Message = $"Bootstrap Data '{bootstrapData.Name}' was created."
                });
            }
        }
    }
}
