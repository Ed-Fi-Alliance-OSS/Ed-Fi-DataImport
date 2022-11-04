// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Reflection;
using System.Text;

namespace DataImport.Common
{
    public static class Version
    {
        public static string ProductVersion
        {
            get
            {
                var productVersion = new StringBuilder();
                int dots = 0;

                foreach (var ch in InternalVersion ?? "")
                {
                    if (ch == '.')
                        dots++;

                    if (dots >= 2)
                        break;

                    productVersion.Append(ch);
                }

                return productVersion.ToString();
            }
        }

        public static string InternalVersion =>
            Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    }
}
