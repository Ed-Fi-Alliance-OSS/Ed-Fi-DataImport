// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using DataImport.Common.ExtensionMethods;
using DataImport.EdFi.Models.Resources;
using RestSharp;

namespace DataImport.EdFi.Api.Resources
{
    public class ObjectiveAssessmentsApi
    {
        private readonly IRestClient _client;
        private readonly string _apiVersion;

        public ObjectiveAssessmentsApi(IRestClient client, string apiVersion)
        {
            _client = client;
            _apiVersion = apiVersion;
        }

        public List<ObjectiveAssessment> GetObjectiveAssessmentsByAssessmentKey(int? offset = null, int? limit = null, string assessmentIdentifier = null, string @namespace = null)
        {
            var request = _apiVersion.IsOdsV2()
                ? new RestRequest("/objectiveAssessments", Method.GET)
                : new RestRequest("/ed-fi/objectiveAssessments", Method.GET);
            request.RequestFormat = DataFormat.Json;

            if (offset != null)
                request.AddParameter("offset", offset);
            if (limit != null)
                request.AddParameter("limit", limit);
            if (assessmentIdentifier != null)
                request.AddParameter("assessmentIdentifier", assessmentIdentifier);
            if (@namespace != null)
                request.AddParameter("@namespace", @namespace);
            request.AddHeader("Accept", "application/json");

            var response = _client.Execute<List<ObjectiveAssessment>>(request);

            return response.Data;
        }
    }
}
