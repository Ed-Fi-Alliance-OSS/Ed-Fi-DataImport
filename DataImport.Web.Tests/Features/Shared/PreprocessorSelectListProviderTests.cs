// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DataImport.Models;
using DataImport.TestHelpers;
using DataImport.Web.Features.Shared.SelectListProviders;
using NUnit.Framework;
using Shouldly;
using static DataImport.Web.Tests.Testing;

namespace DataImport.Web.Tests.Features.Shared
{
    [TestFixture]
    internal class PreprocessorSelectListProviderTests
    {
        [Test]
        public async Task PreprocessorSelectListProviderTest()
        {
            var preprocessor1 = await AddPreprocessor(ScriptType.CustomFileProcessor, true);
            var preprocessor2 = await AddPreprocessor(ScriptType.CustomFileProcessor);
            var preprocessor3 = await AddPreprocessor(ScriptType.ExternalFileProcessor);
            var excluded1 = await AddPreprocessor(ScriptType.CustomRowProcessor);
            var excluded2 = await AddPreprocessor(ScriptType.ExternalFileGenerator);

            Query(d =>
            {
                var scriptTypeSelectListProvider = new PreprocessorSelectListProvider(d);

                var preprocessors = scriptTypeSelectListProvider.GetCustomFileProcessors();

                var item1 = preprocessors.Single(x => x.Value == preprocessor1.Id.ToString(CultureInfo.InvariantCulture));
                var item2 = preprocessors.Single(x => x.Value == preprocessor2.Id.ToString(CultureInfo.InvariantCulture));
                var item3 = preprocessors.Single(x => x.Value == preprocessor3.Id.ToString(CultureInfo.InvariantCulture));

                item1.ShouldMatch(new PreprocessorDropDownItem
                {
                    Value = item1.Value,
                    Text = preprocessor1.Name,
                    RequiresApiConnection = false,
                    RequiresAttribute = true,
                    Group = item1.Group,
                    Disabled = item1.Disabled,
                    Selected = item1.Selected
                });

                item2.ShouldMatch(new PreprocessorDropDownItem
                {
                    Value = item2.Value,
                    Text = preprocessor2.Name,
                    RequiresApiConnection = false,
                    RequiresAttribute = false,
                    Group = item1.Group,
                    Disabled = item1.Disabled,
                    Selected = item1.Selected
                });

                item3.ShouldMatch(new PreprocessorDropDownItem
                {
                    Value = item3.Value,
                    Text = preprocessor3.Name,
                    RequiresApiConnection = false,
                    RequiresAttribute = false,
                    Group = item3.Group,
                    Disabled = item3.Disabled,
                    Selected = item3.Selected
                });

                preprocessors.Any(x => x.Value == excluded1.Id.ToString(CultureInfo.InvariantCulture)).ShouldBeFalse();
                preprocessors.Any(x => x.Value == excluded2.Id.ToString(CultureInfo.InvariantCulture)).ShouldBeFalse();

                return 0;
            });
        }
    }
}
