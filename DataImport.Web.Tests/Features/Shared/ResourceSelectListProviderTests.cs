// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Linq;
using DataImport.Models;
using DataImport.TestHelpers;
using DataImport.Web.Features.Shared.SelectListProviders;
using NUnit.Framework;
using Shouldly;
using static DataImport.Web.Tests.Testing;

namespace DataImport.Web.Tests.Features.Shared
{
    class ResourceSelectListProviderTests
    {
        [Test]
        public void ShouldHaveOptionPerKnownResource()
        {
            var apiVersionId = GetDefaultApiVersion().Id;

            var expectedValues = KnownResourcePaths(apiVersionId);

            With<ResourceSelectListProvider>(x =>
            {
                var actual = x.GetResources(apiVersionId);

                var first = actual.First();
                first.Text.ShouldBe("Select Resource");
                first.Value.ShouldBe("");
                first.Group.ShouldBe(null);

                var rest = actual.Skip(1).ToArray();

                rest.Select(item => item.Text).ShouldMatch(DisplayValues(expectedValues));
                rest.Select(item => item.Value).ShouldMatch(expectedValues);
                rest.All(item => item.Group != null).ShouldBe(true);
            });
        }

        private static string[] KnownResourcePaths(int apiVersionId)
        {
            return Query(database =>
                database.Resources
                    .Where(x => x.ApiVersionId == apiVersionId)
                    .ToList()
                    .OrderBy(x => x.ApiSection)
                    .ThenBy(x => x.ToResourceName())
                    .Select(x => x.Path)
                    .ToArray());
        }

        private static string[] DisplayValues(string[] resourcePaths)
        {
            return resourcePaths.Select(x => x.ToResourceName()).ToArray();
        }
    }
}
