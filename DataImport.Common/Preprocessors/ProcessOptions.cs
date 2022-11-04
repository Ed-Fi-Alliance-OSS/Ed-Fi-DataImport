// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Management.Automation;

namespace DataImport.Common.Preprocessors
{
    public class ProcessOptions
    {
        public bool RequiresOdsConnection { get; set; }
        public ApiServer OdsConnectionSettings { get; set; }
        public bool IsDataMapPreview { get; set; }
        public string CacheIdentifier { get; set; }
        public string MapAttribute { get; set; }
        public string FileName { get; set; }
        public bool UsePowerShellWithNoRestrictions { get; set; }

        public event EventHandler<ProcessMessageEventArgs> ProcessMessageLogged;

        internal void OnProcessStreamDataAdding(object sender, DataAddingEventArgs e)
        {
            if (ProcessMessageLogged == null)
            {
                return;
            }

            var level = GetLogLevel(e);
            var message = e.ItemAdded.ToString();

            ProcessMessageLogged(this, new ProcessMessageEventArgs
            {
                Level = level,
                Message = message
            });
        }

        private static LogLevel GetLogLevel(DataAddingEventArgs e)
        {
            switch (e.ItemAdded)
            {
                case VerboseRecord _:
                    return LogLevel.Trace;
                case DebugRecord _:
                    return LogLevel.Debug;
                case InformationRecord _:
                    return LogLevel.Information;
                case WarningRecord _:
                    return LogLevel.Warning;
                case ErrorRecord _:
                    return LogLevel.Error;
            }

            throw new InvalidOperationException($"Attempting to log an unexpected message type '{e.ItemAdded.GetType()}'.");
        }
    }
}
