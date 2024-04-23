// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.Web.Helpers;
using DataImport.Web.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.Lookup
{
    public class AddLookup
    {
        public class Command : IRequest<Response>
        {
            [Display(Name = "Source Table")]
            public string SourceTable { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }
        }

        public class Response : ToastResponse
        {
            public int LookupId { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            private readonly DataImportDbContext _dbContext;

            public Validator(DataImportDbContext dbContext)
            {
                _dbContext = dbContext;
                RuleFor(x => x.SourceTable).NotEmpty().MaximumLength(1024);
                RuleFor(x => x.Key).NotEmpty().MaximumLength(1024);
                RuleFor(x => x.Value).NotEmpty().MaximumLength(1024);
                RuleFor(x => x).Must(BeUniqueLookUpItem).When(x =>
                        !string.IsNullOrEmpty(x.SourceTable) && !string.IsNullOrEmpty(x.Key))
                    .WithMessage(x => $"Lookup key '{x.Key}' already exists on the source table '{x.SourceTable}'. Please try different key.");
            }

            private bool BeUniqueLookUpItem(Command lookUp) => !_dbContext.Lookups.Any(x =>
                    x.SourceTable == lookUp.SourceTable.Trim() && x.Key == lookUp.Key.Trim());
        }

        public class AddHandler : IRequestHandler<Command, Response>
        {
            private readonly ILogger _logger;
            private readonly DataImportDbContext _database;

            public AddHandler(ILogger<AddLookup> logger, DataImportDbContext database)
            {
                _logger = logger;
                _database = database;
            }

            public Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {
                var lookup = new DataImport.Models.Lookup
                {
                    SourceTable = request.SourceTable.Trim(),
                    Key = request.Key.Trim(),
                    Value = request.Value.Trim()
                };

                _database.Lookups.Add(lookup);
                _database.SaveChanges();

                _logger.Added(lookup, l => l.Key);

                return Task.FromResult(new Response
                {
                    LookupId = lookup.Id,
                    Message = $"Lookup '{lookup.Key}' was created."
                });
            }
        }
    }
}
