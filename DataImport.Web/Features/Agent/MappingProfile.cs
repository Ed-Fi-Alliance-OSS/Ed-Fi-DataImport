// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using AutoMapper;

namespace DataImport.Web.Features.Agent
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<DataImport.Models.Agent, AddEditAgentViewModel>()
                .ForMember(dest => dest.RowProcessors, opt => opt.Ignore())
                .ForMember(dest => dest.FileGenerators, opt => opt.Ignore())
                .ForMember(dest => dest.RowProcessorId, opt => opt.MapFrom(x => x.RowProcessorScriptId))
                .ForMember(dest => dest.FileGeneratorId, opt => opt.MapFrom(x => x.FileGeneratorScriptId))
                .ForMember(dest => dest.DdlDataMaps, opt => opt.Ignore())
                .ForMember(dest => dest.DdlSchedules, opt => opt.Ignore())
                .ForMember(dest => dest.DataMaps, opt => opt.Ignore())
                .ForMember(dest => dest.AgentTypes, opt => opt.Ignore())
                .ForMember(dest => dest.MappedAgents, opt => opt.MapFrom(src => src.DataMapAgents))
                .ForMember(dest => dest.AgentBootstrapDatas, opt => opt.MapFrom(src => src.BootstrapDataAgents))
                .ForMember(dest => dest.ActionFileCode, opt => opt.MapFrom(x => x.ActionFileCode))
                .ForMember(dest => dest.ActionFiles, opt => opt.Ignore())
                .ForMember(dest => dest.EncryptionFailureMsg, opt => opt.Ignore())
                .ForMember(dest => dest.ApiServers, opt => opt.Ignore())
                .ForMember(dest => dest.BootstrapDatas, opt => opt.Ignore())
                .ForMember(dest => dest.DdlBootstrapDatas, opt => opt.Ignore());

            CreateMap<DataImport.Models.DataMapAgent, MappedAgent>();

            CreateMap<DataImport.Models.BootstrapDataAgent, AgentBootstrapData>()
                .ForMember(x => x.BootstrapName, opt => opt.MapFrom(src => src.BootstrapData.Name))
                .ForMember(x => x.Resource, opt => opt.MapFrom(src => src.BootstrapData.ResourcePath));

            CreateMap<DataImport.Models.AgentSchedule, Schedule>();
        }
    }
}
