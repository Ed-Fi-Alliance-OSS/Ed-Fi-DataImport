// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Management.Automation;

namespace DataImport.Common.Preprocessors.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "AgentCacheItem")]
    public class GetAgentCacheItemCmdlet : AgentCacheItemCmdletBase
    {
        [Parameter(Mandatory = true)]
        public string Key { get; set; }

        protected override void BeginProcessing()
        {
            var cache = ResolveCache();

            if (cache.TryGetValue(Key, out var cacheValue))
            {
                WriteObject(cacheValue);
                return;
            }

            WriteObject(null);
        }
    }
}
