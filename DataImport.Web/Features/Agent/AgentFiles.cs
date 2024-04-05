// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Enums;
using DataImport.Common.ExtensionMethods;
using FluentFTP;
using FluentFTP.Client.BaseClient;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataImport.Web.Features.Agent
{
    public class AgentFiles
    {
        private static bool _allowTestCertificates;

        public class QueryResult
        {
            public IEnumerable<string> FileNames { get; set; }
            public string ErrorMessage { get; set; }
        }

        public class Query : IRequest<QueryResult>
        {
            public string FilePattern { get; set; }
            public string Url { get; set; }
            public int? Port { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Directory { get; set; }
            public string AgentTypeCode { get; set; }
        }

        public class QueryHandler : RequestHandler<Query, QueryResult>
        {
            private readonly ILogger _logger;

            public QueryHandler(ILogger<AgentFiles> logger, IOptions<AppSettings> options)
            {
                _logger = logger;
                _allowTestCertificates = options.Value.AllowTestCertificates;
            }

            protected override QueryResult Handle(Query request)
            {
                try
                {
                    if (string.IsNullOrEmpty(request.Url))
                        throw new Exception("'Host Name' must not be empty.");

                    if (string.IsNullOrEmpty(request.Username))
                        throw new Exception("'Username' must not be empty.");

                    if (string.IsNullOrEmpty(request.Password))
                        throw new Exception("'Password' must not be empty.");

                    var files = GetAgentFiles(request.Url, request.Port, request.Username, request.Password, request.Directory, request.FilePattern, request.AgentTypeCode);

                    return new QueryResult { FileNames = files.Select(x => x) };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving agent files.");
                    return new QueryResult { ErrorMessage = ex.Message };
                }
            }

            private static void OnValidateFtpsCertificate(BaseFtpClient control, FtpSslValidationEventArgs e)
            {
                if (_allowTestCertificates)
                    e.Accept = true;
            }

            private static IEnumerable<string> GetAgentFiles(string url, int? port, string username, string password, string directory, string filePattern, string agentType)
            {
                if (agentType == AgentTypeCodeEnum.Ftps)
                {
                    if (port == null)
                        port = AgentTypeCodeEnum.DefaultPort(AgentTypeCodeEnum.Ftps);

                    using (var ftpsClient = new FtpClient(url, username, password, port.Value))
                    {
                        ftpsClient.Config.EncryptionMode = FtpEncryptionMode.Implicit;
                        ftpsClient.ValidateCertificate += OnValidateFtpsCertificate;
                        ftpsClient.Connect();
                        if (!ftpsClient.IsConnected) throw new Exception("Ftps Client Cannot Connect");
                        return ftpsClient.GetListing(directory).Where(x =>
                            x.Type == FtpObjectType.File && x.Name.IsLike(filePattern.Trim())).Select(x => x.Name).ToList();
                    }
                }
                else
                {
                    if (port == null)
                        port = AgentTypeCodeEnum.DefaultPort(AgentTypeCodeEnum.Sftp);

                    using (var sftpClient = new SftpClient(new ConnectionInfo(url, port.Value, username,
                        new PasswordAuthenticationMethod(username, password))))
                    {
                        sftpClient.Connect();
                        if (!sftpClient.IsConnected) throw new Exception("Sftp Client Cannot Connect");
                        filePattern = filePattern.Trim();
                        return sftpClient.ListDirectory(directory).Where(x => x.Name.IsLike(filePattern))
                            .Select(x => x.Name).ToList();
                    }
                }
            }

        }
    }
}
