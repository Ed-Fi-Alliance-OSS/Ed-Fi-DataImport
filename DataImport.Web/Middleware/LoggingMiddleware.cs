// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.Web.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var loggerState = new Dictionary<string, object>();
            var body = await GetRequestBody(context);
            if (null != body)
            {
                loggerState.Add("requestBody", body);
            }

            using (_logger.BeginScope(loggerState))
            {
                await _next(context);
            }
        }

        private static async Task<string> GetRequestBody(HttpContext context)
        {
            if (context?.Request?.Body == null)
                return null;

            var request = context.Request;
            request.EnableBuffering();

            //keep the middleware from eating the request:
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer.AsMemory(0, buffer.Length));
            var requestContent = Encoding.UTF8.GetString(buffer);

            //Reset stream
            request.Body.Position = 0;  

            return requestContent;
        }
    }
}