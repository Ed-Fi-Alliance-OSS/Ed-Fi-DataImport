// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace DataImport.Common.Logging
{
    public static class LoggerExtensions
    {
        private const string LogType = "ApplicationLog";
        public static void LogToTable(this ILogger logger, string message, object model, string logType = LogType)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>
            {
                { $"@{logType}", model }
            };
            using (logger.BeginScope(dictionary))
            {
                logger.LogInformation(message);
            }
        }
    }

}
