// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace DataImport.Web.Features.Lookup
{
    public static class Mapper
    {
        public static LookupIndex.LookupItem ToLookupItem(this DataImport.Models.Lookup src) =>
            new()
            {
                Id = src.Id,
                SourceTable = src.SourceTable,
                Key = src.Key,
                Value = src.Value
            };

        public static EditLookup.Command ToEditCommand(this DataImport.Models.Lookup src) =>
            new()
            {
                Id = src.Id,
                SourceTable = src.SourceTable,
                Key = src.Key,
                Value = src.Value
            };
    }
}
