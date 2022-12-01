// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DataImport.Models;
using DataImport.Web.Features.Shared;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataImport.Web.Features.Share
{
    public abstract class ExportBase
    {
        public abstract class CommandBase : IApiVersionSpecificRequest
        {
            public BootstrapSelection[] Bootstraps { get; set; }
            public DataMapSelection[] DataMaps { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            [Display(Name = "API Version")]
            public string ApiVersion { get; set; }

            public string GetApiVersion()
            {
                return ApiVersion;
            }
        }

        public abstract class ValidatorBase<T> : AbstractValidator<T> where T : CommandBase
        {
            protected ValidatorBase()
            {
                RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
                RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
                RuleFor(x => x.ApiVersion).NotEmpty().WithMessage("Please configure Data Import with an ODS API before exporting.");

                When(x => x.Bootstraps != null && x.Bootstraps.Count(b => b.Selected) > 1, () =>
                {
                    RuleFor(x => x.Bootstraps).Must(ValidateSelectedBootstraps).WithMessage("Exported Bootstrap Data Definitions must have the same API Version.");
                });

                When(x => x.DataMaps != null && x.DataMaps.Count(b => b.Selected) > 1, () =>
                {
                    RuleFor(x => x.DataMaps).Must(ValidateSelectedDataMaps).WithMessage("Exported Data Maps must have the same API Version.");
                });

                RuleFor(x => new { x.Bootstraps, x.DataMaps }).Must(x =>
                {
                    if (x.DataMaps == null || x.Bootstraps == null)
                    {
                        return true;
                    }

                    var apiVersions = x.DataMaps.Where(y => y.Selected).Select(y => y.ApiVersion).ToList();
                    apiVersions.AddRange(x.Bootstraps.Where(y => y.Selected).Select(y => y.ApiVersion));

                    return apiVersions.Distinct().Count() <= 1;
                }).WithMessage("Exported Bootstrap Data Definitions and Data Maps must have the same API Version.");
            }

            private bool ValidateSelectedDataMaps(DataMapSelection[] dataMapSelections)
            {
                return dataMapSelections.Where(x => x.Selected).Select(x => x.ApiVersion).Distinct().Count() == 1;
            }

            private bool ValidateSelectedBootstraps(BootstrapSelection[] bootstrapSelections)
            {
                return bootstrapSelections.Where(x => x.Selected).Select(x => x.ApiVersion).Distinct().Count() == 1;
            }
        }

        public abstract class Selection
        {
            public int Id { get; set; }
            public bool Selected { get; set; }
        }

        public class BootstrapSelection : Selection
        {
            public string Name { get; set; }
            public string ResourcePath { get; set; }
            public string ApiVersion { get; set; }
        }

        public class DataMapSelection : Selection
        {
            public string Name { get; set; }
            public string ResourcePath { get; set; }
            public string Lookups { get; set; }
            public string ApiVersion { get; set; }
            public string CustomFileProcessor { get; set; }
        }

        protected static BootstrapSelection[] BootstrapSelections(DataImportDbContext database)
        {
            return database.BootstrapDatas
                .Include(x => x.ApiVersion)
                .OrderBy(x => x.Name)
                .ToArray()
                .Select(x => new BootstrapSelection
                {
                    Id = x.Id,
                    Name = x.Name,
                    ResourcePath = x.ResourcePath,
                    ApiVersion = x.ApiVersion.Version
                })
                .ToArray();
        }

        protected static DataMapSelection[] DataMapSelections(DataImportDbContext database)
        {
            return database.DataMaps
                .Include(x => x.ApiVersion)
                .Include(x => x.FileProcessorScript)
                .OrderBy(x => x.Name)
                .ToArray()
                .Select(x => new DataMapSelection
                {
                    Id = x.Id,
                    Name = x.Name,
                    ResourcePath = x.ResourcePath,
                    ApiVersion = x.ApiVersion.Version,
                    Lookups = string.Join(", ", ReferencedLookups(x)),
                    CustomFileProcessor = x.FileProcessorScript?.Name
                })
                .ToArray();
        }

        protected static SharingBootstrap[] BootstrapExports(DataImportDbContext database, CommandBase request)
        {
            var bootstrapIds = SelectedIds(request.Bootstraps);

            if (!bootstrapIds.Any())
                return new SharingBootstrap[] { };

            return database.BootstrapDatas
                .Where(x => bootstrapIds.Contains(x.Id))
                .OrderBy(x => x.Name)
                .ToArray()
                .Select(x => new SharingBootstrap
                {
                    Name = x.Name,
                    ResourcePath = x.ResourcePath,
                    Data = JToken.Parse(x.Data)
                })
                .ToArray();
        }

        protected static SharingMap[] DataMapExports(DataImportDbContext database, CommandBase request, out SharingLookup[] lookupExports, out SharingPreprocessor[] sharingPreprocessors)
        {
            var dataMapIds = SelectedIds(request.DataMaps);

            if (!dataMapIds.Any())
            {
                lookupExports = new SharingLookup[] { };
                sharingPreprocessors = new SharingPreprocessor[] { };
                return new SharingMap[] { };
            }

            var dataMaps = database.DataMaps
                .Include(x => x.FileProcessorScript)
                .Where(x => dataMapIds.Contains(x.Id))
                .OrderBy(x => x.Name)
                .ToArray();

            var mapExports = dataMaps
                .Select(x => new SharingMap
                {
                    Name = x.Name,
                    ResourcePath = x.ResourcePath,
                    ColumnHeaders = x.ColumnHeaders == null
                        ? null
                        : JsonConvert.DeserializeObject<string[]>(x.ColumnHeaders),
                    Map = JObject.Parse(x.Map),
                    Attribute = x.FileProcessorScript != null && x.FileProcessorScript.HasAttribute ? x.Attribute : null,
                    CustomFileProcessor = x.FileProcessorScript?.Name
                })
                .ToArray();

            lookupExports = LookupExports(database, dataMaps);
            sharingPreprocessors = GetUniquePreprocessors(dataMaps);
            return mapExports;
        }

        private static SharingPreprocessor[] GetUniquePreprocessors(DataMap[] dataMaps)
        {
            return dataMaps
                .Where(x => x.FileProcessorScript != null)
                .Select(x => x.FileProcessorScript)
                .GroupBy(x => x.Id)
                .Select(x => x.First())
                .Select(x => new SharingPreprocessor
                {
                    Name = x.Name,
                    HasAttribute = x.HasAttribute,
                    RequireOdsApiAccess = x.RequireOdsApiAccess,
                    ScriptContent = x.ScriptContent,
                    ExecutablePath = x.ExecutablePath,
                    ExecutableArguments = x.ExecutableArguments,
                })
                .OrderBy(x => x.Name)
                .ToArray();
        }

        private static SharingLookup[] LookupExports(DataImportDbContext database, DataMap[] dataMaps)
        {
            var lookups = ReferencedLookups(dataMaps);

            return database.Lookups
                .Where(x => lookups.Contains(x.SourceTable))
                .OrderBy(x => x.SourceTable)
                .ThenBy(x => x.Key)
                .ThenBy(x => x.Value)
                .ToArray()
                .Select(x => new SharingLookup
                {
                    SourceTable = x.SourceTable,
                    Key = x.Key,
                    Value = x.Value
                })
                .ToArray();
        }

        private static int[] SelectedIds(IReadOnlyList<Selection> selections)
        {
            if (selections == null)
                return new int[] { };

            return selections.Where(x => x.Selected).Select(x => x.Id).ToArray();
        }

        private static string[] ReferencedLookups(params DataMap[] dataMaps)
        {
            return dataMaps
                .SelectMany(dataMap => dataMap.ReferencedLookups())
                .Distinct()
                .OrderBy(x => x)
                .ToArray();
        }
    }
}
