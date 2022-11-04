// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using System;
using System.Threading.Tasks;
using File = DataImport.Models.File;

namespace DataImport.Common.Helpers
{
    public interface IFileHelper
    {
        void LogFile(string fileName, int agentId, string url, FileStatus status, int rows);
        Task LogFileAsync(string fileName, int agentId, string url, FileStatus status, int rows);
    }

    public class FileHelper : IFileHelper
    {
        private readonly DataImportDbContext _dbContext;

        public FileHelper(DataImportDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void LogFile(string fileName, int agentId, string url, FileStatus status, int rows)
        {
            var fileLog = new File
            {
                AgentId = agentId,
                FileName = fileName,
                Url = url,
                Rows = rows,
                Status = status,
                CreateDate = DateTimeOffset.Now
            };

            _dbContext.Files.Add(fileLog);
            _dbContext.SaveChanges();
        }

        public async Task LogFileAsync(string fileName, int agentId, string url, FileStatus status, int rows)
        {
            var fileLog = new File
            {
                AgentId = agentId,
                FileName = fileName,
                Url = url,
                Rows = rows,
                Status = status,
                CreateDate = DateTimeOffset.Now
            };

            _dbContext.Files.Add(fileLog);
            await _dbContext.SaveChangesAsync();
        }
    }
}
