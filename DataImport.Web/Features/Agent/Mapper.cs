// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Linq;
using DataImport.Models;

namespace DataImport.Web.Features.Agent
{
    public static class Mapper
    {
        public static MappedAgent ToMappedAgent(this DataMapAgent src) =>
            new()
            {
                DataMapId = src.DataMapId,
                ProcessingOrder = src.ProcessingOrder,
                DataMapName = src.DataMap?.Name
            };

        public static AgentBootstrapData ToAgentBootstrapData(this BootstrapDataAgent src) =>
            new()
            {
                BootstrapDataId = src.BootstrapDataId,
                ProcessingOrder = src.ProcessingOrder,
                BootstrapName = src.BootstrapData?.Name,
                Resource = src.BootstrapData?.ResourcePath
            };

        public static Schedule ToSchedule(this AgentSchedule src) =>
            new()
            {
                Id = src.Id,
                Day = src.Day,
                Hour = src.Hour,
                Minute = src.Minute
            };

        public static AddEditAgentViewModel ToViewModel(this DataImport.Models.Agent src) =>
            new()
            {
                Id = src.Id,
                Name = src.Name,
                AgentTypeCode = src.AgentTypeCode,
                Url = src.Url,
                Port = src.Port,
                Username = src.Username,
                // Password intentionally omitted — decryption handled by the handler
                Directory = src.Directory,
                FilePattern = src.FilePattern,
                Enabled = src.Enabled,
                ApiServerId = src.ApiServerId,
                RunOrder = src.RunOrder,
                ActionFileCode = src.ActionFileCode,
                RowProcessorId = src.RowProcessorScriptId,
                FileGeneratorId = src.FileGeneratorScriptId,
                MappedAgents = src.DataMapAgents?.Select(x => x.ToMappedAgent()).ToList() ?? new(),
                AgentBootstrapDatas = src.BootstrapDataAgents?.Select(x => x.ToAgentBootstrapData()).ToList() ?? new(),
                AgentSchedules = src.AgentSchedules?.Select(x => x.ToSchedule()).ToList() ?? new()
                // RowProcessors, FileGenerators, DataMaps, AgentTypes, ActionFiles,
                // EncryptionFailureMsg, ApiServers, BootstrapDatas, DdlDataMaps,
                // DdlSchedules, DdlBootstrapDatas — all filled by handler after this call
            };
    }
}
