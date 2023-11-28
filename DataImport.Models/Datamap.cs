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
    public class DataMap : Entity
    {
        public DataMap()
        {
            DataMapAgents = new HashSet<DataMapAgent>();
        }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255)]
        public string ResourcePath { get; set; }

        [Required]
        public string Metadata { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Map { get; set; }

        public DateTimeOffset? CreateDate { get; set; }
        public DateTimeOffset? UpdateDate { get; set; }

        public string ColumnHeaders { get; set; }

        public int ApiVersionId { get; set; }

        public ApiVersion ApiVersion { get; set; }

        public int? FileProcessorScriptId { get; set; }

        public string Attribute { get; set; }

        [ForeignKey("FileProcessorScriptId")]
        public Script FileProcessorScript { get; set; }

        public ICollection<DataMapAgent> DataMapAgents { get; set; }

        public bool IsDeleteOperation { get; set; }
    }
}
