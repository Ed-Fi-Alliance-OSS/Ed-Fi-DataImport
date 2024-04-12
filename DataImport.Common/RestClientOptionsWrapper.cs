// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using RestSharp;
using RestSharp.Authenticators;

namespace DataImport.EdFi.UnitTests.Api
{
    public class RestClientOptionsWrapper : IRestClientOptionsWrapper
    {
        private readonly RestClientOptions _options;
        public RestClientOptionsWrapper(RestClientOptions options)
        {
            _options = options;
        }

        public IAuthenticator Authenticator
        {
            get => _options.Authenticator;
            set => _options.Authenticator = value;
        }

        public Uri BaseUrl
        {
            get => _options.BaseUrl;
            set => _options.BaseUrl = value;
        }
    }
}
