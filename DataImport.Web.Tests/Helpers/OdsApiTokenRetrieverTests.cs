// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Helpers;
using DataImport.Models;
using FakeItEasy;
using NUnit.Framework;
using Shouldly;
using System;

namespace DataImport.Web.Tests.Helpers
{
    [TestFixture]
    public class OdsApiTokenRetrieverTests
    {
        [TestFixture]
        public class Constructor
        {
            [Test]
            public void Given_happy_path_should_not_throw_exception()
            {
                Action act = () =>
                {
                    var _ = new OdsApiTokenRetriever(A.Fake<IOAuthRequestWrapper>(), new ApiServer
                    {
                        ApiVersion = new ApiVersion
                        {
                            Version = "3.1.1."
                        }
                    }, "random");
                };

                act.ShouldNotThrow();
            }

            [Test]
            public void Given_null_OAuthRequestWrapper_should_throw_exception()
            {
                Action act = () =>
                {
                    var _ = new OdsApiTokenRetriever(null, new ApiServer
                    {
                        ApiVersion = new ApiVersion
                        {
                            Version = "3.1.1."
                        }
                    }, "random");
                };

                act.ShouldThrow<ArgumentNullException>();
            }
        }

        [TestFixture]
        public class ObtainNewBearerToken
        {
            [SetUp]
            public void SetUp()
            {
                _oAuthRequestWrapper = A.Fake<IOAuthRequestWrapper>();
                _apiServer = new ApiServer
                {
                    ApiVersion = new ApiVersion()
                };

                _systemUnderTest = new OdsApiTokenRetriever(_oAuthRequestWrapper, _apiServer, EncryptionKey);
            }

            private OdsApiTokenRetriever _systemUnderTest;

            private IOAuthRequestWrapper _oAuthRequestWrapper;

            private ApiServer _apiServer;

            private const string EncryptionKey = "random value";

            [Test]
            public void Given_setup_for_suite2_version260_then_use_access_code_to_get_bearer_token()
            {
                // Arrange
                const string accessCode = "access code";
                const string bearerToken = "bearer token";

                A.CallTo(() => _oAuthRequestWrapper.GetAccessCode(_apiServer, EncryptionKey))
                    .Returns(accessCode);

                A.CallTo(() =>
                        _oAuthRequestWrapper.GetBearerToken(_apiServer, EncryptionKey, accessCode))
                    .Returns(bearerToken);

                _apiServer.ApiVersion.Version = "2.6.0";

                // Act
                var actual = _systemUnderTest.ObtainNewBearerToken();

                // Assert
                actual.ShouldBe(bearerToken);
            }

            [Test]
            public void Given_setup_for_suite3_version311_then_do_not_need_access_code_for_bearer_token_request()
            {
                // Arrange
                const string bearerToken = "bearer token";

                A.CallTo(() => _oAuthRequestWrapper.GetBearerToken(_apiServer, EncryptionKey))
                    .Returns(bearerToken);

                _apiServer.ApiVersion.Version = "3.1.1";

                // Act
                var actual = _systemUnderTest.ObtainNewBearerToken();

                // Assert
                actual.ShouldBe(bearerToken);
            }

            [Test]
            public void Given_setup_for_suite3_version510_then_do_not_need_access_code_for_bearer_token_request()
            {
                // Arrange
                const string bearerToken = "bearer token";

                A.CallTo(() => _oAuthRequestWrapper.GetBearerToken(_apiServer, EncryptionKey))
                    .Returns(bearerToken);

                _apiServer.ApiVersion.Version = "5.1.0";

                // Act
                var actual = _systemUnderTest.ObtainNewBearerToken();

                // Assert
                actual.ShouldBe(bearerToken);
            }
        }
    }
}
