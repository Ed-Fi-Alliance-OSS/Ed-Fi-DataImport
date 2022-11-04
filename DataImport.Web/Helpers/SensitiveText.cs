// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace DataImport.Web.Helpers
{
    public static class SensitiveText
    {
        public static readonly string MaskedPlaceholder = "*********";

        public static string Mask(string value)
        {
            return MaskedPlaceholder;
        }

        public static bool IsMasked(string value)
        {
            return value == MaskedPlaceholder;
        }
    }
}