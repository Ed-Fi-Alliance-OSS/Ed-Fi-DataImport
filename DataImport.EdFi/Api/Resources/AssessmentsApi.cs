// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DataImport.Common.ExtensionMethods;
using DataImport.EdFi.Models.Resources;
using RestSharp;

namespace DataImport.EdFi.Api.Resources
{
    public class AssessmentsApi
    {
        private readonly IRestClient _client;
        private readonly string _apiVersion;
        private readonly IMapper _mapper;

        public AssessmentsApi(IRestClient client, string apiVersion, IMapper mapper)
        {
            _client = client;
            _apiVersion = apiVersion;
            _mapper = mapper;
        }

        public List<Assessment> GetAllAssessments(int? offset = null, int? limit = null)
        {
            var request = _apiVersion.IsOdsV2()
                ? new RestRequest("/assessments", Method.Get)
                : new RestRequest("/ed-fi/assessments", Method.Get);
            request.RequestFormat = DataFormat.Json;

            if (offset != null)
                request.AddParameter("offset", offset, ParameterType.HttpHeader);
            if (limit != null)
                request.AddParameter("limit", limit, ParameterType.HttpHeader);
            request.AddHeader("Accept", "application/json");

            if (!_apiVersion.IsOdsV2())
            {
                var clientExecute =
                    _client.ExecuteAsync<List<Assessment>>(request);
                clientExecute.Wait();
                return clientExecute.Result.Data
                    .Select(_mapper.Map<Assessment>).ToList();
            }
            else
            {
                var clientExecute =
                    _client.ExecuteAsync<List<ModelsV25.Resources.Assessment>>(request);
                clientExecute.Wait();
                return clientExecute.Result.Data
                    .Select(_mapper.Map<Assessment>).ToList();
            }
        }

        public Assessment GetAssessmentById(string id)
        {
            var request = _apiVersion.IsOdsV2()
                ? new RestRequest("/assessments/{id}", Method.Get)
                : new RestRequest("/ed-fi/assessments/{id}", Method.Get);
            request.RequestFormat = DataFormat.Json;

            request.AddUrlSegment("id", id);
            if (id == null)
                throw new ArgumentException("API method call is missing required parameters");
            request.AddHeader("Accept", "application/json");
            if (!_apiVersion.IsOdsV2())
            {
                var clientExecute =
                    _client.ExecuteAsync<Assessment>(request);
                clientExecute.Wait();
                return clientExecute.Result.Data;
            }
            else
            {
                var clientExecute =
                    _client.ExecuteAsync<ModelsV25.Resources.Assessment>(request);
                clientExecute.Wait();
                return _mapper.Map<Assessment>(clientExecute.Result.Data);
            }
        }
    }
}
