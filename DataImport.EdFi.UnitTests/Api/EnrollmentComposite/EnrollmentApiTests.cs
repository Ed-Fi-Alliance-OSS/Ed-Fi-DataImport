// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using DataImport.EdFi.Api.EnrollmentComposite;
using FakeItEasy;
using NUnit.Framework;
using RestSharp;
using Shouldly;

namespace DataImport.EdFi.UnitTests.Api.EnrollmentComposite
{
    [TestFixture]
    public class EnrollmentApiTests
    {

        // Test-specific Subclass (TSS) to facilitate testing of protected members
        public class EnrollmentApiTss : EnrollmentApi
        {
            public new IRestClient Client => base.Client;

            public EnrollmentApiTss(IRestClient client, string apiVersion, string year) : base(client, apiVersion, year, null)
            {
            }
        }

        [Test]
        public void Given_suite2_version260_then_should_not_reset_client_url()
        {
            // Arrange
            const string apiVersion = "2.6.0";
            const string year = "2199";

            const string initialUrl = "https://example.com";
            var restClient = A.Fake<IRestClient>();
            restClient.BaseUrl = new Uri(initialUrl);

            // Act
            var _ = new EnrollmentApiTss(restClient, apiVersion, year);

            // Assert
            restClient.BaseUrl.ShouldNotBeNull();
            restClient.BaseUrl.ToString().ShouldBe($"{initialUrl}/");
        }

        [TestCase( "3.1.1", "2129", "/composites/v1/2129/")]
        [TestCase( "3.1.1", null, "/composites/v1/")]
        [TestCase( "5.1.0", "2129", "/composites/v1/2129/")]
        [TestCase( "5.1.0", null, "/composites/v1/")]
        public void Given_suite_3_then_should_modify_the_url(string apiVersion, string year, string expectedCompositePath)
        {
            // Arrange
            const string initialUrl = "https://example.com/v3/data/";
            const string expectedUrl = "https://example.com/v3";

            var restClient = A.Fake<IRestClient>();
            restClient.BaseUrl = new Uri(initialUrl);

            // Act
            var _ = new EnrollmentApiTss(restClient, apiVersion, year);

            // Assert
            restClient.BaseUrl.ShouldNotBeNull();
            restClient.BaseUrl.ToString().ShouldBe(expectedUrl);
        }
    }
}
