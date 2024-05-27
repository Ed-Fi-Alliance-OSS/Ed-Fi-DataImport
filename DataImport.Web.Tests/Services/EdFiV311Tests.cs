// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Threading.Tasks;
using AutoMapper;
using DataImport.Common.Helpers;
using DataImport.Models;
using DataImport.Web.Services;
using DataImport.Web.Services.Swagger;
using FakeItEasy;
using NUnit.Framework;
using Shouldly;

namespace DataImport.Web.Tests.Services
{
    [TestFixture]
    public class EdFiServiceV311Tests
    {
        // Test-specific Subclass (TSS) to facilitate testing of protected members
        public class EdFiServiceV311Tss : EdFiServiceV311
        {
            private static IEncryptionKeyResolver GetEncryptionKeyResolver()
            {
                var encryptionKeyResolver = A.Fake<IEncryptionKeyResolver>();
                A.CallTo(() => encryptionKeyResolver.GetEncryptionKey()).Returns("lskdjflskdjf");
                return encryptionKeyResolver;
            }

            public EdFiServiceV311Tss(ISwaggerMetadataFetcher metadataFetcher)
                : base(
                   null,
                   GetEncryptionKeyResolver(),
                   A.Fake<IMapper>(),
                   metadataFetcher,
                   A.Fake<IOAuthRequestWrapper>(),
                   null)
            {
            }

            public async Task<string> GetYearSpecificYearForTesting(string apiVersion)
            {
                return await GetYearSpecificYear(new ApiServer { Url = "https://www.ed-fi.org" }, new ApiVersion { Version = apiVersion });
            }
        }

        public abstract class TestFixture
        {
            protected const string Year = "2020";
            protected EdFiServiceV311Tss SystemUnderTest;

            [SetUp]
            public void SetUp()
            {
                var swaggerMetadataFetcher = A.Fake<ISwaggerMetadataFetcher>();
                A.CallTo(() => swaggerMetadataFetcher.GetYearSpecificYear(A<string>._))
                    .Returns(Year);

                SystemUnderTest = new EdFiServiceV311Tss(swaggerMetadataFetcher);
            }
        }

        [TestFixture]
        public class CanHandle : TestFixture
        {
            [Test]
            public void Given_suite2_version260_then_false()
            {
                SystemUnderTest.CanHandle("2.6.0").ShouldBe(false);
            }

            [Test]
            public void Given_suite3_version311_then_true()
            {
                SystemUnderTest.CanHandle("3.1.1").ShouldBe(true);
            }

            [Test]
            public void Given_suite3_version510_then_true()
            {
                SystemUnderTest.CanHandle("5.1.0").ShouldBe(true);
            }
        }

        [TestFixture]
        public class YearSpecificYear : TestFixture
        {
            [Test]
            public async Task Given_suite2_version260_then_return_null()
            {
                (await SystemUnderTest.GetYearSpecificYearForTesting("2.6.0")).ShouldBe(null);
            }

            [Test]
            public async Task Given_suite3_version311_then_get_year_from_url()
            {
                (await SystemUnderTest.GetYearSpecificYearForTesting("3.1.1")).ShouldBe(Year);
            }

            [Test]
            public async Task Given_suite3_version510_then_get_year_from_url()
            {
                (await SystemUnderTest.GetYearSpecificYearForTesting("5.1.1")).ShouldBe(Year);
            }
        }
    }
}
