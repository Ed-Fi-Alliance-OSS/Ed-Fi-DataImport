// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using DataImport.Common.Helpers;
using DataImport.Models;

namespace DataImport.Common.Preprocessors
{
    public class OdsAuthenticator
    {
        private readonly ITokenRetriever _tokenRetriever;
        private readonly ApiServer _apiServer;

        public OdsAuthenticator(ITokenRetriever tokenRetriever, ApiServer apiServer)
        {
            _tokenRetriever = tokenRetriever;
            _apiServer = apiServer;
        }

        public OdsAuthenticationResult Authenticate()
        {
            return new OdsAuthenticationResult(GetBaseUrl(), _tokenRetriever.ObtainNewBearerToken());
        }

        private Uri GetBaseUrl()
        {
            return new Uri(_apiServer.Url);
        }
    }
}
