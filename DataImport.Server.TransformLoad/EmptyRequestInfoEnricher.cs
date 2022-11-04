// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using DataImport.Common.Logging;
using Serilog.Core;
using Serilog.Events;

namespace DataImport.Server.TransformLoad
{
    public class EmptyRequestInfoEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

            const string notAvailable = "NOT AVAILABLE";

            logEvent.AddProperty(LoggingConstants.ServerNameProp, notAvailable);
            logEvent.AddProperty(LoggingConstants.ServerPortProp, notAvailable);
            logEvent.AddProperty(LoggingConstants.UrlProp, notAvailable);
            logEvent.AddProperty(LoggingConstants.LocalAddress, notAvailable);
            logEvent.AddProperty(LoggingConstants.RemoteAddress, notAvailable);
            logEvent.AddProperty(LoggingConstants.UserName, null);
        }
    }
}
