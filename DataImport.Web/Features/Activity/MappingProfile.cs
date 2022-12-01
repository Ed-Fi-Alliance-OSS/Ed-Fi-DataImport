// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using AutoMapper;
using DataImport.Models;

namespace DataImport.Web.Features.Activity
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
            => CreateMap<File, GetActivity.FileModel>()
                .ForMember(m => m.ApiConnection, opt => opt.MapFrom(x => x.Agent.ApiServer != null ? x.Agent.ApiServer.Name : string.Empty))
                .ForMember(m => m.CreateDate,
                    opt => opt.MapFrom(source =>
                        source.CreateDate.HasValue ? source.CreateDate.Value.ToString("yyyy-MM-dd hh:mm tt") : null));
    }
}
