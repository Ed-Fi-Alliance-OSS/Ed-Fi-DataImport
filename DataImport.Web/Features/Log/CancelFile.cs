// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common;
using DataImport.Models;
using DataImport.Web.Helpers;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataImport.Web.Features.Log
{
    public class CancelFile
    {
        public class Command : IRequest<ToastResponse>
        {
            public int Id { get; set; }
        }

        public class CommandHandler : IRequestHandler<Command, ToastResponse>
        {
            private readonly DataImportDbContext _dataImportDbContext;
            private readonly IFileService _fileService;

            public CommandHandler(IOptions<AppSettings> options, DataImportDbContext dataImportDbContext, ResolveFileService fileServices)
            {
                _dataImportDbContext = dataImportDbContext;
                _fileService = fileServices(options.Value.FileMode);
            }

            public Task<ToastResponse> Handle(Command request, CancellationToken cancellationToken)
            {
                var file = _dataImportDbContext.Files.Single(x => x.Id == request.Id);

                _fileService.Delete(file);

                file.Status = FileStatus.Canceled;
                file.UpdateDate = DateTimeOffset.Now;

                return Task.FromResult(new ToastResponse
                {
                    Message = $"File '{file.FileName}' set to {file.Status}."
                });
            }
        }
    }
}
