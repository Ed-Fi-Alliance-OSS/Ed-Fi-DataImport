// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common;
using DataImport.Models;
using DataImport.Server.TransformLoad.Features.FileTransport;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.Server.TransformLoad.Tests.Features.FileTransport
{
    public class TestFtpsServer : IFileServer
    {
        private readonly IFileService _fileService;

        public TestFtpsServer(IOptions<AppSettings> options, ResolveFileService fileServices)
        {
            _fileService = fileServices(options.Value.FileMode);
        }

        public Task<IEnumerable<string>> GetFileList(Agent ftpsAgent)
        {
            IEnumerable<string> result = new List<string> { "FtpsFile1", "FtpsFile2" };
            return Task.FromResult(result);
        }

        public async Task TransferFileToStorage(Agent agent, string fileName)
        {
            const string fileContent = "ftps file content";
            var byteArray = Encoding.ASCII.GetBytes(fileContent);
            using (var stream = new MemoryStream(byteArray))
            {
                await _fileService.Transfer(stream, fileName, agent);
            }
        }
    }

    public class TestSftpServer : IFileServer
    {
        private readonly IFileService _fileService;

        public TestSftpServer(IOptions<AppSettings> options, ResolveFileService fileServices)
        {
            _fileService = fileServices(options.Value.FileMode);
        }

        public Task<IEnumerable<string>> GetFileList(Agent sftpAgent)
        {
            IEnumerable<string> enumerable = new List<string> { "SftpFile1", "SftpFile2" };
            return Task.FromResult(enumerable);
        }

        public async Task TransferFileToStorage(Agent agent, string fileName)
        {
            const string fileContent = "sftp file content";
            var byteArray = Encoding.ASCII.GetBytes(fileContent);
            using (var stream = new MemoryStream(byteArray))
            {
                await _fileService.Transfer(stream, fileName, agent);
            }
        }
    }
}