// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using AutoMapper;
using Section = DataImport.EdFi.ModelsV25.EnrollmentComposite.Section;

namespace DataImport.Web.Features.School
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Section, EdFi.Models.EnrollmentComposite.Section>()
                .ForMember(m => m.SectionIdentifier, opt => opt.MapFrom(x => x.UniqueSectionCode))
                .ForMember(m => m.EducationalEnvironmentDescriptor, opt => opt.MapFrom(x => x.EducationalEnvironmentType));

            CreateMap<EdFi.ModelsV25.EnrollmentComposite.School, EdFi.Models.EnrollmentComposite.School>()
                .ForMember(m => m.LocalEducationAgency, opt => opt.MapFrom(x => x.LocalEducationAgencyReference));

            CreateMap<EdFi.ModelsV25.EnrollmentComposite.SchoolLocalEducationAgency, EdFi.Models.EnrollmentComposite.SchoolLocalEducationAgency>();
        }
    }
}