// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.Web.Helpers;
using DataImport.Web.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace DataImport.Web.Features.DataMaps
{
    public class DeleteDataMap
    {
        public class Command : IRequest<ToastResponse>
        {
            public int Id { get; set; }
        }

        public class CommandHandler : RequestHandler<Command, ToastResponse>
        {
            private readonly ILogger<DeleteDataMap> _logger;
            private readonly DataImportDbContext _database;

            public CommandHandler(ILogger<DeleteDataMap> logger, DataImportDbContext database)
            {
                _logger = logger;
                _database = database;
            }

            protected override ToastResponse Handle(Command request)
            {
                var dataMap = _database.DataMaps.Include(x => x.DataMapAgents).Single(x => x.Id == request.Id);

                var dataMapName = dataMap.Name;

                _logger.Deleted(dataMap, d => d.Name);

                foreach (var agentAssociation in dataMap.DataMapAgents.ToList())
                    _database.DataMapAgents.Remove(agentAssociation);

                _database.DataMaps.Remove(dataMap);

                return new ToastResponse
                {
                    Message = $"Data Map '{dataMapName}' was deleted."
                };
            }
        }
    }
}