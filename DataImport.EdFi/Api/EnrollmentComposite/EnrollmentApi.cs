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
        protected readonly IRestClient Client;
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
                var baseUrl = Common.Helpers.UrlUtility.RemoveAfterLastInstanceOf(Client.BaseUrl.ToString().Trim(), "/data/");
                Client.BaseUrl = new Uri(baseUrl);
            }
        }
      
        public List<School> GetAllSchools(int? offset= null, int? limit= null)
        {
            var request = _apiVersion.IsOdsV2()
                ? new RestRequest("/enrollment/schools", Method.GET)
                : new RestRequest($"{CompositePath}/ed-fi/enrollment/schools", Method.GET);
            request.RequestFormat = DataFormat.Json;

            if (offset != null)
                request.AddParameter("offset", offset);
            if (limit != null)
                request.AddParameter("limit", limit);

            return _apiVersion.IsOdsV2()
                ? Client.Execute<List<ModelsV25.EnrollmentComposite.School>>(request).Data
                    .Select(_mapper.Map<School>).ToList()
                : Client.Execute<List<School>>(request).Data;
        }

        public List<Section> GetSectionsBySchoolId(string schoolId, int? offset= null, int? limit= null)
        {
            var request = _apiVersion.IsOdsV2()
                ? new RestRequest("/enrollment/schools/{school_id}/sections", Method.GET)
                : new RestRequest($"{CompositePath}/ed-fi/enrollment/schools/{{school_id}}/sections", Method.GET);
            request.RequestFormat = DataFormat.Json;

            request.AddUrlSegment("school_id", schoolId);
            if (schoolId == null )
               throw new ArgumentException("API method call is missing required parameters");
            if (offset != null)
                request.AddParameter("offset", offset);
            if (limit != null)
                request.AddParameter("limit", limit);

            return _apiVersion.IsOdsV2()
                ? Client.Execute<List<ModelsV25.EnrollmentComposite.Section>>(request).Data
                    .Select(_mapper.Map<Section>).ToList()
                : Client.Execute<List<Section>>(request).Data;
        }

        public List<Student> GetStudentsBySchoolId(string schoolId, int? offset= null, int? limit= null)
        {
            var request = _apiVersion.IsOdsV2()
                ? new RestRequest("/enrollment/schools/{school_id}/students", Method.GET)
                : new RestRequest($"{CompositePath}/ed-fi/enrollment/schools/{{school_id}}/students", Method.GET);
            request.RequestFormat = DataFormat.Json;

            request.AddUrlSegment("school_id", schoolId);
            if (schoolId == null )
               throw new ArgumentException("API method call is missing required parameters");
            if (offset != null)
                request.AddParameter("offset", offset);
            if (limit != null)
                request.AddParameter("limit", limit);
            var response = Client.Execute<List<Student>>(request);

            return response.Data;
        }

        public List<Staff> GetStaffsBySchoolId(string schoolId, int? offset= null, int? limit= null)
        {
            var request = _apiVersion.IsOdsV2()
                ? new RestRequest("/enrollment/schools/{school_id}/staffs", Method.GET)
                : new RestRequest($"{CompositePath}/ed-fi/enrollment/schools/{{school_id}}/staffs", Method.GET);
            request.RequestFormat = DataFormat.Json;

            request.AddUrlSegment("school_id", schoolId);
            if (schoolId == null )
               throw new ArgumentException("API method call is missing required parameters");
            if (offset != null)
                request.AddParameter("offset", offset);
            if (limit != null)
                request.AddParameter("limit", limit);
            var response = Client.Execute<List<Staff>>(request);

            return response.Data;
        }
    }
}
