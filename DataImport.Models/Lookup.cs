// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.ComponentModel.DataAnnotations;

namespace DataImport.Models
{
    public class Lookup : Entity
    {
        [Required(AllowEmptyStrings = false)]
        [MaxLength(1024)]
        public string SourceTable { get; set; }
        [Required(AllowEmptyStrings = false)]
        [MaxLength(1024)]
        public string Key { get; set; }
        [Required(AllowEmptyStrings = false)]
        [MaxLength(1024)]
        public string Value { get; set; }
    }
}
