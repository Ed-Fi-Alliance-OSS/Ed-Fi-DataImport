// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using DataImport.Common.Logging;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace DataImport.Web.Infrastructure
{
    public class RequestInfoEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestInfoEnricher()
        {
            _httpContextAccessor = new HttpContextAccessor();
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

            var httpContext = _httpContextAccessor.HttpContext;

            const string notAvailable = "NOT AVAILABLE";

            var serverName = httpContext?.Request.Host.Host ?? notAvailable;
            var serverPort = httpContext?.Request.Host.Port?.ToString() ?? notAvailable;
            var url = httpContext?.Request.Path.Value ?? notAvailable;
            var localAddress = httpContext?.Connection.LocalIpAddress?.ToString() ?? notAvailable;
            var remoteAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? notAvailable;
            var userName = httpContext?.User.Identity?.Name;

            logEvent.AddProperty(LoggingConstants.ServerNameProp, serverName);
            logEvent.AddProperty(LoggingConstants.ServerPortProp, serverPort);
            logEvent.AddProperty(LoggingConstants.UrlProp, url);
            logEvent.AddProperty(LoggingConstants.LocalAddress, localAddress);
            logEvent.AddProperty(LoggingConstants.RemoteAddress, remoteAddress);
            logEvent.AddProperty(LoggingConstants.UserName, userName);
        }
    }
}
