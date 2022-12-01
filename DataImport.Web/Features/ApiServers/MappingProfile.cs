// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using AutoMapper;

namespace DataImport.Web.Features.ApiServers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<DataImport.Models.ApiServer, ApiServerIndex.ApiServerModel>()
                .ForMember(dest => dest.ApiVersion, opt => opt.MapFrom(src => src.ApiVersion.Version));
        }
    }
}
