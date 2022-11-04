// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Linq;
using System.Threading.Tasks;
using DataImport.Models;
using DataImport.TestHelpers;
using DataImport.Web.Features.Agent;
using Microsoft.AspNetCore.Mvc.Rendering;
using NUnit.Framework;
using Shouldly;
using static DataImport.Web.Tests.Testing;

namespace DataImport.Web.Tests.Features.Agent
{
    public class AgentSelectListProviderTests
    {
        [Test]
        public void ShouldGetSelectableAgentTypes()
        {
            With<AgentSelectListProvider>(provider =>
            {
                provider
                    .GetAgentTypes()
                    .ShouldMatch(
                        new SelectListItem
                        {
                            Text = "Select Type",
                            Value = ""
                        },
                        new SelectListItem
                        {
                            Text = "Manual",
                            Value = "Manual"
                        },
                        new SelectListItem
                        {
                            Text = "SFTP",
                            Value = "SFTP"
                        },
                        new SelectListItem
                        {
                            Text = "FTPS",
                            Value = "FTPS"
                        },
                        new SelectListItem
                        {
                            Text = "File System / PowerShell",
                            Value = "PowerShell"
                        });
            });
        }

        [Test]
        public async Task ShouldGetBothKindsOfFileGenerators()
        {
            var psFileGenerator = await AddPreprocessor(ScriptType.CustomFileGenerator);
            var exFileGenerator = await AddPreprocessor(ScriptType.ExternalFileGenerator);

            With<AgentSelectListProvider>(provider =>
            {
                var generators = provider.GetFileGenerators().ToList();
                generators.Any(x => x.Text == psFileGenerator.Name && x.Value == psFileGenerator.Id.ToString())
                    .ShouldBeTrue();
                generators.Any(x => x.Text == exFileGenerator.Name && x.Value == exFileGenerator.Id.ToString())
                    .ShouldBeTrue();
            });
        }
    }
}
