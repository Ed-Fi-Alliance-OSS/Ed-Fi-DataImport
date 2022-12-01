// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common;
using DataImport.Common.ExtensionMethods;
using DataImport.Common.Helpers;
using DataImport.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DataImport.Server.TransformLoad.Tests.Features.FileTransport
{
    public class TestLocalFileService : LocalFileService
    {
        private readonly IFileHelper _fileHelper;
        private readonly AppSettings _appSettings;

        public TestLocalFileService(ILogger<TestLocalFileService> logger, IOptions<AppSettings> options, IFileHelper fileHelper)
            : base(logger, options.Value, fileHelper)
        {
            _fileHelper = fileHelper;
            _appSettings = options.Value;
        }

        public override async Task Transfer(Stream stream, string file, Agent agent)
        {
            var shortFileName = file.Substring(file.LastIndexOf('/') + 1);
            var fileMode = _appSettings.FileMode;
            var localFilePath = Path.Combine(_appSettings.ShareName, fileMode, agent.GetDirectory(), shortFileName);
            var localFileUri = new Uri(localFilePath);
            stream.Seek(0, SeekOrigin.Begin);
            var recordCount = stream.TotalLines(true);
            await _fileHelper.LogFileAsync(file, agent.Id, localFileUri.AbsoluteUri, FileStatus.Uploaded, recordCount);
        }
    }
}
