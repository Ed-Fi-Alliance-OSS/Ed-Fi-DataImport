// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.ExtensionMethods;
using DataImport.Common.Helpers;
using DataImport.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using File = DataImport.Models.File;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;

namespace DataImport.Common
{
    public class AzureFileService : IFileService
    {
        private readonly ILogger<AzureFileService> _logger;
        private readonly IFileSettings _azureFileSettings;
        private readonly ConnectionStrings _connectionStrings;
        private readonly IFileHelper _fileHelper;

        public AzureFileService(ILogger<AzureFileService> logger, IOptions<ConnectionStrings> connectionStringsOptions, IFileSettings azureFileSettings, IFileHelper fileHelper)
        {
            _logger = logger;
            _connectionStrings = connectionStringsOptions.Value;
            _azureFileSettings = azureFileSettings;
            _fileHelper = fileHelper;
        }

        public async Task Upload(string fileName, Stream fileStream, Agent agent)
        {
            var fileShare = GetFileShare();

            if (await fileShare.ExistsAsync())
            {
                var fileDirectoryRoot = fileShare.GetRootDirectoryClient();
                var fileAgentDirectory = fileDirectoryRoot.GetSubdirectoryClient(agent.GetDirectory());

                await EnsureDataImportDirectoryExists(fileDirectoryRoot);
                await fileAgentDirectory.CreateIfNotExistsAsync();
                var cloudFile = fileAgentDirectory.GetFileClient($"{Guid.NewGuid()}-{fileName}");
                await cloudFile.UploadAsync(fileStream);
                var recordCount = fileStream.TotalLines(fileName.IsCsvFile());

                _fileHelper.LogFile(fileName, agent.Id, cloudFile.Uri.ToString(), FileStatus.Uploaded, recordCount);
                _logger.LogInformation("File '{File}' was uploaded to '{Uri}' for Agent '{Name}' (Id: {Id}).", fileName, cloudFile.Uri, agent.Name, agent.Id);
            }
            else
            {
                var message = $"The file share '{fileShare}' does not exist.";
                _logger.LogError(message);
                throw new Exception(message);
            }
        }

        public async Task Transfer(Stream stream, string file, Agent agent)
        {
            var shortFileName = file.Substring(file.LastIndexOf('/') + 1);

            var fileShare = GetFileShare();

            if (!await fileShare.ExistsAsync())
                _logger.LogError("Azure file share does not exist.");
            else
            {
                try
                {
                    var fileDirectoryRoot = fileShare.GetRootDirectoryClient();
                    var fileAgentDirectory = fileDirectoryRoot.GetSubdirectoryClient(agent.GetDirectory());

                    await EnsureDataImportDirectoryExists(fileDirectoryRoot);
                    await fileAgentDirectory.CreateIfNotExistsAsync();
                    var cloudFile = fileAgentDirectory.GetFileClient($"{Guid.NewGuid()}-{shortFileName}");
                    stream.Seek(0, SeekOrigin.Begin);
                    await cloudFile.UploadAsync(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    var recordCount = stream.TotalLines(file.IsCsvFile());

                    await _fileHelper.LogFileAsync(shortFileName, agent.Id, cloudFile.Uri.ToString(), FileStatus.Uploaded, recordCount);
                    _logger.LogInformation("Successfully transferred file {File} to {Uri} by agent ID: {Agent}", shortFileName, cloudFile.Uri, agent.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in TransferFile for file: {File} on site: ", agent.Url);
                    await _fileHelper.LogFileAsync(shortFileName, agent.Id, "", FileStatus.ErrorUploaded, 0);
                }
            }
        }

        public async Task<string> Download(File file)
        {
            var tempFileFullPath = Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid() + Path.GetExtension(file.FileName));

            var cloudFile = GetCloudFile(file);
            ShareFileDownloadInfo download = await cloudFile.DownloadAsync();
            using (FileStream stream = System.IO.File.OpenWrite(tempFileFullPath))
            {
                await download.Content.CopyToAsync(stream);
            }
            return tempFileFullPath;
        }

        public async Task Delete(File file)
        {
            var shareClient = GetFileShare();
            var cloudFile = GetCloudFile(file);
            string parentDirectoryName = Path.GetDirectoryName(cloudFile.Path);
            ShareDirectoryClient parentDirectoryClient = shareClient.GetDirectoryClient(parentDirectoryName);
            await cloudFile.DeleteAsync();
            await CleanAgentDirectoryIfEmpty(parentDirectoryClient);
        }

        private ShareClient GetFileShare()
        {
            var shareName = _azureFileSettings.ShareName;
            var connectionString = _connectionStrings.StorageConnection;
            var shareClient = new ShareClient(connectionString, shareName);
            return shareClient;
        }

        private ShareFileClient GetCloudFile(File file)
        {
            var shareName = _azureFileSettings.ShareName;
            var connectionString = _connectionStrings.StorageConnection;
            var fileUri = new Uri(file.Url);
            return new ShareFileClient(connectionString, shareName, fileUri.AbsolutePath);
        }

        private static async Task EnsureDataImportDirectoryExists(ShareDirectoryClient fileDirectoryRoot)
        {
            var fileAgentDirectory = fileDirectoryRoot.GetSubdirectoryClient("DataImport");
            await fileAgentDirectory.CreateIfNotExistsAsync();
        }

        private static async Task CleanAgentDirectoryIfEmpty(ShareDirectoryClient agentDirectory)
        {
            if (!(await ListFilesAndDirectories(agentDirectory)).Any())
                await agentDirectory.DeleteAsync();
        }

        private static async Task<IEnumerable<ShareFileItem>> ListFilesAndDirectories(ShareDirectoryClient directory)
        {
            var listResultItems = new List<ShareFileItem>();
            await foreach (ShareFileItem item in directory.GetFilesAndDirectoriesAsync())
            {
                listResultItems.Add(item);
            }
            return listResultItems;
        }

        public async Task<string> GetRowProcessorScript(string name)
        {
            return await GetScriptContent("RowProcessors", name);
        }

        public async Task<string> GetFileGeneratorScript(string name)
        {
            return await GetScriptContent("FileGenerators", name);
        }

        private async Task<string> GetScriptContent(string scriptFolder, string name)
        {
            var directory = GetFileShare()
                .GetRootDirectoryClient()
                .GetSubdirectoryClient(Path.Combine("DataImport", scriptFolder));

            var filesAndDirectories = await ListFilesAndDirectories(directory);

            var scriptToDownload = filesAndDirectories
                .OfType<ShareFileItem>()
                .Single(x => x.Name == name);
            if (scriptToDownload != null)
            {
                // Get a reference to the file
                ShareFileClient file = directory.GetFileClient(scriptToDownload.Name);

                // Download the file
                ShareFileDownloadInfo downloadFile = await file.DownloadAsync();

                // Convert the downloaded data to a string
                StreamReader reader = new StreamReader(downloadFile.Content);
                return await reader.ReadToEndAsync();
            }
            return null;
        }
        public async Task<bool> Exist(File file)
        {
            var cloudFile = GetCloudFile(file);
            return await cloudFile.ExistsAsync();
        }
    }
}
