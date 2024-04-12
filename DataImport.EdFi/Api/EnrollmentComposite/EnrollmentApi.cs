// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DataImport.Common.ExtensionMethods;
using DataImport.EdFi.Models.EnrollmentComposite;
using RestSharp;

namespace DataImport.EdFi.Api.EnrollmentComposite
{
    public class EnrollmentApi
    {
        public readonly IRestClient Client;
        private readonly string _apiVersion;
        private readonly IMapper _mapper;
        protected readonly string CompositePath;

        public EnrollmentApi(IRestClient client, string apiVersion, string year, IMapper mapper = null)
        {
            Client = client;
            _apiVersion = apiVersion;
            _mapper = mapper;

            if (!apiVersion.IsOdsV2())
            {
                CompositePath = year == null ? "/composites/v1" : $"/composites/v1/{year}";
                var baseUrl = Common.Helpers.UrlUtility.RemoveAfterLastInstanceOf(Client.Options.BaseUrl.ToString().Trim(), "/data/");
                var options = new RestClientOptions();
                options.Authenticator = client.Options.Authenticator;
                options.BaseUrl = new Uri(baseUrl);
                Client = new RestClient(options);
            }
        }

        public List<School> GetAllSchools(int? offset = null, int? limit = null)
        {
            var request = _apiVersion.IsOdsV2()
                ? new RestRequest("/enrollment/schools", Method.Get)
                : new RestRequest($"{CompositePath}/ed-fi/enrollment/schools", Method.Get);
            request.RequestFormat = DataFormat.Json;

            if (offset != null)
                request.AddParameter("offset", offset, ParameterType.HttpHeader);
            if (limit != null)
                request.AddParameter("limit", limit, ParameterType.HttpHeader);
            if (!_apiVersion.IsOdsV2())
            {
                var clientExecute =
                 Client.ExecuteAsync<List<School>>(request);
                clientExecute.Wait();
                return clientExecute.Result.Data;
            }
            else
            {
                var clientExecute =
                 Client.ExecuteAsync<List<ModelsV25.EnrollmentComposite.School>>(request);
                clientExecute.Wait();
                return clientExecute.Result.Data
                    .Select(_mapper.Map<School>).ToList();
            }
        }

        public List<Section> GetSectionsBySchoolId(string schoolId, int? offset = null, int? limit = null)
        {
            var request = _apiVersion.IsOdsV2()
                ? new RestRequest("/enrollment/schools/{school_id}/sections", Method.Get)
                : new RestRequest($"{CompositePath}/ed-fi/enrollment/schools/{{school_id}}/sections", Method.Get);
            request.RequestFormat = DataFormat.Json;

            request.AddUrlSegment("school_id", schoolId);
            if (schoolId == null)
                throw new ArgumentException("API method call is missing required parameters");
            if (offset != null)
                request.AddParameter("offset", offset, ParameterType.HttpHeader);
            if (limit != null)
                request.AddParameter("limit", limit, ParameterType.HttpHeader);

            if (!_apiVersion.IsOdsV2())
            {
                var clientExecute = Client.ExecuteAsync<List<Section>>(request);
                clientExecute.Wait();
                return clientExecute.Result.Data;
            }
            else
            {
                var clientExecute = Client.ExecuteAsync<List<ModelsV25.EnrollmentComposite.Section>>(request);
                clientExecute.Wait();
                return clientExecute.Result.Data
                    .Select(_mapper.Map<Section>).ToList();
            }
        }

        public List<Student> GetStudentsBySchoolId(string schoolId, int? offset = null, int? limit = null)
        {
            var request = _apiVersion.IsOdsV2()
                ? new RestRequest("/enrollment/schools/{school_id}/students", Method.Get)
                : new RestRequest($"{CompositePath}/ed-fi/enrollment/schools/{{school_id}}/students", Method.Get);
            request.RequestFormat = DataFormat.Json;

            request.AddUrlSegment("school_id", schoolId);
            if (schoolId == null)
                throw new ArgumentException("API method call is missing required parameters");
            if (offset != null)
                request.AddParameter("offset", offset, ParameterType.HttpHeader);
            if (limit != null)
                request.AddParameter("limit", limit, ParameterType.HttpHeader);
            var clientExecute = Client.ExecuteAsync<List<Student>>(request);
            clientExecute.Wait();
            var response = clientExecute.Result;

            return response.Data;
        }

        public List<Staff> GetStaffsBySchoolId(string schoolId, int? offset = null, int? limit = null)
        {
            var request = _apiVersion.IsOdsV2()
                ? new RestRequest("/enrollment/schools/{school_id}/staffs", Method.Get)
                : new RestRequest($"{CompositePath}/ed-fi/enrollment/schools/{{school_id}}/staffs", Method.Get);
            request.RequestFormat = DataFormat.Json;

            request.AddUrlSegment("school_id", schoolId);
            if (schoolId == null)
                throw new ArgumentException("API method call is missing required parameters");
            if (offset != null)
                request.AddParameter("offset", offset, ParameterType.HttpHeader);
            if (limit != null)
                request.AddParameter("limit", limit, ParameterType.HttpHeader);
            var clientExecute = Client.ExecuteAsync<List<Staff>>(request);
            clientExecute.Wait();
            var response = clientExecute.Result;

            return response.Data;
        }
    }
}
