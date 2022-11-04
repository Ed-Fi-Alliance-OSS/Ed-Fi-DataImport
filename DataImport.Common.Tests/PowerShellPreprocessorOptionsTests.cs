// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using DataImport.Common.Preprocessors;
using NUnit.Framework;
using Shouldly;

namespace DataImport.Common.Tests
{
    [TestFixture, Category("PowerShellTests")]
    public class PowerShellPreprocessorOptionsTests
    {
        [Test]
        public void ShouldHaveDefaultCommands()
        {
            new PowerShellPreprocessorOptions().AllowedCommands.ShouldNotBeEmpty();
        }

        [Test]
        public void ShouldBeAbleToMergeCommandsAndModules()
        {
            var options = new PowerShellPreprocessorOptions
            {
                AllowedCommands = null,
                Modules = null
            };
            // Start with empty list and merge some options
            options.MergeOptions("Add-Command", "C:\\SomeModule.psm1");
            options.AllowedCommands.ShouldBe(new List<string>
            {
                "Add-Command"
            });
            options.Modules.ShouldBe(new List<string>
            {
                "C:\\SomeModule.psm1"
            });

            // Merge again with different case. make sure duplicates are not created
            options.MergeOptions("add-command", "C:\\somemodule.psm1");
            options.AllowedCommands.ShouldBe(new List<string>
            {
                "Add-Command"
            });
            options.Modules.ShouldBe(new List<string>
            {
                "C:\\SomeModule.psm1"
            });

            // Merge again with separator
            options.MergeOptions($"Add-Command1{PowerShellPreprocessorOptions.CmdletsSeparator}add-command1", $"C:\\somemodule1.psm1{PowerShellPreprocessorOptions.ModulesSeparator}C:\\somemodule2.psm1");
            options.AllowedCommands.ShouldBe(new List<string>
            {
                "Add-Command",
                "Add-Command1"
            });
            options.Modules.ShouldBe(new List<string>
            {
                "C:\\SomeModule.psm1",
                "C:\\somemodule1.psm1",
                "C:\\somemodule2.psm1"
            });
        }
    }
}
