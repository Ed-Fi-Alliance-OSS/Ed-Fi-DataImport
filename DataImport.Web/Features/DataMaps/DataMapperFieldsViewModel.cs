// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace DataImport.Web.Features.DataMaps
{
    public class DataMapperFieldsViewModel
    {
        public DataMapperFieldsViewModel()
        {
            ResourceMetadata = new List<ResourceMetadata>();
            Mappings = new List<DataMapper>();
            DataSources = new List<SelectListItem>();
            SourceColumns = new List<SelectListItem>();
            SourceTables = new List<SelectListItem>();
        }

        public IReadOnlyList<ResourceMetadata> ResourceMetadata { get; set; }
        public IReadOnlyList<DataMapper> Mappings { get; set; }
        public IReadOnlyList<SelectListItem> DataSources { get; set; }
        public IReadOnlyList<SelectListItem> SourceColumns { get; set; }
        public IReadOnlyList<SelectListItem> SourceTables { get; set; }
    }
}