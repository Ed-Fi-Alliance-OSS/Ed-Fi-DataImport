// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.ExtensionMethods;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DataImport.Server.TransformLoad.Features.LoadResources
{
    public interface IOdsApi
    {
        Task<OdsResponse> PostBootstrapData(string endpointUrl, string dataToInsert);
        Task<OdsResponse> Post(string content, string endpointUrl, string postInfo = null);
        ApiConfig Config { get; set; }
    }

    public class OdsApi : IOdsApi
    {
        public ApiConfig Config { get; set; }

        //HttpClient instances are meant to be long-lived and shared, so we only
        //have separate instances when they would be configured differently.
        private static readonly HttpClient UnauthenticatedHttpClient = new();
        private readonly ILogger _logger;

        private Lazy<HttpClient> AuthenticatedHttpClient { get; set; }

        private string AccessToken { get; set; }

        public OdsApi(ILogger logger, ApiConfig config)
        {
            _logger = logger;
            Config = config;
            AuthenticatedHttpClient = new Lazy<HttpClient>(CreateAuthenticatedHttpClient);
        }

        private HttpClient CreateAuthenticatedHttpClient()
        {
            if (AccessToken == null)
                throw new Exception("An attempt was made to make authenticated HTTP requests without an Access Token.");

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            return httpClient;
        }

        private async Task Authenticate()
        {
            if (AccessToken == null)
            {
                if (Config.ApiVersion.IsOdsV2())
                {
                    var authorizationCode = await GetAuthorizationCode(Config.AuthorizeUrl, Config.ClientId);

                    AccessToken = await GetAccessToken(Config.AccessTokenUrl, Config.ClientId, Config.ClientSecret, authorizationCode);
                }
                else
                {
                    AccessToken = await GetAccessToken(Config.AccessTokenUrl, Config.ClientId, Config.ClientSecret);
                }
            }
        }

        private async Task<string> GetAuthorizationCode(string authorizeUrl, string clientId)
        {
            var contentParams = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Client_id", clientId),
                new KeyValuePair<string, string>("Response_type", "code")
            });

            _logger.LogInformation("Retrieving auth code from {url}", authorizeUrl);

            var response = await UnauthenticatedHttpClient.PostAsync(authorizeUrl, contentParams);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("Failed to get Authorization Code. HTTP Status Code: " + response.StatusCode);

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonToken = JToken.Parse(jsonResponse);
            return jsonToken["code"].ToString();
        }

        private static async Task<string> GetAccessToken(string accessTokenUrl, string clientId, string clientSecret, string authorizationCode = null)
        {
            FormUrlEncodedContent contentParams;

            if (authorizationCode != null)
            {
                contentParams = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Client_id", clientId),
                    new KeyValuePair<string, string>("Client_secret", clientSecret),
                    new KeyValuePair<string, string>("Code", authorizationCode),
                    new KeyValuePair<string, string>("Grant_type", "authorization_code")
                });
            }
            else
            {
                contentParams = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Grant_type", "client_credentials")
                });

                var encodedKeySecret = Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");
                UnauthenticatedHttpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(encodedKeySecret));
            }

            var response = await UnauthenticatedHttpClient.PostAsync(accessTokenUrl, contentParams);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("Failed to get Access Token. HTTP Status Code: " + response.StatusCode);

            var jsonResult = await response.Content.ReadAsStringAsync();
            var jsonToken = JToken.Parse(jsonResult);
            return jsonToken["access_token"].ToString();
        }

        public async Task<OdsResponse> PostBootstrapData(string endpointUrl, string dataToInsert)
        {
            await Authenticate();

            var strContent = new StringContent(dataToInsert);
            strContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await AuthenticatedHttpClient.Value.PostAsync(endpointUrl, strContent);

            var responseContent = await response.Content.ReadAsStringAsync();
            return new OdsResponse(response.StatusCode, responseContent);
        }

        public async Task<OdsResponse> Post(string content, string endpointUrl, string postInfo)
        {
            await Authenticate();

            const int retryAttempts = 3;
            var currentAttempt = 0;
            HttpResponseMessage response = null;

            while (retryAttempts > currentAttempt)
            {
                var strContent = new StringContent(content);
                strContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                response = await AuthenticatedHttpClient.Value.PostAsync(endpointUrl, strContent);
                currentAttempt++;

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    AccessToken = null;
                    await Authenticate();
                    AuthenticatedHttpClient = new Lazy<HttpClient>(CreateAuthenticatedHttpClient);
                    _logger.LogWarning("POST failed. Reason: {reason}. StatusCode: {status}.", response.ReasonPhrase, response.StatusCode);
                    _logger.LogInformation("Refreshing token and retrying POST request for {info}.", postInfo);
                }
                else
                    break;
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            return new OdsResponse(response.StatusCode, responseContent);
        }
    }
}
