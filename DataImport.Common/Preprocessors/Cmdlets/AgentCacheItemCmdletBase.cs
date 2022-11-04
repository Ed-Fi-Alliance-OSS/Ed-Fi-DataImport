// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace DataImport.Common.Preprocessors.Cmdlets
{
    public abstract class AgentCacheItemCmdletBase : PSCmdlet
    {
        public static readonly string CacheIdentifierVariableName = "CacheIdentifier";

        protected Dictionary<string, object> ResolveCache()
        {
            var cacheIdentifier = GetVariableValue(CacheIdentifierVariableName);
            if (cacheIdentifier == null)
            {
                throw new InvalidOperationException("AgentCacheIdentifier is not set. Make sure you provider CacheIdentifier to powershell preprocessor.");
            }

            return DataImportCacheManager.ResolveCache((string) cacheIdentifier);
        }
    }
}
