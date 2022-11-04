// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.ComponentModel.DataAnnotations.Schema;

namespace DataImport.Models
{
    public class DataMapAgent : Entity
    {
        public int DataMapId { get; set; }
        public int AgentId { get; set; }
        public int ProcessingOrder { get; set; }

        [ForeignKey("AgentId")]
        public Agent Agent { get; set; }
        [ForeignKey("DataMapId")]
        public DataMap DataMap { get; set; }
    }
}
