// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataImport.Models
{
    public class File : Entity
    {
        [Required]
        public string FileName { get; set; }
        public string Url { get; set; }
        public int AgentId { get; set; }

        [Required]
        public FileStatus Status { get; set; }
        public string Message { get; set; }
        public int? Rows { get; set; }
        public DateTimeOffset? CreateDate { get; set; }
        public DateTimeOffset? UpdateDate { get; set; }

        [ForeignKey("AgentId")]
        public Agent Agent { get; set; }
    }
}
