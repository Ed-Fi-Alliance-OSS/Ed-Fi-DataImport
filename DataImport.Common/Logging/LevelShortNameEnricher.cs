// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using Serilog.Core;
using Serilog.Events;

namespace DataImport.Common.Logging
{
    public class LevelShortNameEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var value = logEvent.Level switch
            {
                LogEventLevel.Verbose => "ALL",
                LogEventLevel.Debug => "DEBUG",
                LogEventLevel.Information => "INFO",
                LogEventLevel.Warning => "WARN",
                LogEventLevel.Error => "ERROR",
                LogEventLevel.Fatal => "FATAL",
                // This gets flagged by SonarSource with code smell S3928
                // https://community.sonarsource.com/t/false-positive-on-c-parameter-names-used-into-argumentexception/17415/2
                // Basically, the complains that "Level" is not a named argument in our signature, therefore `ArgumentOutOfRangeException`
                // is not an appropriate exception. That is a legitimate point. To be technically accurate, we should change this,
                // although in concept it makes sense how we got here.
                // _ => throw new ArgumentOutOfRangeException(nameof(logEvent.Level), logEvent.Level, "Unexpected value for LogEventLevel")
                _ => throw new ArgumentException("Unexpected value for LogEvent.Level", nameof(logEvent))
            };

            logEvent.AddProperty(LoggingConstants.LevelShort, value);
        }
    }
}
