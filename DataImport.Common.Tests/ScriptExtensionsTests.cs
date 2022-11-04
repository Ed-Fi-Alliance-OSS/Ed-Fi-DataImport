// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Preprocessors;
using DataImport.Models;
using NUnit.Framework;
using Shouldly;
using System;

namespace DataImport.Common.Tests
{
    [TestFixture]
    public class ScriptExtensionsTests
    {
        [Test]
        public void ShouldRunPowerShellWithNoRestrictions()
        {
            try
            {
                ConfigureScriptExtensionsGlobal();
                UpdateUsePowerShellWithNoRestrictionsValueOnAppConfig(true);

                foreach (ScriptType scriptType in Enum.GetValues(typeof(ScriptType)))
                {
                    new Script
                    {
                        ScriptType = scriptType
                    }.ShouldRunPowerShellWithNoRestrictions().ShouldBeTrue();
                }

                UpdateUsePowerShellWithNoRestrictionsValueOnAppConfig(false);

                foreach (ScriptType scriptType in Enum.GetValues(typeof(ScriptType)))
                {
                    new Script
                    {
                        ScriptType = scriptType
                    }.ShouldRunPowerShellWithNoRestrictions().ShouldBeFalse();
                }
            }
            finally
            {
                UpdateUsePowerShellWithNoRestrictionsValueOnAppConfig(false);
            }
        }

        private class FakeAppSettings : IPowerShellPreprocessSettings
        {
            public string EncryptionKey { get; set; }
            public bool UsePowerShellWithNoRestrictions { get; set; }
        }

        private IPowerShellPreprocessSettings _powerShellPreprocessSettings;

        private void ConfigureScriptExtensionsGlobal()
        {
            _powerShellPreprocessSettings = new FakeAppSettings { UsePowerShellWithNoRestrictions = false };
            ScriptExtensions.SetAppSettingsOptions(_powerShellPreprocessSettings);
        }

        private void UpdateUsePowerShellWithNoRestrictionsValueOnAppConfig(bool value)
        {
            _powerShellPreprocessSettings.UsePowerShellWithNoRestrictions = value;
        }
    }
}
