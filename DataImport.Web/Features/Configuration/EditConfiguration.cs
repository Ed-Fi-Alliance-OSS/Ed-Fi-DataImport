// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common;
using DataImport.Models;
using DataImport.Web.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.Configuration
{
    using Configuration = DataImport.Models.Configuration;

    public class EditConfiguration
    {
        public class Query : IRequest<ViewModel>
        {
            public bool OdsApiServerException { get; set; }
            public bool MissingConfigurationException { get; set; }
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
                var configuration = _database.Configurations.SingleOrDefault();

                var viewModel = new ViewModel
                {
                    InstanceAllowUserRegistration = configuration?.InstanceAllowUserRegistration ?? false,
                    ConfigurationFailureMsg = null,
                };

                if (request.MissingConfigurationException)
                {
                    viewModel.ConfigurationFailureMsg = "In order to proceed, please configure Data Import settings.";
                }
                else if (request.OdsApiServerException)
                {
                    viewModel.ConfigurationFailureMsg = "In order to proceed, please configure the ODS API Server.";
                }

                return viewModel;
            }
        }

        public class Command : IRequest
        {
            [Display(Name = "Allow User Registration")]
            public bool InstanceAllowUserRegistration { get; set; }
        }

        public class ViewModel : Command
        {
            public string ConfigurationFailureMsg { get; set; }
        }

        public class CommandHandler : AsyncRequestHandler<Command>
        {
            private readonly ILogger _logger;
            private readonly DataImportDbContext _database;

            public CommandHandler(ILogger<EditConfiguration> logger, DataImportDbContext database)
            {
                _logger = logger;
                _database = database;
            }

            protected override async Task Handle(Command message, CancellationToken cancellationToken)
            {
                var configuration = await _database.Configurations.SingleOrDefaultAsync(cancellationToken);
                if (configuration == null)
                {
                    configuration = new Configuration();

                    _database.Configurations.Add(configuration);
                }

                configuration.InstanceAllowUserRegistration = message.InstanceAllowUserRegistration;

                await _database.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Configuration was modified.");
            }
        }
    }
}