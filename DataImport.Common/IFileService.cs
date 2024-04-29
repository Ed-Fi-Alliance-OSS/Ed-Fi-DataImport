// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.IO;
using System.Threading.Tasks;
using DataImport.Models;
using File = DataImport.Models.File;

namespace DataImport.Common
{
    public interface IFileService
    {
        Task Upload(string fileName, Stream fileStream, Agent agent);
        Task Transfer(Stream stream, string file, Agent agent);
        Task<string> Download(File file);
        Task Delete(File file);
        Task<string> GetRowProcessorScript(string name);
        Task<string> GetFileGeneratorScript(string name);
        Task<bool> Exist(File file);
    }
}
