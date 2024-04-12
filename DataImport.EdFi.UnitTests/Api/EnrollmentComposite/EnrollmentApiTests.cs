// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using DataImport.Common.ExtensionMethods;
using DataImport.EdFi.Api.EnrollmentComposite;
using DataImport.Models;
using FakeItEasy;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
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
            const string ApiVersion = "2.6.0";
            const string Year = "2199";

            const string InitialUrl = "https://example.com";
            RestClientOptions options = new RestClientOptions();
            options.BaseUrl = new Uri(InitialUrl);
            var restClient = new RestClient(options);

            // Act
            var enrollmentApi = new EnrollmentApiTss(restClient, ApiVersion, Year);

            // Assert
            enrollmentApi.Client.Options.BaseUrl.ShouldNotBeNull();
            enrollmentApi.Client.Options.BaseUrl.ToString().ShouldBe($"{InitialUrl}/");
        }

        [TestCase("3.1.1", "2129", "/composites/v1/2129/")]
        [TestCase("3.1.1", null, "/composites/v1/")]
        [TestCase("5.1.0", "2129", "/composites/v1/2129/")]
        [TestCase("5.1.0", null, "/composites/v1/")]
        public void Given_suite_3_then_should_modify_the_url(string apiVersion, string year, string expectedCompositePath)
        {
            // Arrange
            const string InitialUrl = "https://example.com/v3/data/";
            const string ExpectedUrl = "https://example.com/v3";
            RestClientOptions options = new RestClientOptions();
            options.BaseUrl = new Uri(InitialUrl);
            var restClient = new RestClient(options);

            // Act
            var enrollmentApi = new EnrollmentApiTss(restClient, apiVersion, year);

            // Assert
            enrollmentApi.Client.Options.BaseUrl.ShouldNotBeNull();
            enrollmentApi.Client.Options.BaseUrl.ToString().ShouldBe(ExpectedUrl);
        }

        [Test]
        public void Given_suite2_version260_then_should_have_the_authenticator()
        {
            // Arrange
            const string ApiVersion = "2.6.0";
            const string Year = "2199";
            const string InitialUrl = "https://example.com/v3/data/";
            RestClientOptions options = new RestClientOptions();
            options.BaseUrl = new Uri(InitialUrl);
            options.Authenticator = A.Fake<IAuthenticator>();
            var restClient = new RestClient(options);

            // Act
            var enrollmentApi = new EnrollmentApiTss(restClient, ApiVersion, Year);

            // Assert
            enrollmentApi.Client.Options.Authenticator.ShouldNotBeNull();
        }

        [TestCase("3.1.1", "2129")]
        [TestCase("3.1.1", null)]
        [TestCase("5.1.0", "2129")]
        [TestCase("5.1.0", null)]
        public void Given_suite_3_then_should_have_the_authenticator(string apiVersion, string year)
        {
            const string InitialUrl = "https://example.com/v3/data/";
            RestClientOptions options = new RestClientOptions();
            options.BaseUrl = new Uri(InitialUrl);
            options.Authenticator = A.Fake<IAuthenticator>();
            var restClient = new RestClient(options);

            // Act
            var enrollmentApi = new EnrollmentApiTss(restClient, apiVersion, year);

            // Assert
            enrollmentApi.Client.Options.Authenticator.ShouldNotBeNull();
        }
    }
}
