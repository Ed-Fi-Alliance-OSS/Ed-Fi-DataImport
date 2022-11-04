// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataImport.Models
{
    public class Script : Entity
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(50)]
        public ScriptType ScriptType { get; set; }

        public string ScriptContent { get; set; }

        public bool RequireOdsApiAccess { get; set; }

        public ICollection<DataMap> DataMaps { get; set; }

        public bool HasAttribute { get; set; }

        public string ExecutablePath { get; set; }

        public string ExecutableArguments { get; set; }
    }
}
