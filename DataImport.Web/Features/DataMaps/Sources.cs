// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Linq;
using System.Reflection;

namespace DataImport.Web.Features.DataMaps
{
    public static class Sources
    {
        public const string Column = "column";
        public const string LookupTable = "lookup-table";
        public const string Static = "static";

        public static string[] GetAll()
        {
            return typeof(Sources)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(i => i.IsLiteral && !i.IsInitOnly && i.FieldType == typeof(string))
                .Select(i => (string)i.GetRawConstantValue())
                .ToArray();
        }
    }
}
