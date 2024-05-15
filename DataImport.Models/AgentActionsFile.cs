// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.ComponentModel;
namespace DataImport.Models
{
    public enum AgentActionsFile
    {
        [Description("Always delete")]
        AlwaysDelete = 0,
        [Description("Delete on successful")]
        DeleteOnSuccessful = 1,
        [Description("Never delete")]
        NeverDelete = 2
    }
}
