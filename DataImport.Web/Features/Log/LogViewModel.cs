// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Enums;
using DataImport.Models;
using DataImport.Web.Features.Shared;
using DataImport.Web.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace DataImport.Web.Features.Log
{
    public class LogViewModel : IApiServerListViewModel
    {
        public PagedList<File> Files { get; set; }

        public PagedList<Ingestion> IngestionLogs { get; set; }

        public PagedList<ApplicationLog> ApplicationLogs { get; set; }

        public IngestionResult Results => new IngestionResult();

        public EdFiHttpStatus HttpStatuses => new EdFiHttpStatus();

        public List<SelectListItem> Tenants = new List<SelectListItem>();

        public List<SelectListItem> Contexts = new List<SelectListItem>();

        public List<SelectListItem> EducationOrganizationIds = new List<SelectListItem>();

        public Filters LogFilters { get; set; }

        public class Filters
        {
            public int SelectedResult { get; set; }
            public int SelectedResponse { get; set; }
            public string Filename { get; set; }
            public string SelectedTenant { get; set; }
            public string SelectedContext { get; set; }
            public int? EducationOrganizationId { get; set; }

        }

        public class Ingestion
        {
            public string Level { get; set; }
            public string Operation { get; set; }
            public string Process { get; set; }
            public string FileName { get; set; }
            public string Result { get; set; }
            public string Date { get; set; }
            public int RowNumber { get; set; }
            public string EndPointUrl { get; set; }
            public string HttpStatusCode { get; set; }
            public string Data { get; set; }
            public string OdsResponse { get; set; }
            public string Tenant { get; set; }
            public string Context { get; set; }
            public string EducationOrganizationId { get; set; }
        }

        public class File
        {
            public int Id { get; set; }
            public string CreateDate { get; set; }
            public string UpdateDate { get; set; }
            public int NumberOfRows { get; set; }
            public FileStatus Status { get; set; }
            public string FileName { get; set; }
            public string AgentName { get; set; }
            public string Message { get; set; }
            public string ApiConnection { get; set; }
        }

        public class ApplicationLog
        {
            public string LoggedDate { get; set; }
            public string Level { get; set; }
            public string Message { get; set; }
            public string UserName { get; set; }
            public string Logger { get; set; }
            public string Exception { get; set; }
        }

        public List<SelectListItem> ApiServers { get; set; }

        public int? ApiServerId { get; set; }
    }
}
