// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;

namespace DataImport.Web.Helpers
{
    public static class ExtensionMethods
    {
        public static string ToDescriptorName(this string descriptor)
        {
            if (string.IsNullOrEmpty(descriptor))
            {
                return string.Empty;
            }

            var index = descriptor.IndexOf("#", StringComparison.Ordinal);
            return index > 0
                ? descriptor.Substring(index + 1)
                : descriptor;
        }
    }
}
