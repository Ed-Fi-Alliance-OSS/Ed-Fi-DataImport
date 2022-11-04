// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using System;

namespace DataImport.Common.Preprocessors
{
    public static class ScriptExtensions
    {
        private static IPowerShellPreprocessSettings _powerShellPreprocessSettings;

        public static void SetAppSettingsOptions(IPowerShellPreprocessSettings powerShellPreprocessSettings)
        {
            if (_powerShellPreprocessSettings == default(IPowerShellPreprocessSettings))
                _powerShellPreprocessSettings = powerShellPreprocessSettings;
            else
            {
                // this is to make sure that the global state isn't changed while running.
                throw new NotSupportedException("AppSettingsOptions may not be set more than once.");
            }
        }

        public static bool ShouldRunPowerShellWithNoRestrictions(this Script script)
        {
            if (script == null) throw new ArgumentNullException(nameof(script));
            return _powerShellPreprocessSettings.UsePowerShellWithNoRestrictions;
        }
    }
}
