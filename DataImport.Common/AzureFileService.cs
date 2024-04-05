// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.ExtensionMethods;
using DataImport.Common.Helpers;
using DataImport.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using File = DataImport.Models.File;

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
                var fileDirectoryRoot = fileShare.GetRootDirectoryReference();
                var fileAgentDirectory = fileDirectoryRoot.GetDirectoryReference(agent.GetDirectory());

                await EnsureDataImportDirectoryExists(fileDirectoryRoot);
                await fileAgentDirectory.CreateIfNotExistsAsync();
                var cloudFile = fileAgentDirectory.GetFileReference($"{Guid.NewGuid()}-{fileName}");
                await cloudFile.UploadFromStreamAsync(fileStream);
                var recordCount = fileStream.TotalLines(fileName.IsCsvFile());

                _fileHelper.LogFile(fileName, agent.Id, cloudFile.StorageUri.PrimaryUri.ToString(), FileStatus.Uploaded, recordCount);
                _logger.LogInformation("File '{File}' was uploaded to '{Uri}' for Agent '{Name}' (Id: {Id}).", fileName, cloudFile.StorageUri.PrimaryUri, agent.Name, agent.Id);
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
                    var fileDirectoryRoot = fileShare.GetRootDirectoryReference();
                    var fileAgentDirectory = fileDirectoryRoot.GetDirectoryReference(agent.GetDirectory());

                    await EnsureDataImportDirectoryExists(fileDirectoryRoot);
                    await fileAgentDirectory.CreateIfNotExistsAsync();
                    var cloudFile = fileAgentDirectory.GetFileReference($"{Guid.NewGuid()}-{shortFileName}");
                    stream.Seek(0, SeekOrigin.Begin);
                    await cloudFile.UploadFromStreamAsync(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    var recordCount = stream.TotalLines(file.IsCsvFile());

                    await _fileHelper.LogFileAsync(shortFileName, agent.Id, cloudFile.StorageUri.PrimaryUri.ToString(), FileStatus.Uploaded, recordCount);
                    _logger.LogInformation("Successfully transferred file {File} to {Uri} by agent ID: {Agent}", shortFileName, cloudFile.StorageUri.PrimaryUri, agent.Id);
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
            await cloudFile.DownloadToFileAsync(tempFileFullPath, FileMode.Create);

            return tempFileFullPath;
        }

        public async Task Delete(File file)
        {
            var cloudFile = GetCloudFile(file);
            await cloudFile.DeleteAsync();
            await CleanAgentDirectoryIfEmpty(cloudFile.Parent);
        }

        private CloudFileShare GetFileShare()
        {
            var storageAccount = GetStorageAccount();
            var fileClient = storageAccount.CreateCloudFileClient();
            return fileClient.GetShareReference(_azureFileSettings.ShareName);
        }

        private CloudFile GetCloudFile(File file)
        {
            var fileUri = new Uri(file.Url);
            var storageAccount = GetStorageAccount();
            storageAccount.CreateCloudFileClient();
            return new CloudFile(fileUri, storageAccount.Credentials);
        }

        private CloudStorageAccount GetStorageAccount()
        {
            var azureFileConnectionString = _connectionStrings.StorageConnection;
            return CloudStorageAccount.Parse(azureFileConnectionString);
        }

        private static async Task EnsureDataImportDirectoryExists(CloudFileDirectory fileDirectoryRoot)
        {
            var fileAgentDirectory = fileDirectoryRoot.GetDirectoryReference("DataImport");
            await fileAgentDirectory.CreateIfNotExistsAsync();
        }

        private static async Task CleanAgentDirectoryIfEmpty(CloudFileDirectory agentDirectory)
        {
            if (!(await ListFilesAndDirectories(agentDirectory)).Any())
                await agentDirectory.DeleteAsync();
        }

        private static async Task<IEnumerable<IListFileItem>> ListFilesAndDirectories(CloudFileDirectory directory)
        {
            FileContinuationToken token = null;
            var listResultItems = new List<IListFileItem>();
            do
            {
                FileResultSegment resultSegment = await directory.ListFilesAndDirectoriesSegmentedAsync(token);
                token = resultSegment.ContinuationToken;

                foreach (IListFileItem listResultItem in resultSegment.Results)
                {
                    listResultItems.Add(listResultItem);
                }
            }
            while (token != null);

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
                .GetRootDirectoryReference()
                .GetDirectoryReference(Path.Combine("DataImport", scriptFolder));

            var filesAndDirectories = await ListFilesAndDirectories(directory);

            return await filesAndDirectories
                .OfType<CloudFile>()
                .Single(x => x.Name == name)
                .DownloadTextAsync();
        }
    }
}
