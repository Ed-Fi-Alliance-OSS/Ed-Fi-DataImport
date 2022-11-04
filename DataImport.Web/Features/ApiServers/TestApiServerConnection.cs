// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Helpers;
using DataImport.EdFi;
using DataImport.EdFi.Api.Resources;
using DataImport.Models;
using DataImport.Web.Helpers;
using DataImport.Web.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataImport.Common.ExtensionMethods;

namespace DataImport.Web.Features.ApiServers
{
    public class TestApiServerConnection
    {
        public class Query : IRequest<Response>
        {
            public int? Id { get; set; }

            [Display(Name = "API Version")]
            public string ApiVersion { get; set; }

            [Display(Name = "API Server Url")]
            public string Url { get; set; }

            [Display(Name = "API Server Key")]
            public string Key { get; set; }

            [Display(Name = "API Server Secret")]
            public string Secret { get; set; }
        }

        public class Response
        {
            public bool IsSuccessful { get; }
            public string Message { get; }

            private Response(bool isSuccessful, string message)
            {
                IsSuccessful = isSuccessful;
                Message = message;
            }

            public static Response Success() => new Response(true, null);
            public static Response Failure(string message) => new Response(false, message);
        }

        public class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.ApiVersion).NotEmpty().WithMessage("The API Version could not be determined from the API URL provided.").WithName("API Version");

                RuleFor(x => x.Url).NotEmpty().WithName("API Server Url");

                RuleFor(x => x)
                    .Must(model => !string.IsNullOrEmpty(model.Key) &&
                                   !string.IsNullOrEmpty(model.Secret))
                    .WithMessage("You must authorize access to the ODS API by providing your Key and Secret.");
            }
        }

        public class QueryHandler : IRequestHandler<Query, Response>
        {
            private readonly ILogger _logger;
            private readonly DataImportDbContext _database;
            private readonly IEncryptionService _encryptionService;
            private readonly IConfigurationService _configurationService;
            private readonly IOAuthRequestWrapper _oAuthRequestWrapper;
            private readonly string _encryptionKey;

            public QueryHandler(ILogger<TestApiServerConnection> logger, DataImportDbContext database, IEncryptionKeyResolver encryptionKeyResolver, 
                IEncryptionService encryptionService, IConfigurationService configurationService, IOAuthRequestWrapper oAuthRequestWrapper)
            {
                _logger = logger;
                _database = database;
                _encryptionService = encryptionService;
                _encryptionKey = encryptionKeyResolver.GetEncryptionKey();
                _configurationService = configurationService;
                _oAuthRequestWrapper = oAuthRequestWrapper;
            }

            public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
            {
                var id = request.Id;
                var apiVersion = request.ApiVersion;
                var url = request.Url;
                var key = request.Key;
                var secret = request.Secret;

                var keyIsMasked = SensitiveText.IsMasked(key);
                var secretIsMasked = SensitiveText.IsMasked(secret);

                if (keyIsMasked || secretIsMasked)
                {
                    var existingApiServer = _database.ApiServers.SingleOrDefault(x => x.Id == id);

                    if (existingApiServer != null)
                    {
                        if (keyIsMasked)
                        {
                            key = DecryptText(existingApiServer.Key);
                        }

                        if (secretIsMasked)
                        {
                            secret = DecryptText(existingApiServer.Secret);
                        }
                    }
                }

                var apiServer = new ApiServer
                {
                    ApiVersion = new ApiVersion
                    {
                        Version = apiVersion
                    },
                    Url = url,
                    Key = key,
                    Secret = secret,
                };

                try
                {
                    apiServer.TokenUrl = await _configurationService.GetTokenUrl(request.Url, request.ApiVersion);
                    apiServer.AuthUrl = await _configurationService.GetAuthUrl(request.Url, request.ApiVersion);
                }
                catch (OdsApiServerException e)
                {
                    _logger.LogError(e, "Failed to fetch API metadata.");
                    return Response.Failure("Failed to fetch API metadata. Check the URL is correct.");
                }

                try
                {
                    var client = new RestClient(url);
                    var tokenRetriever = new OdsApiTokenRetriever(_oAuthRequestWrapper, apiServer);
                    client.Authenticator = new BearerTokenAuthenticator(tokenRetriever);

                    var api = new SchoolsApi(client, apiVersion);
                    var apiCall = api.GetAllSchoolsWithHttpResponse(0, 1);

                    if(apiCall.IsSuccessful)
                        return Response.Success();

                    var statusMessage = apiCall.StatusCode.IsSuccessStatusCode()
                        ? "" : $"Status: {apiCall.StatusCode} ";

                    var message = string.IsNullOrEmpty(apiCall.ErrorMessage)
                        ? $"Content: {apiCall.Content}"
                        : apiCall.ErrorMessage;

                    var failureMessage = "API Test failed: " + statusMessage + message;
                    _logger.LogError(failureMessage);
                    return Response.Failure(failureMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Configuration for API failed.");
                    return Response.Failure("API Test threw an exception: " + ex.Message);
                }
            }

            private string DecryptText(string input)
            {
                return _encryptionService.TryDecrypt(input, _encryptionKey, out var decryptedValue)
                    ? decryptedValue
                    : string.Empty;
            }
        }
    }
}