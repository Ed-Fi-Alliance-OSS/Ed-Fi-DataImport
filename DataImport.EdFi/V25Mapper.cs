// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Linq;

namespace DataImport.EdFi
{
    /// <summary>
    /// Static mappers for converting Ed-Fi ODS API v2.5 model types to current model types.
    /// </summary>
    public static class V25Mapper
    {
        public static Models.Resources.Assessment ToCurrentAssessment(
            this ModelsV25.Resources.Assessment src) =>
            new()
            {
                Id = src.Id,
                AssessmentCategoryDescriptor = src.CategoryDescriptor,
                AssessmentIdentifier = src.Identifier,
                AssessmentTitle = src.Title,
                Namespace = src.Namespace,
                AssessmentVersion = src.Version,
                AcademicSubjects = src.AcademicSubjects?
                    .Select(x => new Models.Resources.AssessmentAcademicSubject
                    {
                        AcademicSubjectDescriptor = x.AcademicSubjectDescriptor
                    }).ToList(),
                AssessedGradeLevels = src.AssessedGradeLevels?
                    .Select(x => new Models.Resources.AssessmentAssessedGradeLevel
                    {
                        GradeLevelDescriptor = x.GradeLevelDescriptor
                    }).ToList(),
                IdentificationCodes = src.IdentificationCodes?
                    .Select(x => new Models.Resources.AssessmentIdentificationCode
                    {
                        AssessmentIdentificationSystemDescriptor = x.AssessmentIdentificationSystemDescriptor
                    }).ToList(),
                PerformanceLevels = src.PerformanceLevels?
                    .Select(x => new Models.Resources.AssessmentPerformanceLevel
                    {
                        AssessmentReportingMethodDescriptor = x.AssessmentReportingMethodType,
                        PerformanceLevelDescriptor = x.PerformanceLevelDescriptor,
                        ResultDatatypeTypeDescriptor = x.ResultDatatypeType,
                        MinimumScore = x.MinimumScore,
                        MaximumScore = x.MaximumScore
                    }).ToList()
            };

        public static Models.EnrollmentComposite.School ToCurrentSchool(
            this ModelsV25.EnrollmentComposite.School src) =>
            new()
            {
                Id = src.Id,
                NameOfInstitution = src.NameOfInstitution,
                ShortNameOfInstitution = src.ShortNameOfInstitution,
                LocalEducationAgency = src.LocalEducationAgencyReference != null
                    ? new Models.EnrollmentComposite.SchoolLocalEducationAgency
                    {
                        Id = src.LocalEducationAgencyReference.Id
                    }
                    : null
            };

        public static Models.EnrollmentComposite.Section ToCurrentSection(
            this ModelsV25.EnrollmentComposite.Section src) =>
            new()
            {
                SectionIdentifier = src.UniqueSectionCode,
                SequenceOfCourse = src.SequenceOfCourse,
                EducationalEnvironmentDescriptor = src.EducationalEnvironmentType,
                AcademicSubjectDescriptor = src.AcademicSubjectDescriptor
            };
    }
}
