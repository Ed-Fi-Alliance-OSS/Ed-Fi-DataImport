// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;

namespace DataImport.Web.Features.Activity
{
    public static class Mapper
    {
        public static GetActivity.FileModel ToFileModel(this File src) =>
            new()
            {
                AgentName = src.Agent?.Name,
                FileName = src.FileName,
                CreateDate = src.CreateDate,
                Rows = src.Rows,
                Status = src.Status,
                ApiConnection = src.Agent?.ApiServer != null ? src.Agent.ApiServer.Name : string.Empty
            };
    }
}
