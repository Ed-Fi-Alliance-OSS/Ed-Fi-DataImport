// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using AutoMapper;
using DataImport.Models;

namespace DataImport.Web.Features.Log
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<File, LogViewModel.File>()
                .ForMember(m => m.ApiConnection, opt => opt.MapFrom(x => x.Agent.ApiServer != null ? x.Agent.ApiServer.Name : string.Empty))
                .ForMember(m => m.CreateDate,
                    opt => opt.MapFrom(source =>
                        source.CreateDate.HasValue ? source.CreateDate.Value.ToString("yyyy-MM-dd hh:mm tt") : null))
                .ForMember(m => m.UpdateDate,
                    opt => opt.MapFrom(source =>
                        source.UpdateDate.HasValue ? source.UpdateDate.Value.ToString("yyyy-MM-dd hh:mm tt") : null))
                .ForMember(m => m.NumberOfRows, opt => opt.MapFrom(source =>
                    source.Rows.GetValueOrDefault()))
                .ForMember(m => m.AgentName, opt => opt.MapFrom(source => source.Agent.Name + (source.Agent.Archived ? " (Archived)" : "")));

            CreateMap<DataImport.Models.IngestionLog, LogViewModel.Ingestion>()
                .ForMember(destination => destination.Result,
                    opt => opt.MapFrom(source => Enum.GetName(typeof(IngestionResult), source.Result)))
                .ForMember(destination => destination.Date,
                    opt => opt.MapFrom(source => source.Date.ToString("yyyy-MM-dd hh:mm tt")));

            CreateMap<DataImport.Models.ApplicationLog, LogViewModel.ApplicationLog>()
                .ForMember(m => m.LoggedDate,
                    opt => opt.MapFrom(source => source.Logged.ToString("yyyy-MM-dd hh:mm tt")));
        }
    }
}
