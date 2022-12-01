// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.Web.Features.DataMaps;
using DataImport.Web.Features.Shared.SelectListProviders;
using DataImport.Web.Services;
using NUnit.Framework;
using Shouldly;
using System.Collections.Generic;

namespace DataImport.Web.Tests.Features.DataMaps
{
    [TestFixture]
    public class QueryHandlerTests
    {
        // Test-specific Subclass (TSS) to facilitate testing of protected members
        public class QueryHandlerTss : RetrieveDescriptors.QueryHandler
        {
            public QueryHandlerTss(EdFiServiceManager edFiServiceManager,
                DataImportDbContext dbContext,
                ResourceSelectListProvider resourceProvider
                )
                : base(edFiServiceManager, dbContext, resourceProvider)
            {
            }

            public new string GetDescriptorPath(string descriptorName, string apiVersion) => base.GetDescriptorPath(descriptorName, apiVersion);
        }

        public abstract class TestFixture
        {
            public QueryHandlerTss SystemUnderTest { get; set; }

            [SetUp]
            public void SetUp()
            {
                SystemUnderTest = new QueryHandlerTss(new EdFiServiceManager(new List<EdFiServiceBase>(), null), null, new ResourceSelectListProvider(null));
            }
        }

        [TestFixture]
        public class GetDescriptorPath
        {
            [TestFixture]
            public class GivenSuite2Version260 : TestFixture
            {
                protected string InjectedApiVersion => "2.6.0";

                [TestCase("AbcDefs", "AbcDefs", Description = "Input is output")]
                [TestCase("Abc/Defs", "Abc/Defs", Description = "Ignores slash in the middle")]
                [TestCase("/AbcDefs/", "AbcDefs", Description = "Strips off leading and trailing slash")]
                [TestCase("AbcDef", "AbcDefs", Description = "Pluralizes descriptor")]
                [TestCase("AbcDef/", "AbcDefs", Description = "Both strips off trailing slash and pluralizes descriptor")]
                public void When_getting_descriptor_path(string descriptorName, string expected)
                {
                    SystemUnderTest.GetDescriptorPath(descriptorName, InjectedApiVersion).ShouldBe(expected);
                }
            }

            [TestFixture]
            public class GivenSuite3Version311 : TestFixture
            {
                protected string InjectedApiVersion => "3.1.1";

                [TestCase("a/AbcDef", "a/AbcDefs", Description = "ed-fi/ not prefix if input contains non-closing slash")]
                [TestCase("AbcDefs", "ed-fi/AbcDefs", Description = "Input is prefixed with ed-fi/")]
                [TestCase("/AbcDefs/", "ed-fi/AbcDefs", Description = "Strips off leading and trailing slash")]
                [TestCase("AbcDef", "ed-fi/AbcDefs", Description = "Pluralizes descriptor")]
                [TestCase("AbcDef/", "ed-fi/AbcDefs", Description = "Both strips off trailing slash and pluralizes descriptor")]
                public void When_getting_descriptor_path(string descriptorName, string expected)
                {
                    SystemUnderTest.GetDescriptorPath(descriptorName, InjectedApiVersion).ShouldBe(expected);
                }
            }

            [TestFixture]
            public class GivenSuite3Version510 : TestFixture
            {
                protected string InjectedApiVersion => "5.1.0";

                [TestCase("a/AbcDef", "a/AbcDefs", Description = "ed-fi/ not prefix if input contains non-closing slash")]
                [TestCase("AbcDefs", "ed-fi/AbcDefs", Description = "Input is prefixed with ed-fi/")]
                [TestCase("/AbcDefs/", "ed-fi/AbcDefs", Description = "Strips off leading and trailing slash")]
                [TestCase("AbcDef", "ed-fi/AbcDefs", Description = "Pluralizes descriptor")]
                [TestCase("AbcDef/", "ed-fi/AbcDefs", Description = "Both strips off trailing slash and pluralizes descriptor")]
                public void When_getting_descriptor_path(string descriptorName, string expected)
                {
                    SystemUnderTest.GetDescriptorPath(descriptorName, InjectedApiVersion).ShouldBe(expected);
                }
            }
        }
    }
}
