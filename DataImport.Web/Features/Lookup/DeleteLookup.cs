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
using System.Linq;

namespace DataImport.Web.Features.Lookup
{
    public class DeleteLookup
    {
        public class Command : IRequest<ToastResponse>
        {
            public int Id { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            private readonly DataImportDbContext _dbContext;

            public Validator(DataImportDbContext dbContext)
            {
                _dbContext = dbContext;
                RuleFor(x => x.Id)
                    .Must(NotDeleteReferencedLookUp)
                    .WithMessage(model => "The lookup cannot be deleted because it is referenced by another data map. Please remove all references before deleting the lookup.");
            }

            private bool NotDeleteReferencedLookUp(int id)
            {
                var lookup = _dbContext.Lookups.FirstOrDefault(x => x.Id == id);
                return LookUpNotReferenced(lookup?.SourceTable) ||
                       LookUpsCountMoreThanOne(lookup?.SourceTable);
            }

            private bool LookUpNotReferenced(string sourceTable) => !Enumerable.Any(_dbContext.DataMaps, dataMap => dataMap.ReferencedLookups().Contains(sourceTable));
            private bool LookUpsCountMoreThanOne(string sourceTable) => _dbContext.Lookups.Count(x => x.SourceTable == sourceTable) > 1;
        }

        public class CommandHandler : RequestHandler<Command, ToastResponse>
        {
            private readonly ILogger _logger;
            private readonly DataImportDbContext _dataImportDbContext;

            public CommandHandler(ILogger<DeleteLookup> logger, DataImportDbContext dataImportDbContext)
            {
                _logger = logger;
                _dataImportDbContext = dataImportDbContext;
            }

            protected override ToastResponse Handle(Command request)
            {
                var lookup = _dataImportDbContext.Lookups.Single(x => x.Id == request.Id);

                var lookupKey = lookup.Key;

                _logger.Deleted(lookup, l => l.Key);

                _dataImportDbContext.Lookups.Remove(lookup);

                return new ToastResponse
                {
                    Message = $"Lookup '{lookupKey}' was deleted."
                };
            }
        }
    }
}
