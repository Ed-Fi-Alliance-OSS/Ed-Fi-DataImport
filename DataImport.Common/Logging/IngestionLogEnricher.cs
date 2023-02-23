// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using Serilog.Core;
using Serilog.Events;

namespace DataImport.Common.Logging
{
    public class IngestionLogEnricher : ILogEventEnricher
    {
        private const string TableName = "IngestionLog";
        private const string LogTypeProperty = "LogType";
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent.Properties.ContainsKey(TableName))
            {
                logEvent.AddProperty(LogTypeProperty, TableName);
                LogEventPropertyValue value;
                if (logEvent.Properties.TryGetValue(TableName, out value) &&
                    value is StructureValue sv)
                {
                    foreach (var item in sv.Properties)
                    {
                        if (item.Value is ScalarValue scalar && scalar.Value != null)
                        {
                            if (scalar.Value is IngestionResult rawValue)
                            {
                                logEvent.AddProperty(item.Name, (int) rawValue);
                            }
                            else if (scalar.Value is string stringValue)
                            {
                                logEvent.AddProperty(item.Name, stringValue);
                            }
                            else
                            {
                                logEvent.AddProperty(item.Name, scalar.Value);
                            }
                        }
                    }
                }
            }
        }
    }
}
