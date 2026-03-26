// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Linq;
using DataImport.Web.Helpers;

namespace DataImport.Web.Features.Assessment
{
    public static class Mapper
    {
        public static AssessmentIndex.ViewModel.Assessment ToAssessmentIndexViewModel(this EdFi.Models.Resources.Assessment src) =>
            new()
            {
                Id = src.Id,
                Title = src.AssessmentTitle,
                CategoryDescriptor = src.AssessmentCategoryDescriptor.ToDescriptorName(),
                AcademicSubjectDescriptor = string.Join(", ", src.AcademicSubjects.Select(a => a.AcademicSubjectDescriptor.ToDescriptorName())),
                AssessedGradeLevelDescriptor = string.Join(", ", src.AssessedGradeLevels.Select(a => a.GradeLevelDescriptor.ToDescriptorName()))
                // AssessmentIdentificationSystemDescriptor is set by the handler after mapping
            };

        public static AssessmentDetails.AssessmentDetail ToAssessmentDetail(this EdFi.Models.Resources.Assessment src) =>
            new()
            {
                Id = src.Id,
                AssessmentCategoryDescriptor = src.AssessmentCategoryDescriptor.ToDescriptorName(),
                AssessmentIdentifier = src.AssessmentIdentifier,
                AssessmentTitle = src.AssessmentTitle,
                Namespace = src.Namespace,
                AssessmentVersion = src.AssessmentVersion,
                AcademicSubjects = string.Join(", ", src.AcademicSubjects.Select(a => a.AcademicSubjectDescriptor.ToDescriptorName())),
                AssessedGradeLevels = string.Join(", ", src.AssessedGradeLevels.Select(a => a.GradeLevelDescriptor.ToDescriptorName())),
                IdentificationCodes = string.Join(", ", src.IdentificationCodes.Select(a => a.AssessmentIdentificationSystemDescriptor.ToDescriptorName())),
                PerformanceLevels = src.PerformanceLevels
                // ObjectiveAssessments, ApiServerId, ApiServers are set by the handler after mapping
            };
    }
}
