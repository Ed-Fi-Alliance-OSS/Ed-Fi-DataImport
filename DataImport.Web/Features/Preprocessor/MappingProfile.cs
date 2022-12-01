// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using AutoMapper;
using DataImport.Models;

namespace DataImport.Web.Features.Preprocessor
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AddEditPreprocessorViewModel, Script>()
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.DataMaps, opt => opt.Ignore());

            CreateMap<Script, AddEditPreprocessorViewModel>()
                .ForMember(m => m.ExternalPreprocessorsEnabled, opt => opt.Ignore())
                .ForMember(m => m.ScriptTypes, opt => opt.Ignore());

            CreateMap<Script, PreprocessorIndex.PreprocessorIndexModel>()
                .ForMember(x => x.UsedBy, opt => opt.Ignore());
        }
    }
}
