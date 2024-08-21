// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using DataImport.Models;
using RestSharp;
using static DataImport.Common.Encryption;

namespace DataImport.Common.Helpers
{
    public interface IOAuthRequestWrapper : IAuthRequestWrapper
    {
        string GetBearerToken(ApiServer apiServer, string encryptionKey, string accessCode);

        string GetBearerToken(ApiServer apiServer, string encryptionKey);
    }

    public class OAuthRequestWrapper : AuthRequestWrapper, IOAuthRequestWrapper
    {
        public string GetBearerToken(ApiServer apiServer, string encryptionKey)
        {
            return GetBearerToken(apiServer, encryptionKey, null);
        }

        public string GetBearerToken(ApiServer apiServer, string encryptionKey, string accessCode)
        {
            var tokenUrl = new Uri(apiServer.TokenUrl);
            RestClientOptions options = GetOptions(tokenUrl);

            var oauthClient = new RestClient(options);

            var bearerTokenRequest = new RestRequest(tokenUrl.AbsolutePath, Method.Post);

            var apiServerKey = !string.IsNullOrEmpty(encryptionKey)
                ? Decrypt(apiServer.Key, encryptionKey)
                : apiServer.Key;
            var apiServerSecret = !string.IsNullOrEmpty(encryptionKey)
                ? Decrypt(apiServer.Secret, encryptionKey)
                : apiServer.Secret;

            bearerTokenRequest.AddParameter("client_id", apiServerKey);
            bearerTokenRequest.AddParameter("client_secret", apiServerSecret);
            if (accessCode != null)
            {
                bearerTokenRequest.AddParameter("code", accessCode);
                bearerTokenRequest.AddParameter("grant_type", "authorization_code");
            }
            else
            {
                bearerTokenRequest.AddParameter("grant_type", "client_credentials");
            }

            return GetToken(bearerTokenRequest, oauthClient);
        }
    }
}
