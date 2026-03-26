// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using DataImport.Models;

namespace DataImport.Web.Features.Log
{
    public static class Mapper
    {
        public static LogViewModel.File ToLogFile(this File src) =>
            new()
            {
                Id = src.Id,
                CreateDate = src.CreateDate.HasValue ? src.CreateDate.Value.ToString("yyyy-MM-dd hh:mm tt") : null,
                UpdateDate = src.UpdateDate.HasValue ? src.UpdateDate.Value.ToString("yyyy-MM-dd hh:mm tt") : null,
                NumberOfRows = src.Rows.GetValueOrDefault(),
                Status = src.Status,
                FileName = src.FileName,
                Message = src.Message,
                AgentName = src.Agent.Name + (src.Agent.Archived ? " (Archived)" : ""),
                ApiConnection = src.Agent.ApiServer != null ? src.Agent.ApiServer.Name : string.Empty
            };

        public static LogViewModel.Ingestion ToLogIngestion(this DataImport.Models.IngestionLog src) =>
            new()
            {
                Level = src.Level,
                Operation = src.Operation,
                Process = src.Process,
                FileName = src.FileName,
                Result = Enum.GetName(typeof(IngestionResult), src.Result),
                Date = src.Date.ToString("yyyy-MM-dd hh:mm tt"),
                // RowNumber is stored as a string in the ingestion log. When it isn't numeric, default to 0 (unknown) for display.
                RowNumber = int.TryParse(src.RowNumber, out var rowNum) ? rowNum : 0,
                EndPointUrl = src.EndPointUrl,
                HttpStatusCode = src.HttpStatusCode,
                Data = src.Data,
                OdsResponse = src.OdsResponse,
                Tenant = src.Tenant,
                Context = src.Context,
                EducationOrganizationId = src.EducationOrganizationId?.ToString()
            };

        public static LogViewModel.ApplicationLog ToLogApplicationLog(this DataImport.Models.ApplicationLog src) =>
            new()
            {
                LoggedDate = src.Logged.ToString("yyyy-MM-dd hh:mm tt"),
                Level = src.Level,
                Message = src.Message,
                UserName = src.UserName,
                Logger = src.Logger,
                Exception = src.Exception
            };
    }
}
