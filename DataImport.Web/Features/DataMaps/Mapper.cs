// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;

namespace DataImport.Web.Features.DataMaps
{
    public static class Mapper
    {
        public static DataMapIndex.ViewModel ToViewModel(this DataMap src) =>
            new()
            {
                Id = src.Id,
                Name = src.Name,
                ResourceName = src.ToResourceName()
            };
    }
}
