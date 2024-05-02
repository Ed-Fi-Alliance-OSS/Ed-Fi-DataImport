// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace DataImport.Common.Helpers
{
    public static class ScriptExtensions
    {
        private static IIgnoresCertificateErrorsSettings _ignoresCertificateErrorsSettings;

        public static void SetAppSettingsOptions(IIgnoresCertificateErrorsSettings ignoresCertificateErrorsSettings)
        {
            _ignoresCertificateErrorsSettings = ignoresCertificateErrorsSettings;
        }

        public static bool IgnoresCertificateErrors()
        {
            return _ignoresCertificateErrorsSettings.IgnoresCertificateErrors;
        }
    }
}
