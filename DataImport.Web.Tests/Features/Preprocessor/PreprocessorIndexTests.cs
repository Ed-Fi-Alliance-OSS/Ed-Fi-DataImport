// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.TestHelpers;
using DataImport.Web.Features.Agent;
using DataImport.Web.Features.Preprocessor;
using Microsoft.AspNetCore.Mvc.Rendering;
using NUnit.Framework;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static DataImport.Web.Tests.Testing;

namespace DataImport.Web.Tests.Features.Preprocessor
{
    public class PreprocessorIndexTests
    {
        [Test]
        public async Task ShouldDisplayAllPreprocessors()
        {
            var preprocessor = await AddPreprocessor(ScriptType.CustomRowProcessor);

            var preprocessors = await Send(new PreprocessorIndex.Query());

            CollectionAssert.IsNotEmpty(preprocessors.Preprocessors);

            preprocessors.Preprocessors.Single(x => x.Id == preprocessor.Id)
                .ShouldMatch(new PreprocessorIndex.PreprocessorIndexModel
                {
                    Id = preprocessor.Id,
                    Name = preprocessor.Name,
                    ScriptType = preprocessor.ScriptType,
                    UsedBy = new List<SelectListItem>()
                });
        }

        [Test]
        public async Task ShouldDisplayUsedByForUsedPreprocessors()
        {
            var fileProcessor = await AddPreprocessor(ScriptType.CustomFileProcessor);
            var resource = RandomResource();
            var columnHeaders = new[] { "Csv Column A" };
            var dataMap = await AddDataMap(resource, columnHeaders, null, fileProcessor.Id);
            var rowProcessor = await AddPreprocessor(ScriptType.CustomRowProcessor);
            var viewModel = new AddEditAgentViewModel
            {
                AgentTypeCode = "Manual",
                Name = "Test Agent",
                Enabled = true,
                RowProcessorId = rowProcessor.Id,
                ApiServerId = GetDefaultApiServer().Id
            };
            var agentId = (await Send(new AddAgent.Command { ViewModel = viewModel })).AgentId;

            var preprocessors = await Send(new PreprocessorIndex.Query());

            var preprocessorViewModel = preprocessors.Preprocessors.Single(x => x.Id == fileProcessor.Id);
            preprocessorViewModel.UsedBy.ShouldMatch(new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = dataMap.Name,
                    Value = dataMap.Id.ToString(CultureInfo.InvariantCulture),
                    Group = new SelectListGroup
                    {
                        Name = "Data Maps"
                    }
                }
            });

            preprocessorViewModel = preprocessors.Preprocessors.Single(x => x.Id == rowProcessor.Id);
            preprocessorViewModel.UsedBy.ShouldMatch(new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = viewModel.Name,
                    Value = agentId.ToString(CultureInfo.InvariantCulture),
                    Group = new SelectListGroup
                    {
                        Name = "Agents"
                    }
                }
            });
        }
    }
}
