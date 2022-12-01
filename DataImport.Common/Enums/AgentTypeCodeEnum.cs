// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataImport.Common.Enums
{
    public static class AgentTypeCodeEnum
    {
        public const string Sftp = "SFTP";
        public const string Ftps = "FTPS";
        public const string Manual = "Manual";

        [Display(Name = "File System / PowerShell")]
        public const string PowerShell = "PowerShell";

        public static List<string> ToList() => new List<string>() { Manual, Sftp, Ftps, PowerShell };

        public static int DefaultPort(string agentType)
        {
            if (agentType == Sftp)
                return 22;

            if (agentType == Ftps)
                return 990;

            throw new Exception($"There is no default port number for '{agentType}' Agents.");
        }
    }
}
