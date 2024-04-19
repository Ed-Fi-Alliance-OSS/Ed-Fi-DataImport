// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataImport.Models;
using Newtonsoft.Json.Linq;

namespace DataImport.Web.Services.Swagger
{
    public class SwaggerMetadataProcessorV3 : ISwaggerMetadataProcessor
    {
        public bool CanHandle(JObject swaggerDocument)
        {
            var version = GetSwaggerDocumentVersion(swaggerDocument);
            return version == "3.0.1";
        }

        private static string GetSwaggerDocumentVersion(JObject swaggerDocument)
        {
            return swaggerDocument.Value<string>("openapi");
        }

        public Task<List<SwaggerResource>> GetMetadata(JObject swaggerDocument, ApiSection apiSection)
        {
            var resources = FetchMetadata(swaggerDocument, apiSection);
            return Task.FromResult(resources.ToList());
        }

        private IEnumerable<SwaggerResource> FetchMetadata(JObject swaggerDocument, ApiSection apiSection)
        {
            var swaggerVersion = GetSwaggerDocumentVersion(swaggerDocument);

            var apiOperations = swaggerDocument["paths"].ToObject<Dictionary<string, JObject>>();

            foreach (var apiPath in apiOperations.Keys)
            {
                var apiOperation = apiOperations[apiPath];

                //generally we only care about POST entities, so we'll filter
                //out and return only entities involved in a POST operation
                var entityReference = apiOperation["post"]?["requestBody"]?["content"]?["application/json"]?["schema"]?.Value<string>("$ref");
                if (entityReference == null)
                    continue;

                var swaggerEntityName = SwaggerHelpers.GetSwagger20EntityNameFromReference(entityReference);
                var postEntity = swaggerDocument["components"]["schemas"][swaggerEntityName];
                postEntity["resourcePath"] = apiPath;

                yield return new SwaggerResource
                {
                    Metadata = GetEntityMetadata(postEntity, swaggerDocument),
                    SwaggerVersion = swaggerVersion,
                    Path = apiPath,
                    ApiSection = apiSection
                };
            }
        }

        public string GetTokenUrl(JObject swaggerDocument)
            => swaggerDocument["components"]["securitySchemes"]["oauth2_client_credentials"]["flows"]["clientCredentials"].Value<string>("tokenUrl");

        public string GetTokenUrl(string apiUrl, string apiVersion, string tenant, string context) =>
            apiUrl.Replace("/data/v3/", "/oauth/token/");

        public string GetAuthUrl(JObject swaggerDocument) => null;

        private string GetEntityMetadata(JToken entity, JObject swaggerDocument)
        {
            var copyOfEntity = entity.DeepClone();

            var referenceDictionary = GetReferencedEntitiesByName(copyOfEntity, swaggerDocument);
            copyOfEntity["models"] = JObject.FromObject(referenceDictionary);
            copyOfEntity["swaggerVersion"] = "3.0";

            return copyOfEntity.ToString();
        }

        private Dictionary<string, JToken> GetReferencedEntitiesByName(JToken entity, JToken swaggerDocument)
        {
            var result = new Dictionary<string, JToken>();
            MapReferencedEntitiesIntoDictionary(entity, swaggerDocument, result);

            return result;
        }

        private void MapReferencedEntitiesIntoDictionary(JToken entity, JToken swaggerDocument, Dictionary<string, JToken> entityNameToObjectDictionary)
        {
            var refs = entity.SelectTokens("..['$ref']");
            foreach (var @ref in refs)
            {
                var entityName = SwaggerHelpers.GetSwagger20EntityNameFromReference(@ref.Value<string>());

                //if the entity name is already in the reference dictionary, no further processing
                //is necessary because we've already processed it before
                if (entityNameToObjectDictionary.ContainsKey(entityName)) continue;

                var referencedEntity = GetReferencedEntity(swaggerDocument, entityName);
                entityNameToObjectDictionary[entityName] = referencedEntity;

                MapReferencedEntitiesIntoDictionary(referencedEntity, swaggerDocument, entityNameToObjectDictionary);
            }
        }

        private JToken GetReferencedEntity(JToken swaggerDocument, string swaggerEntityName)
        {
            return swaggerDocument["components"]["schemas"][swaggerEntityName];
        }
    }
}
