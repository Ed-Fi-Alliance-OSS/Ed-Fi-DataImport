// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataImport.Models
{
    public class ApiServer : Entity
    {
        [Required]
        [MaxLength(255)]
        public string Url { get; set; }

        [Required]
        public string Key { get; set; }

        [Required]
        public string Secret { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255)]
        public string TokenUrl { get; set; }

        [MaxLength(255)]
        public string AuthUrl { get; set; }

        public int ApiVersionId { get; set; }

        public ApiVersion ApiVersion { get; set; }

        public ICollection<BootstrapDataApiServer> BootstrapDataApiServers { get; set; }
    }
}
