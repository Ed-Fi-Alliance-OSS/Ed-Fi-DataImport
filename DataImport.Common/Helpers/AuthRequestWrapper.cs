// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Net;
using System.Security.Authentication;
using DataImport.Models;
using RestSharp;
using static DataImport.Common.Encryption;

namespace DataImport.Common.Helpers
{
    public abstract class AuthRequestWrapper
    {
        public virtual RestClientOptions GetOptions(Uri tokenUrl)
        {
            RestClientOptions options;

            if (ScriptExtensions.IgnoresCertificateErrors())
            {
#pragma warning disable S4830
                options = new RestClientOptions(tokenUrl.GetLeftPart(UriPartial.Authority))
                {
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
                };
#pragma warning restore S4830
            }
            else
            {
                options = new RestClientOptions(tokenUrl.GetLeftPart(UriPartial.Authority));
            }

            return options;
        }

        public virtual string GetAccessCode(ApiServer apiServer, string encryptionKey)
        {
            var authUrl = new Uri(apiServer.AuthUrl);
            var authClient = new RestClient(authUrl.GetLeftPart(UriPartial.Authority));

            var accessCodeRequest = new RestRequest(authUrl.AbsolutePath, Method.Post);
            var apiServerKey = !string.IsNullOrEmpty(encryptionKey)
                ? Decrypt(apiServer.Key, encryptionKey)
                : apiServer.Key;
            accessCodeRequest.AddParameter("Client_id", apiServerKey);
            accessCodeRequest.AddParameter("Response_type", "code");

            var accessCodeResponse = authClient.Execute<AccessCodeResponse>(accessCodeRequest);

            if (accessCodeResponse.StatusCode != HttpStatusCode.OK)
                throw new AuthenticationException("Unable to retrieve an authorization code. Error message: " +
                                                  accessCodeResponse.ErrorMessage);
            if (accessCodeResponse.Data.Error != null)
                throw new AuthenticationException(
                    "Unable to retrieve an authorization code. Please verify that your application key is correct. Alternately, the service address may not be correct: " +
                    authUrl);

            return accessCodeResponse.Data.Code;
        }

        public virtual string GetToken(RestRequest tokenRequest, RestClient oauthClient)
        {
            var tokenResponse = oauthClient.Execute<BearerTokenResponse>(tokenRequest);
            if (tokenResponse.StatusCode != HttpStatusCode.OK)
                throw new AuthenticationException("Unable to retrieve an access token. Error message: " +
                                                  tokenResponse.ErrorMessage);

            if (tokenResponse.Data.Error != null || tokenResponse.Data.TokenType != "bearer")
                throw new AuthenticationException(
                    "Unable to retrieve an access token. Please verify that your application secret is correct.");

            return tokenResponse.Data.AccessToken;
        }
    }
}
