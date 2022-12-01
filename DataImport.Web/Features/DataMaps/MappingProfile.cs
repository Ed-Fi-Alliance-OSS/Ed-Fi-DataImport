// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using AutoMapper;
using DataImport.Models;

namespace DataImport.Web.Features.DataMaps
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<DataMap, DataMapIndex.ViewModel>()
                .ForMember(m => m.ResourceName, opt => opt.MapFrom(x => x.ToResourceName()));
        }
    }
}
