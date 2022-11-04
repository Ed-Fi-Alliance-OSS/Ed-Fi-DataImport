// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace DataImport.Common.Logging
{
    public static class DefaultLogger
    {
        public static Logger Build()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss,fff}] [{Level:u4}] [{SourceContext}] {Message} {Exception} {NewLine}")
                .CreateLogger();
        }
    }
}
