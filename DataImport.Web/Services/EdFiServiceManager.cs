// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.EdFi.Models;
using DataImport.EdFi.Models.Resources;
using DataImport.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using School = DataImport.EdFi.Models.EnrollmentComposite.School;
using Section = DataImport.EdFi.Models.EnrollmentComposite.Section;
using Staff = DataImport.EdFi.Models.EnrollmentComposite.Staff;
using Student = DataImport.EdFi.Models.EnrollmentComposite.Student;

namespace DataImport.Web.Services
{
    public class EdFiServiceManager
    {
        private readonly IEnumerable<EdFiServiceBase> _edFiServices;
        private readonly DataImportDbContext _dbContext;

        public EdFiServiceManager(IEnumerable<EdFiServiceBase> edFiServices, DataImportDbContext dbContext)
        {
            _edFiServices = edFiServices;
            _dbContext = dbContext;
        }

        public Task<List<Staff>> GetStaffBySchoolId(int apiServerId, string schoolId, int offset, int limit)
        {
            return GetEdFiService(apiServerId, out var apiServer).GetStaffBySchoolId(apiServer, schoolId, offset, limit);
        }

        public Task<List<Student>> GetStudentsBySchoolId(int apiServerId, string schoolId, int offset, int limit)
        {
            return GetEdFiService(apiServerId, out var apiServer).GetStudentsBySchoolId(apiServer, schoolId, offset, limit);
        }

        public Task<List<Section>> GetSectionsBySchoolId(int apiServerId, string schoolId, int offset, int limit)
        {
            return GetEdFiService(apiServerId, out var apiServer).GetSectionsBySchoolId(apiServer, schoolId, offset, limit);
        }

        public Task<Assessment> GetAssessmentById(int apiServerId, string id)
        {
            return GetEdFiService(apiServerId, out var apiServer).GetAssessmentById(apiServer, id);
        }

        public Task<List<ObjectiveAssessment>> GetObjectiveAssessmentsByAssessment(int apiServerId, Assessment assessment, int offset, int limit)
        {
            return GetEdFiService(apiServerId, out var apiServer).GetObjectiveAssessmentsByAssessment(apiServer, assessment, offset, limit);
        }

        public Task<List<School>> GetSchools(int apiServerId, int? offset, int? limit)
        {
            return GetEdFiService(apiServerId, out var apiServer).GetSchools(apiServer, offset, limit);
        }

        public Task<LocalEducationAgency> GetLocalEducationAgencyById(int apiServerId, string id)
        {
            return GetEdFiService(apiServerId, out var apiServer).GetLocalEducationAgencyById(apiServer, id);
        }

        public Task<List<Assessment>> GetResourceAssessments(int apiServerId, int? offset, int? limit)
        {
            return GetEdFiService(apiServerId, out var apiServer).GetResourceAssessments(apiServer, offset, limit);
        }

        public Task<EdFi.Models.Resources.School> GetSchool(int apiServerId, string id)
        {
            return GetEdFiService(apiServerId, out var apiServer).GetSchool(apiServer, id);
        }

        public Task<List<Descriptor>> GetDescriptors(int apiServerId, string descriptorPath, int? offset, int? limit)
        {
            return GetEdFiService(apiServerId, out var apiServer).GetDescriptors(apiServer, descriptorPath, offset, limit);
        }

        private EdFiServiceBase GetEdFiService(int apiServerId, out ApiServer apiServer)
        {
            apiServer = _dbContext.ApiServers.Include(x => x.ApiVersion).SingleOrDefault(x => x.Id == apiServerId);

            if (apiServer == null)
            {
                throw new OdsApiServerException(new Exception("No ODS API server configured"));
            }

            return GetEdFiService(apiServer);
        }

        private EdFiServiceBase GetEdFiService(ApiServer apiServer)
        {
            var apiVersion = apiServer.ApiVersion;

            var service = _edFiServices?.FirstOrDefault(p => p.CanHandle(apiVersion.Version));
            if (service == null)
                throw new NotSupportedException("No handler available to process Swagger document");

            return service;
        }
    }
}
