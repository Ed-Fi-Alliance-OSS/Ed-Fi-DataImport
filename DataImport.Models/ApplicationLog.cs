// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataImport.Models
{
    public class ApplicationLog
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [MaxLength(200)]
        public string MachineName { get; set; }
        [Required]
        public DateTimeOffset Logged { get; set; }
        [MaxLength(5), Required]
        public string Level { get; set; }
        [MaxLength(200)]
        public string UserName { get; set; }
        public string Message { get; set; }
        [MaxLength(300)]
        public string Logger { get; set; }
        public string Properties { get; set; }
        [MaxLength(200)]
        public string ServerName { get; set; }
        [MaxLength(100)]
        public string Port { get; set; }
        [MaxLength(2000)]
        public string Url { get; set; }
        [MaxLength(100)]
        public string ServerAddress { get; set; }
        public string RemoteAddress { get; set; }
        public string Exception { get; set; }
    }
}