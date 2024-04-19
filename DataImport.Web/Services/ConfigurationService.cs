// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataImport.Models;
using DataImport.Web.Services.Swagger;
using Microsoft.Extensions.Options;

namespace DataImport.Web.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly DataImportDbContext _database;
        private readonly ISwaggerMetadataFetcher _swaggerMetadataFetcher;
        private readonly IOptions<AppSettings> _options;

        public ConfigurationService(DataImportDbContext database,
            ISwaggerMetadataFetcher swaggerMetadataFetcher, IOptions<AppSettings> options)
        {
            _database = database;
            _swaggerMetadataFetcher = swaggerMetadataFetcher;
            _options = options;
        }

        public bool AllowUserRegistrations()
        {
            return _options.Value.AllowUserRegistration;
        }

        public async Task FillSwaggerMetadata(ApiServer apiServer)
        {
            if (apiServer.ApiVersion == null)
                throw new ArgumentException($"{nameof(ApiServer.ApiVersion)} must be populated in {nameof(apiServer)}", nameof(apiServer));

            IEnumerable<SwaggerResource> swaggerResources;
            try
            {
                swaggerResources = await _swaggerMetadataFetcher.GetMetadata(apiServer.Url, apiServer.ApiVersion.Version, apiServer.Tenant, apiServer.Context);
            }
            catch (OdsApiServerException e)
            {
                e.ApiServerId = apiServer.Id;
                throw;
            }

            var existingResources = _database.Resources.Where(x => x.ApiVersion.Version == apiServer.ApiVersion.Version).ToList();
            _database.Resources.RemoveRange(existingResources);

            var resources = new List<DataImport.Models.Resource>();

            foreach (var swaggerResource in swaggerResources)
            {
                var metadata = SwaggerMetadataParser.Parse(swaggerResource.Path, swaggerResource.Metadata);

                if (resources.Count(x => x.Path == swaggerResource.Path && x.ApiVersion == apiServer.ApiVersion) == 0)
                {
                    var resource = new DataImport.Models.Resource
                    {
                        Metadata = ResourceMetadata.Serialize(metadata),
                        Path = swaggerResource.Path,
                        ApiSection = swaggerResource.ApiSection,
                        ApiVersion = apiServer.ApiVersion
                    };

                    resources.Add(resource);
                }
            }

            _database.Resources.AddRange(resources);
        }

        public async Task<string> GetTokenUrl(string apiUrl, string apiVersion, string tenant, string context)
            => await _swaggerMetadataFetcher.GetTokenUrl(apiUrl, apiVersion, tenant, context);

        public async Task<string> GetAuthUrl(string apiUrl, string apiVersion, string tenant, string context)
            => await _swaggerMetadataFetcher.GetAuthUrl(apiUrl, apiVersion, tenant, context);

        public async Task<string> InferOdsApiVersion(string apiUrl)
            => await _swaggerMetadataFetcher.InferOdsApiVersion(apiUrl);
    }
}
