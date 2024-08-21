// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Text;
using DataImport.Models;
using RestSharp;
using static DataImport.Common.Encryption;

namespace DataImport.Common.Helpers
{
    public interface IBasicAuthRequestWrapper : IAuthRequestWrapper
    {
        string GetBasicToken(ApiServer apiServer, string encryptionKey, string accessCode);
    }

    public class BasicAuthRequestWrapper : AuthRequestWrapper, IBasicAuthRequestWrapper
    {
        public string GetBasicToken(ApiServer apiServer, string encryptionKey, string accessCode)
        {
            var tokenUrl = new Uri(apiServer.TokenUrl);
            RestClientOptions options = GetOptions(tokenUrl);

            var authClient = new RestClient(options);

            var tokenRequest = new RestRequest(tokenUrl.AbsolutePath, Method.Post);

            var apiServerKey = !string.IsNullOrEmpty(encryptionKey)
                ? Decrypt(apiServer.Key, encryptionKey)
                : apiServer.Key;
            var apiServerSecret = !string.IsNullOrEmpty(encryptionKey)
                ? Decrypt(apiServer.Secret, encryptionKey)
                : apiServer.Secret;

            var keySecretBytes = Encoding.UTF8.GetBytes($"{apiServerKey}:{apiServerSecret}");
            tokenRequest.AddHeader("Authorization", $"Basic {Convert.ToBase64String(keySecretBytes)}");

            if (accessCode != null)
            {
                tokenRequest.AddParameter("code", accessCode);
                tokenRequest.AddParameter("grant_type", "authorization_code");
            }
            else
            {
                tokenRequest.AddParameter("grant_type", "client_credentials");
            }

            return GetToken(tokenRequest, authClient);
        }
    }
}
