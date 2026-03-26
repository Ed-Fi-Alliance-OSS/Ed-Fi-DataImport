// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace DataImport.Web.Features.ApiServers
{
    public static class Mapper
    {
        public static ApiServerIndex.ApiServerModel ToApiServerModel(this DataImport.Models.ApiServer src) =>
            new()
            {
                Id = src.Id,
                Name = src.Name,
                Url = src.Url,
                // ApiVersion can be null for legacy records; preserve null (avoid a new exception and match prior mapping behavior).
                ApiVersion = src.ApiVersion?.Version
            };
    }
}
