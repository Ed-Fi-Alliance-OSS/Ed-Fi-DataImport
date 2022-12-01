// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Linq;

namespace DataImport.Models
{
    public class LookupCollection
    {
        private readonly Dictionary<string, Dictionary<string, string>> _lookups;

        public LookupCollection(Lookup[] lookups)
            => _lookups = ToDictionaries(lookups);

        public bool TryLookup(string sourceTable, string key, out string value)
        {
            if (_lookups.ContainsKey(sourceTable))
            {
                var inner = _lookups[sourceTable];
                if (inner.ContainsKey(key))
                {
                    value = inner[key];
                    return true;
                }
            }

            value = null;
            return false;
        }

        private static Dictionary<string, Dictionary<string, string>> ToDictionaries(Lookup[] lookups)
        {
            var outer = lookups
                .Select(x => x.SourceTable.Trim())
                .Distinct()
                .ToDictionary(sourceTable => sourceTable, sourceTable => new Dictionary<string, string>());

            foreach (var lookup in lookups)
                outer[lookup.SourceTable.Trim()].Add(lookup.Key.Trim(), lookup.Value.Trim());

            return outer;
        }

        public IEnumerable<string> SourceTables() => _lookups.Keys;
    }
}
