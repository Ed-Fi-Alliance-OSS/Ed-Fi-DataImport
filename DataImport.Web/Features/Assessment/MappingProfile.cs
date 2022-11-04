// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Linq;
using AutoMapper;
using DataImport.Web.Helpers;

namespace DataImport.Web.Features.Assessment
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<EdFi.Models.Resources.Assessment, AssessmentIndex.ViewModel.Assessment>()
                .ForMember(m => m.AssessmentIdentificationSystemDescriptor, opt => opt.Ignore())
                .ForMember(m => m.CategoryDescriptor, opt => opt.MapFrom(x => x.AssessmentCategoryDescriptor.ToDescriptorName()))
                .ForMember(m => m.AcademicSubjectDescriptor, opt => opt.MapFrom(x => string.Join(", ", x.AcademicSubjects.Select(a => a.AcademicSubjectDescriptor.ToDescriptorName()))))
                .ForMember(m => m.AssessedGradeLevelDescriptor, opt => opt.MapFrom(x => string.Join(", ", x.AssessedGradeLevels.Select(a => a.GradeLevelDescriptor.ToDescriptorName()))))
                .ForMember(m => m.Title, opt => opt.MapFrom(x => x.AssessmentTitle));

            CreateMap<EdFi.ModelsV25.Resources.Assessment, EdFi.Models.Resources.Assessment>()
                .ForMember(m => m.AssessmentCategoryDescriptor, opt => opt.MapFrom(x => x.CategoryDescriptor))
                .ForMember(m => m.AssessmentIdentifier, opt => opt.MapFrom(x => x.Identifier))
                .ForMember(m => m.AssessmentVersion, opt => opt.MapFrom(x => x.Version))
                .ForMember(m => m.AssessmentTitle, opt => opt.MapFrom(x => x.Title));

            CreateMap<EdFi.ModelsV25.Resources.AssessmentAcademicSubject, EdFi.Models.Resources.AssessmentAcademicSubject>();

            CreateMap<EdFi.ModelsV25.Resources.AssessmentAssessedGradeLevel, EdFi.Models.Resources.AssessmentAssessedGradeLevel>();

            CreateMap<EdFi.ModelsV25.Resources.AssessmentIdentificationCode, EdFi.Models.Resources.AssessmentIdentificationCode>();

            CreateMap<EdFi.ModelsV25.Resources.AssessmentPerformanceLevel, EdFi.Models.Resources.AssessmentPerformanceLevel>()
                .ForMember(m => m.AssessmentReportingMethodDescriptor, opt => opt.MapFrom(x => x.AssessmentReportingMethodType))
                .ForMember(m => m.ResultDatatypeTypeDescriptor, opt => opt.MapFrom(x => x.ResultDatatypeType));

            CreateMap<EdFi.Models.Resources.Assessment, AssessmentDetails.AssessmentDetail>()
                .ForMember(m => m.ObjectiveAssessments, opt => opt.Ignore())
                .ForMember(m => m.AssessmentCategoryDescriptor, opt => opt.MapFrom(x => x.AssessmentCategoryDescriptor.ToDescriptorName()))
                .ForMember(m => m.AcademicSubjects, opt => opt.MapFrom(x => string.Join(", ", x.AcademicSubjects.Select(a => a.AcademicSubjectDescriptor.ToDescriptorName()))))
                .ForMember(m => m.AssessedGradeLevels, opt => opt.MapFrom(x => string.Join(", ", x.AssessedGradeLevels.Select(a => a.GradeLevelDescriptor.ToDescriptorName()))))
                .ForMember(m => m.IdentificationCodes, opt => opt.MapFrom(x => string.Join(", ", x.IdentificationCodes.Select(a => a.AssessmentIdentificationSystemDescriptor.ToDescriptorName()))))
                .ForMember(m => m.ApiServerId, opt => opt.Ignore())
                .ForMember(m => m.ApiServers, opt => opt.Ignore());
        }
    }
}