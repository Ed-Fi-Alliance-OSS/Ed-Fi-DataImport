// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using DataImport.Models;
using MediatR;

namespace DataImport.Server.TransformLoad.Features.Events
{
    public class JobCompleted
    {
        public class Command : IRequest
        {
        }

        public class EventHandler : IRequestHandler<Command>
        {
            private readonly DataImportDbContext _dbContext;

            public EventHandler(DataImportDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public Task Handle(Command request, CancellationToken cancellationToken)
            {
                var jobStatus = _dbContext.EnsureSingle<JobStatus>();
                jobStatus.Completed = DateTimeOffset.Now;
                _dbContext.SaveChanges();
                return Task.CompletedTask;
            }
        }
    }
}
