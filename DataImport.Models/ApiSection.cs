// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace DataImport.Models
{
    /// <summary>
    /// Identifies which section of the API an ODS resource belongs to.
    ///
    /// This corresponds with Swagger metadata route subsections,
    /// and the "API Section" option as seen in the Swagger UI.
    /// </summary>
    public enum ApiSection
    {
        Resources = 1,
        Descriptors = 2
    }

    public static class ApiSectionExtensions
    {
        public static string ToDisplayName(this ApiSection apiSection)
            => apiSection.ToString();

        public static string ToMetadataRoutePart(this ApiSection apiSection)
            => apiSection.ToString().ToLower();
    }
}
