// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using AutoMapper;
using DataImport.EdFi;
using DataImport.EdFi.Api;
using DataImport.EdFi.Api.EnrollmentComposite;
using DataImport.EdFi.Api.Resources;
using DataImport.EdFi.Models;
using DataImport.EdFi.Models.EnrollmentComposite;
using DataImport.EdFi.Models.Resources;
using DataImport.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using School = DataImport.EdFi.Models.EnrollmentComposite.School;

namespace DataImport.Web.Services
{
    public abstract class EdFiServiceBase
    {
        protected IMapper Mapper { get; }

        public DataImportDbContext DatabaseContext { get; }

        protected EdFiServiceBase(IMapper mapper, DataImportDbContext databaseContext)
        {
            Mapper = mapper;
            DatabaseContext = databaseContext;
        }

        public abstract bool CanHandle(string apiVersion);

        public Task<List<Staff>> GetStaffBySchoolId(ApiServer apiServer, string schoolId, int offset, int limit)
        {
            return Query((client, apiVersion, yearSpecificYear) =>
            {
                var api = new EnrollmentApi(client, apiVersion.Version, yearSpecificYear);
                return api.GetStaffsBySchoolId(schoolId, offset, limit);
            }, apiServer);
        }

        public Task<List<Student>> GetStudentsBySchoolId(ApiServer apiServer, string schoolId, int offset, int limit)
        {
            return Query((client, apiVersion, yearSpecificYear) =>
            {
                var api = new EnrollmentApi(client, apiVersion.Version, yearSpecificYear);
                return api.GetStudentsBySchoolId(schoolId, offset, limit);
            }, apiServer);
        }

        public Task<List<Section>> GetSectionsBySchoolId(ApiServer apiServer, string schoolId, int offset, int limit)
        {
            return Query((client, apiVersion, yearSpecificYear) =>
            {
                var api = new EnrollmentApi(client, apiVersion.Version, yearSpecificYear, Mapper);
                return api.GetSectionsBySchoolId(schoolId, offset, limit);
            }, apiServer);
        }

        public Task<Assessment> GetAssessmentById(ApiServer apiServer, string id)
        {
            return Query((client, apiVersion, yearSpecificYear) =>
            {
                var api = new AssessmentsApi(client, apiVersion.Version, Mapper);
                return api.GetAssessmentById(id);
            }, apiServer);
        }

        public Task<List<ObjectiveAssessment>> GetObjectiveAssessmentsByAssessment(ApiServer apiServer, Assessment assessment, int offset,
            int limit)
        {
            return Query((client, apiVersion, yearSpecificYear) =>
            {
                var api = new ObjectiveAssessmentsApi(client, apiVersion.Version);
                return api.GetObjectiveAssessmentsByAssessmentKey(offset, limit, assessment.AssessmentIdentifier,
                    assessment.Namespace);
            }, apiServer);
        }

        public Task<List<School>> GetSchools(ApiServer apiServer, int? offset, int? limit)
        {
            return Query((client, apiVersion, yearSpecificYear) =>
            {
                var api = new EnrollmentApi(client, apiVersion.Version, yearSpecificYear, Mapper);
                return api.GetAllSchools(offset, limit);
            }, apiServer);
        }

        public Task<LocalEducationAgency> GetLocalEducationAgencyById(ApiServer apiServer, string id)
        {
            return Query((client, apiVersion, yearSpecificYear) =>
            {
                var api = new LocalEducationAgenciesApi(client, apiVersion.Version);
                return api.GetLocalEducationAgenciesById(id);
            }, apiServer);
        }

        public Task<List<Assessment>> GetResourceAssessments(ApiServer apiServer, int? offset, int? limit)
        {
            return Query((client, apiVersion, yearSpecificYear) =>
            {
                var api = new AssessmentsApi(client, apiVersion.Version, Mapper);
                return api.GetAllAssessments(offset, limit);
            }, apiServer);
        }

        public Task<EdFi.Models.Resources.School> GetSchool(ApiServer apiServer, string id)
        {
            return Query((client, apiVersion, yearSpecificYear) =>
            {
                var api = new SchoolsApi(client, apiVersion.Version);
                return api.GetSchoolById(id);
            }, apiServer);
        }

        public Task<List<Descriptor>> GetDescriptors(ApiServer apiServer, string descriptorPath, int? offset, int? limit)
        {
            return Query((client, apiVersion, yearSpecificYear) =>
            {
                var api = new DescriptorsApi(client);
                return api.GetAllDescriptors(descriptorPath, offset, limit);
            }, apiServer);
        }

        protected async Task<TResponseData> Query<TResponseData>(Func<IRestClient, ApiVersion, string, TResponseData> getData, ApiServer apiServer)
        {
            try
            {
                var apiVersion = apiServer.ApiVersion ?? DatabaseContext.ApiVersions.Single(x => x.Id == apiServer.ApiVersionId);

                string yearSpecificYear = await GetYearSpecificYear(apiServer, apiVersion);
                return getData(EstablishApiClient(apiServer), apiVersion, yearSpecificYear);
            }
            catch (DescriptorNotFoundException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new OdsApiServerException(apiServer, exception);
            }
        }

        protected abstract IRestClient EstablishApiClient(ApiServer apiServer);

        protected abstract Task<string> GetYearSpecificYear(ApiServer apiServer, ApiVersion apiVersion);
    }
}
