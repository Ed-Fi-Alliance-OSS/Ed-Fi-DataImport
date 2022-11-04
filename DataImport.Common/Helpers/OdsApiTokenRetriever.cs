// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using DataImport.Common.ExtensionMethods;
using DataImport.Models;

namespace DataImport.Common.Helpers
{
    public class OdsApiTokenRetriever : ITokenRetriever
    {
        private readonly ApiServer _apiServer;
        private readonly string _encryptionKey;
        private readonly IOAuthRequestWrapper _requestWrapper;

        public OdsApiTokenRetriever(IOAuthRequestWrapper requestWrapper, ApiServer apiServer, string encryptionKey = "")
        {
            _requestWrapper = requestWrapper ?? throw new ArgumentNullException(nameof(requestWrapper));

            _apiServer = apiServer;
            _encryptionKey = encryptionKey;
        }

        public string ObtainNewBearerToken()
        {
            if (_apiServer.ApiVersion.Version.IsOdsV2())
            {
                var accessCode = _requestWrapper.GetAccessCode(_apiServer, _encryptionKey);

                return _requestWrapper.GetBearerToken(_apiServer, _encryptionKey, accessCode);
            }

            return _requestWrapper.GetBearerToken(_apiServer, _encryptionKey);
        }
    }

    internal class AccessCodeResponse
    {
        public string Code { get; set; }
        public string State { get; set; }
        public string Error { get; set; }
    }

    internal class BearerTokenResponse
    {
        public string AccessToken { get; set; }
        public string ExpiresIn { get; set; }
        public string TokenType { get; set; }
        public string Error { get; set; }
    }
}
