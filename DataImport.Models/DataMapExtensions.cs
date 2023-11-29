// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace DataImport.Models
{
    public static class DataMapExtensions
    {
        public static string[] ReferencedLookups(this DataMap dataMap)
        {
            return Mappings(dataMap).ReferencedLookups();
        }

        public static string[] ReferencedColumns(this DataMap dataMap)
        {
            return Mappings(dataMap).ReferencedColumns();
        }

        public static string[] ReferencedLookups(this DataMapper[] mappings)
        {
            return DistinctValues(mappings.Yield(x => x.SourceTable));
        }

        public static string[] ReferencedColumns(this DataMapper[] mappings)
        {
            return DistinctValues(mappings.Yield(x => x.SourceColumn));
        }

        private static DataMapper[] Mappings(DataMap dataMap)
        {
            var dataMapSerializer = new DataMapSerializer(dataMap);
            return dataMapSerializer.Deserialize(dataMap.Map, false);
        }

        private static IEnumerable<string> Yield(this IEnumerable<DataMapper> mappings, Func<DataMapper, string> propertyAccessor)
        {
            foreach (var mapping in mappings)
            {
                var propertyValue = propertyAccessor(mapping);

                if (propertyValue != null)
                    yield return propertyValue;

                foreach (var lookupTable in Yield(mapping.Children, propertyAccessor))
                    yield return lookupTable;
            }
        }

        private static string[] DistinctValues(IEnumerable<string> items)
        {
            return items.Distinct().OrderBy(x => x).ToArray();
        }
    }
}
