// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace DataImport.TestHelpers
{
    public static class TestHelpers
    {
        public static string Json(object value)
        {
            return JsonConvert.SerializeObject(value, Formatting.Indented);
        }

        public static string ReadTestFile(string fileName)
        {
            var filePath = Path.Combine(GetAssemblyPath(), fileName);
            return File.ReadAllText(filePath);
        }

        public static string GetAssemblyPath()
        {
            var location = Assembly.GetExecutingAssembly().Location;

            if(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Path.GetDirectoryName(location);

            var uri = new UriBuilder(location);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}
