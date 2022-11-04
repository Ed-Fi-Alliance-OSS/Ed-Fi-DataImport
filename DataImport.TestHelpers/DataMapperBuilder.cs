// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Linq;
using DataImport.Models;

namespace DataImport.TestHelpers
{
    public static class DataMapperBuilder
    {
        public static DataMapper MapArray(string name, params DataMapper[] items)
        {
            return new DataMapper
            {
                Name = name,
                SourceColumn = null,
                SourceTable = null,
                Value = null,
                Default = null,
                Children = items.ToList()
            };
        }

        public static DataMapper MapObject(string name, params DataMapper[] properties)
        {
            return new DataMapper
            {
                Name = name,
                SourceColumn = null,
                SourceTable = null,
                Value = null,
                Default = null,
                Children = properties.ToList()
            };
        }

        public static DataMapper MapColumn(string name, string sourceColumn, string @default = null)
        {
            return new DataMapper
            {
                Name = name,
                SourceColumn = sourceColumn,
                SourceTable = null,
                Value = null,
                Default = @default
            };
        }

        public static DataMapper Unmapped(string name)
        {
            return new DataMapper
            {
                Name = name,
                SourceColumn = null,
                SourceTable = null,
                Value = null,
                Default = null
            };
        }

        public static DataMapper MapLookup(string name, string sourceColumn, string sourceTable, string @default = null)
        {
            return new DataMapper
            {
                Name = name,
                SourceColumn = sourceColumn,
                SourceTable = sourceTable,
                Value = null,
                Default = @default
            };
        }

        public static DataMapper MapStatic(string name, string value)
        {
            return new DataMapper
            {
                Name = name,
                SourceColumn = null,
                SourceTable = null,
                Value = value,
                Default = null
            };
        }
    }
}
