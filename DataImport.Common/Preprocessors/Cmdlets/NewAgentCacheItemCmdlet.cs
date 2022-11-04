// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Management.Automation;

namespace DataImport.Common.Preprocessors.Cmdlets
{
    [Cmdlet(VerbsCommon.New, "AgentCacheItem")]
    public class NewAgentCacheItemCmdlet : AgentCacheItemCmdletBase
    {
        [Parameter(Mandatory = true)]
        public string Key { get; set; }

        [Parameter(Mandatory = true)]
        public object Value { get; set; }

        protected override void BeginProcessing()
        {
            var cache = ResolveCache();
            if (cache.ContainsKey(Key))
            {
                cache[Key] = Value;
            }
            else
            {
                cache.Add(Key, Value);
            }
        }
    }
}
