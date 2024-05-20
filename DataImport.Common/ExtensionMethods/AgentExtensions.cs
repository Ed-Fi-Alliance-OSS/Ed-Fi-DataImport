// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using System;

namespace DataImport.Common.ExtensionMethods
{
    public static class AgentExtensions
    {
        public static AgentActionsFile GetActionFileCode(this Agent value)
        {
            if (value.ActionFileCode is null)
                return AgentActionsFile.DeleteOnSuccessful;
            return ((AgentActionsFile) Enum.Parse(typeof(AgentActionsFile), value.ActionFileCode));
        }
    }
}
