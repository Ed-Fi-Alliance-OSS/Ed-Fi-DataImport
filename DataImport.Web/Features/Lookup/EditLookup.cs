// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using AutoMapper;
using DataImport.Models;
using DataImport.Web.Helpers;
using DataImport.Web.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DataImport.Web.Features.Lookup
{
    public class EditLookup
    {
        public class Query : IRequest<Command>
        {
            public int Id { get; set; }
        }

        public class QueryHandler : RequestHandler<Query, Command>
        {
            private readonly DataImportDbContext _database;
            private readonly IMapper _mapper;

            public QueryHandler(DataImportDbContext database, IMapper mapper)
            {
                _database = database;
                _mapper = mapper;
            }

            protected override Command Handle(Query request)
            {
                var lookup = _database.Lookups.FirstOrDefault(x => x.Id == request.Id);

                return _mapper.Map<Command>(lookup);
            }
        }

        public class Command : IRequest<ToastResponse>
        {
            public int Id { get; set; }

            [Display(Name = "Source Table")]
            public string SourceTable { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }
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
                RuleFor(x => x).Must(NotEditReferencedLookUpSourceTableName).WithMessage(model =>
                    "The source table name cannot be edited because it is referenced by another data map. Please remove all references first before editing the lookup's source table name.");
                RuleFor(x => x).Must(BeUniqueLookUpItem).When(x =>
                        !string.IsNullOrEmpty(x.SourceTable) && !string.IsNullOrEmpty(x.Key) && !string.IsNullOrEmpty(x.Value))
                    .WithMessage(x => $"Lookup key '{x.Key}' already exists on the source table '{x.SourceTable}'. Please try different key.");
            }

            private bool BeUniqueLookUpItem(Command lookUp) =>
                !EditingLookUpItemWithDifferentIdButWithSameValues(lookUp) || EditingExistingLookUpWithoutChangingValues(lookUp);

            private bool EditingExistingLookUpWithoutChangingValues(Command lookUp) => _dbContext.Lookups.Any(x => x.Id == lookUp.Id && x.Key == lookUp.Key &&
                                                                            x.SourceTable == lookUp.SourceTable);

            private bool EditingLookUpItemWithDifferentIdButWithSameValues(Command lookUp) => _dbContext.Lookups.Any(x => x.Id != lookUp.Id &&
                x.SourceTable == lookUp.SourceTable.Trim() && x.Key == lookUp.Key.Trim());

            private bool NotEditReferencedLookUpSourceTableName(Command editLookUp)
            {
                var lookup = _dbContext.Lookups.FirstOrDefault(x => x.Id == editLookUp.Id);
                return LookUpNotReferenced(lookup?.SourceTable) ||
                       SameSourceTableName(lookup?.SourceTable, editLookUp.SourceTable) ||
                       LookUpsCountMoreThanOne(lookup?.SourceTable);
            }

            private bool LookUpNotReferenced(string sourceTable) => !Enumerable.Any(_dbContext.DataMaps, dataMap => dataMap.ReferencedLookups().Contains(sourceTable));
            private static bool SameSourceTableName(string sourceTableName, string editedSourceTableName) => sourceTableName == editedSourceTableName;
            private bool LookUpsCountMoreThanOne(string sourceTable) => _dbContext.Lookups.Count(x => x.SourceTable == sourceTable) > 1;
        }

        public class CommandHandler : RequestHandler<Command, ToastResponse>
        {
            private readonly ILogger<EditLookup> _logger;
            private readonly DataImportDbContext _database;

            public CommandHandler(ILogger<EditLookup> logger, DataImportDbContext database)
            {
                _logger = logger;
                _database = database;
            }

            protected override ToastResponse Handle(Command message)
            {
                var lookup = _database.Lookups.Single(x => x.Id == message.Id);

                lookup.SourceTable = message.SourceTable.Trim();
                lookup.Key = message.Key.Trim();
                lookup.Value = message.Value.Trim();

                _database.Lookups.Update(lookup);
                _database.SaveChanges();

                _logger.Modified(lookup, l => l.Key);

                return new ToastResponse
                {
                    Message = $"Lookup '{lookup.Key}' was modified."
                };
            }
        }
    }
}
