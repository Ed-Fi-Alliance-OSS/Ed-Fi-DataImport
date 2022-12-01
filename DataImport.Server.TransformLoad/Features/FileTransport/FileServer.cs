// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common;
using DataImport.Common.Enums;
using DataImport.Common.ExtensionMethods;
using DataImport.Models;
using FluentFTP;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DataImport.Server.TransformLoad.Features.FileTransport
{
    public interface IFileServer
    {
        Task<IEnumerable<string>> GetFileList(Agent agent);
        Task TransferFileToStorage(Agent agent, string fileName);
    }

    public class FtpsServer : IFileServer
    {
        private readonly ILogger<FtpServer> _logger;
        private readonly AppSettings _appSettings;
        private readonly IFileService _fileService;

        public FtpsServer(ILogger<FtpServer> logger, IOptions<AppSettings> options, ResolveFileService fileServices)
        {
            _logger = logger;
            _appSettings = options.Value;
            _fileService = fileServices(_appSettings.FileMode);
        }

        public async Task<IEnumerable<string>> GetFileList(Agent ftpsAgent)
        {
            var list = new List<string>();

            try
            {
                _logger.LogInformation("Connecting to host: {url}", ftpsAgent.Url);
                using (var client = CreateClient(ftpsAgent))
                {
                    client.ValidateCertificate += OnValidateFtpsCertificate;
                    await client.ConnectAsync();

                    list.AddRange(from file in await client.GetListingAsync(ftpsAgent.Directory)
                                  where file.Type == FtpFileSystemObjectType.File && file.Name.IsLike(ftpsAgent.FilePattern)
                                  select file.FullName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception in GetFileList()");
            }

            return list;
        }

        private void OnValidateFtpsCertificate(FtpClient control, FtpSslValidationEventArgs e)
        {
            if (_appSettings.AllowTestCertificates)
                e.Accept = true;
        }

        public async Task TransferFileToStorage(Agent agent, string fileName)
        {
            using (var stream = new MemoryStream())
            using (var client = CreateClient(agent))
            {
                client.ValidateCertificate += OnValidateFtpsCertificate;
                await client.ConnectAsync();
                await client.DownloadAsync(stream, fileName);
                await _fileService.Transfer(stream, fileName, agent);
            }
        }

        private FtpClient CreateClient(Agent agent)
        {
            return new FtpClient(agent.Url, Port(agent), agent.Username,
                Encryption.Decrypt(agent.Password, _appSettings.EncryptionKey))
            {
                EncryptionMode = FtpEncryptionMode.Implicit
            };
        }

        private int Port(Agent agent)
        {
            return agent.Port ?? AgentTypeCodeEnum.DefaultPort(AgentTypeCodeEnum.Ftps);
        }
    }

    public class SftpServer : IFileServer
    {
        private readonly ILogger<SftpServer> _logger;
        private readonly AppSettings _appSettings;
        private readonly IFileService _fileService;

        public SftpServer(ILogger<SftpServer> logger, IOptions<AppSettings> options, ResolveFileService fileServices)
        {
            _logger = logger;
            _appSettings = options.Value;
            _fileService = fileServices(_appSettings.FileMode);
        }

        public async Task<IEnumerable<string>> GetFileList(Agent sftpAgent)
        {
            var list = new List<string>();

            try
            {
                _logger.LogInformation("Connecting to host: {url}", sftpAgent.Url);
                using (var client = CreateClient(sftpAgent))
                {
                    client.Connect();
                    _logger.LogInformation("Connected, server version: {version}", client.ConnectionInfo.ServerVersion);

                    var fileList = client.ListDirectory(sftpAgent.Directory);
                    list.AddRange(from file in fileList
                                  where file.Name.IsLike(sftpAgent.FilePattern)
                                  select file.FullName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception in GetFileList()");
            }

            return await Task.FromResult(list);
        }

        public async Task TransferFileToStorage(Agent agent, string fileName)
        {
            using (var stream = new MemoryStream())
            using (var client = CreateClient(agent))
            {
                client.Connect();
                client.DownloadFile(fileName, stream);
                await _fileService.Transfer(stream, fileName, agent);
            }
        }

        private SftpClient CreateClient(Agent agent)
        {
            return new SftpClient(agent.Url, Port(agent), agent.Username,
                Encryption.Decrypt(agent.Password, _appSettings.EncryptionKey));
        }

        private static int Port(Agent agent)
        {
            return agent.Port ?? AgentTypeCodeEnum.DefaultPort(AgentTypeCodeEnum.Sftp);
        }
    }
}
