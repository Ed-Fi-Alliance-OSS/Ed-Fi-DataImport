// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataImport.Models
{
    public class Agent : Entity
    {
        public Agent()
        {
            DataMapAgents = new HashSet<DataMapAgent>();
            AgentSchedules = new HashSet<AgentSchedule>();
            Files = new HashSet<File>();
            BootstrapDataAgents = new HashSet<BootstrapDataAgent>();
        }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Agent Type")]
        [MaxLength(50)]
        public string AgentTypeCode { get; set; }

        [Display(Name = "Agent Action")]
        [MaxLength(50)]
        public string AgentAction { get; set; }

        public string Url { get; set; }

        public int? Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Directory { get; set; }

        [Display(Name = "File Pattern")]
        public string FilePattern { get; set; }

        public bool Enabled { get; set; }

        public bool Archived { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset? LastExecuted { get; set; }

        public int? RowProcessorScriptId { get; set; }

        public int? FileGeneratorScriptId { get; set; }

        public int? ApiServerId { get; set; }

        public int? RunOrder { get; set; }

        [ForeignKey("ApiServerId")]
        public ApiServer ApiServer { get; set; }
        public ICollection<DataMapAgent> DataMapAgents { get; set; }
        public ICollection<AgentSchedule> AgentSchedules { get; set; }
        public ICollection<File> Files { get; set; }
        public ICollection<BootstrapDataAgent> BootstrapDataAgents { get; set; }

        [ForeignKey("RowProcessorScriptId")]
        public Script RowProcessor { get; set; }

        [ForeignKey("FileGeneratorScriptId")]
        public Script FileGenerator { get; set; }

        [Display(Name = "Action Files")]
        [MaxLength(50)]
        public string ActionFileCode { get; set; }
    }
}
