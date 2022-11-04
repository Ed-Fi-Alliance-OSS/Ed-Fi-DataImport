// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Serilog.Events;

namespace DataImport.Common.Logging
{
    public static class LogEventExtensions
    {
        public static void AddProperty(this LogEvent logEvent, string propertyName, object value)
        {
            var property = new LogEventProperty(propertyName, new ScalarValue(value));
            logEvent.AddPropertyIfAbsent(property);
        }
    }
}
