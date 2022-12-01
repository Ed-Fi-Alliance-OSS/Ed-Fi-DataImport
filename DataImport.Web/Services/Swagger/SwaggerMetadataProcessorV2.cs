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
    public class SwaggerMetadataProcessorV2 : ISwaggerMetadataProcessor
    {
        public bool CanHandle(JObject swaggerDocument)
        {
            var version = GetSwaggerDocumentVersion(swaggerDocument);
            return version == "2.0";
        }

        private static string GetSwaggerDocumentVersion(JObject swaggerDocument)
        {
            return swaggerDocument.Value<string>("swagger");
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
                var entityReference = apiOperation["post"]?["parameters"]?[0]?["schema"]?.Value<string>("$ref");
                if (entityReference == null)
                    continue;

                var swaggerEntityName = SwaggerHelpers.GetSwagger20EntityNameFromReference(entityReference);
                var postEntity = swaggerDocument["definitions"][swaggerEntityName];
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
            => swaggerDocument["securityDefinitions"]["oauth2_client_credentials"].Value<string>("tokenUrl");

        public string GetAuthUrl(JObject swaggerDocument) => null;

        private string GetEntityMetadata(JToken entity, JObject swaggerDocument)
        {
            //Swagger 2.0 has a number of differences in format compared to v1.2
            //The most important difference is that we get one large Swagger doc with all
            //API endpoints and models.
            //
            //Since the app stores the Swagger metadata on a per-entity basis
            //for later processing/mapping, we need the metadata we return to be
            //self contained.  Therefore, any references in the Swagger doc ("$ref")
            //are added into a new "models" collection on the object, which mirrors
            //Swagger v1.2 behavior

            var copyOfEntity = entity.DeepClone();

            var referenceDictionary = GetReferencedEntitiesByName(copyOfEntity, swaggerDocument);
            copyOfEntity["models"] = JObject.FromObject(referenceDictionary);
            copyOfEntity["swaggerVersion"] = "2.0";

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
            return swaggerDocument["definitions"][swaggerEntityName];
        }
    }
}
