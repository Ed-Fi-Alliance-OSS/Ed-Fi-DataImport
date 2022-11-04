// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using AutoMapper;
using DataImport.Common.ExtensionMethods;
using DataImport.Common.Helpers;
using DataImport.EdFi;
using DataImport.Models;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace DataImport.Web.Services
{
    public class EdFiServiceV25 : EdFiServiceBase
    {
        private readonly IOAuthRequestWrapper _oauthRequestWrapper;
        private readonly string _encryptionKey;

        public EdFiServiceV25(DataImportDbContext dbContext, IEncryptionKeyResolver encryptionKeyResolver, IMapper mapper, IOAuthRequestWrapper oauthRequestWrapper) 
            : base(mapper, dbContext)
        {
            _encryptionKey = encryptionKeyResolver.GetEncryptionKey();

            _oauthRequestWrapper = oauthRequestWrapper ?? throw new ArgumentNullException(nameof(oauthRequestWrapper));
        }

        public override bool CanHandle(string apiVersion) => apiVersion.IsOdsV2();

        protected override IRestClient EstablishApiClient(ApiServer apiServer)
        {
            var tokenRetriever = new OdsApiTokenRetriever(_oauthRequestWrapper, apiServer, _encryptionKey);

            return new RestClient(apiServer?.Url)
            {
                Authenticator = new BearerTokenAuthenticator(tokenRetriever)
            };
        }

        protected override Task<string> GetYearSpecificYear(ApiServer apiServer, ApiVersion apiVersion)
        {
            return null;
        }
    }
}