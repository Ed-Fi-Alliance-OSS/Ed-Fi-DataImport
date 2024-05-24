// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataImport.Models
{
    public class IngestionLog
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [MaxLength(255)]
        public string Level { get; set; }

        [MaxLength(255)]
        public string Operation { get; set; }

        [MaxLength(255)]
        public string AgentName { get; set; }

        public string Process { get; set; }

        public string FileName { get; set; }

        public IngestionResult Result { get; set; }

        public DateTimeOffset Date { get; set; }

        public string RowNumber { get; set; }

        public string EndPointUrl { get; set; }

        public string HttpStatusCode { get; set; }

        public string Data { get; set; }

        public string OdsResponse { get; set; }

        [MaxLength(20)]
        public string ApiVersion { get; set; }

        [MaxLength(255)]
        public string ApiServerName { get; set; }
    }
}
