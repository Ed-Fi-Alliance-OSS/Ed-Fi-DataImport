// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace DataImport.Web.Tests
{
    public class StubHostEnvironment : IHostEnvironment
    {
        public string ApplicationName { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; } = HostEnvironmentEnum.Development;
    }

    public static class HostEnvironmentEnum
    {
        public const string Production = "Production";
        public const string Development = "Development";
    }
}