// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataImport.Models
{
    public class BootstrapDataApiServer
    {
        public int BootstrapDataId { get; set; }

        [ForeignKey("BootstrapDataId")]
        public BootstrapData BootstrapData { get; set; }

        public int ApiServerId { get; set; }

        public ApiServer ApiServer { get; set; }

        public DateTimeOffset ProcessedDate { get; set; }
    }
}
