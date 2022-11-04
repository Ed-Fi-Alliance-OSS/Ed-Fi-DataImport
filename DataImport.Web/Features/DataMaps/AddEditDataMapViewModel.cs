// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Web.Features.Shared;
using DataImport.Web.Features.Shared.SelectListProviders;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace DataImport.Web.Features.DataMaps
{
    public class AddEditDataMapViewModel : IApiVersionListViewModel, IApiServerListViewModel
    {
        [Display(Name = "Map Name")]
        public string MapName { get; set; }
        public string[] ColumnHeaders { get; set; }

        [Display(Name = "Map To Resource")]
        public string ResourceName { get; set; }

        [Display(Name = "Map To Resource")]
        public string ResourcePath { get; set; }

        public int DataMapId { get; set; }
        public DataTable CsvPreviewDataTable { get; set; }
        public DataMapperFieldsViewModel FieldsViewModel { get; set; }
        public bool MetadataIsIncompatible { get; set; }
        public List<SelectListItem> ApiVersions { get; set; }

        [Display(Name = "API Version")]
        public int? ApiVersionId { get; set; }

        [Display(Name = "API Version")]
        public string ApiVersion { get; set; }

        public List<PreprocessorDropDownItem> Preprocessors { get; set; }

        [Display(Name = "Processor")]
        public int? PreprocessorId { get; set; }

        public List<SelectListItem> ApiServers { get; set; }

        [Display(Name = "API Connection")]
        public int? ApiServerId { get; set; }

        public string Attribute { get; set; }

        public IList<LogMessageViewModel> PreprocessorLogMessages { get; set; }

        public string CsvError { get; set; }
    }
}