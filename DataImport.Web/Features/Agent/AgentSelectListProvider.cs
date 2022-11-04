// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Enums;
using DataImport.Models;
using DataImport.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace DataImport.Web.Features.Agent
{
    public class AgentSelectListProvider
    {
        private readonly DataImportDbContext _dataImportDbContext;

        public AgentSelectListProvider(DataImportDbContext dataImportDbContext)
        {
            _dataImportDbContext = dataImportDbContext;
        }

        public IReadOnlyList<SelectListItem> GetAgentTypes()
        {
            return AgentTypeCodeEnum.ToList()
                .ToSelectListItems("Select Type", getText: DisplayName);
        }

        private static string DisplayName(string value)
        {
            var field = typeof(AgentTypeCodeEnum)
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .Single(x => (string)x.GetValue(null) == value);

            var display = field.GetCustomAttribute<DisplayAttribute>();

            return display != null ? display.Name : value;
        }

        public IEnumerable<DropdownItem> GetDataMapList()
        {
            var emptyDataMaps =
                new List<DropdownItem> { new DropdownItem { Text = "Select Map", Value = string.Empty } };

            emptyDataMaps.AddRange(_dataImportDbContext.DataMaps.Select(x =>
                new DropdownItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }));

            return emptyDataMaps;
        }

        public List<AgentBootstrapDataDropdownItem> GetBootstrapDataList()
        {
            var emptyDataMaps =
                new List<AgentBootstrapDataDropdownItem> { new AgentBootstrapDataDropdownItem { Text = "Select Bootstrap Data", Value = string.Empty } };

            emptyDataMaps.AddRange(_dataImportDbContext.BootstrapDatas.Select(x =>
                new AgentBootstrapDataDropdownItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),
                    Resource = x.ResourcePath,
                }));

            return emptyDataMaps;
        }

        public List<SelectListItem> GetRowProcessors()
        {
            return GetProcessors(ScriptType.CustomRowProcessor, "Select Row Processor");
        }

        public List<SelectListItem> GetFileGenerators()
        {
            return _dataImportDbContext.Scripts
                .Where(x => x.ScriptType == ScriptType.CustomFileGenerator || x.ScriptType == ScriptType.ExternalFileGenerator)
                .AsNoTracking()
                .ToList()
                .ToSelectListItems("Select File Generator", x => x.Id.ToString(CultureInfo.InvariantCulture), x => x.Name);
        }

        private List<SelectListItem> GetProcessors(ScriptType scriptType, string selectRowText)
        {
            return _dataImportDbContext.Scripts
                .Where(x => x.ScriptType == scriptType)
                .AsNoTracking()
                .ToList()
                .ToSelectListItems(selectRowText, x => x.Id.ToString(CultureInfo.InvariantCulture), x => x.Name);
        }
    }
}