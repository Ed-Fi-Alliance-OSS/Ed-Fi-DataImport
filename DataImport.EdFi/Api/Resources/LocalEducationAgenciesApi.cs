// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using DataImport.Common.ExtensionMethods;
using DataImport.EdFi.Models.Resources;
using RestSharp;

namespace DataImport.EdFi.Api.Resources
{
    public class LocalEducationAgenciesApi
    {
        private readonly IRestClient _client;
        private readonly string _apiVersion;

        public LocalEducationAgenciesApi(IRestClient client, string apiVersion)
        {
            _client = client;
            _apiVersion = apiVersion;
        }

        public LocalEducationAgency GetLocalEducationAgenciesById(string id)
        {
            var request = _apiVersion.IsOdsV2()
                ? new RestRequest("/localEducationAgencies/{id}", Method.Get)
                : new RestRequest("/ed-fi/localEducationAgencies/{id}", Method.Get);
            request.RequestFormat = DataFormat.Json;

            request.AddUrlSegment("id", id);
            if (id == null)
                throw new ArgumentException("API method call is missing required parameters");
            request.AddHeader("Accept", "application/json");
            var clientExecute = _client.ExecuteAsync<LocalEducationAgency>(request);
            clientExecute.Wait();
            var response = clientExecute.Result;

            return response.Data;
        }
    }
}
