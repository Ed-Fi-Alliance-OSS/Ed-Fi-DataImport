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
    public interface IOAuthRequestWrapper
    {
        string GetAccessCode(ApiServer apiServer, string encryptionKey);

        string GetBearerToken(ApiServer apiServer, string encryptionKey, string accessCode);

        string GetBearerToken(ApiServer apiServer, string encryptionKey);
    }

    public class OAuthRequestWrapper : IOAuthRequestWrapper
    {
        public string GetAccessCode(ApiServer apiServer, string encryptionKey)
        {
            var authUrl = new Uri(apiServer.AuthUrl);
            var oauthClient = new RestClient(authUrl.GetLeftPart(UriPartial.Authority));

            var accessCodeRequest = new RestRequest(authUrl.AbsolutePath, Method.POST);
            var apiServerKey = !string.IsNullOrEmpty(encryptionKey)
                ? Decrypt(apiServer.Key, encryptionKey)
                : apiServer.Key;
            accessCodeRequest.AddParameter("Client_id", apiServerKey);
            accessCodeRequest.AddParameter("Response_type", "code");

            var accessCodeResponse = oauthClient.Execute<AccessCodeResponse>(accessCodeRequest);

            if (accessCodeResponse.StatusCode != HttpStatusCode.OK)
                throw new AuthenticationException("Unable to retrieve an authorization code. Error message: " +
                                                  accessCodeResponse.ErrorMessage);
            if (accessCodeResponse.Data.Error != null)
                throw new AuthenticationException(
                    "Unable to retrieve an authorization code. Please verify that your application key is correct. Alternately, the service address may not be correct: " +
                    authUrl);

            return accessCodeResponse.Data.Code;
        }

        public string GetBearerToken(ApiServer apiServer, string encryptionKey)
        {
            return GetBearerToken(apiServer, encryptionKey, null);
        }

        public string GetBearerToken(ApiServer apiServer, string encryptionKey, string accessCode)
        {
            var tokenUrl = new Uri(apiServer.TokenUrl);
            var oauthClient = new RestClient(tokenUrl.GetLeftPart(UriPartial.Authority));

            var bearerTokenRequest = new RestRequest(tokenUrl.AbsolutePath, Method.POST);

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


            var bearerTokenResponse = oauthClient.Execute<BearerTokenResponse>(bearerTokenRequest);

            if (bearerTokenResponse.StatusCode != HttpStatusCode.OK)
                throw new AuthenticationException("Unable to retrieve an access token. Error message: " +
                                                  bearerTokenResponse.ErrorMessage);

            if (bearerTokenResponse.Data.Error != null || bearerTokenResponse.Data.TokenType != "bearer")
                throw new AuthenticationException(
                    "Unable to retrieve an access token. Please verify that your application secret is correct.");

            return bearerTokenResponse.Data.AccessToken;
        }
    }
}
