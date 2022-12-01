// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using AutoMapper;
using DataImport.Common.ExtensionMethods;
using DataImport.Common.Helpers;
using DataImport.EdFi;
using DataImport.Models;
using DataImport.Web.Services.Swagger;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataImport.Web.Services
{
    public class EdFiServiceV311 : EdFiServiceBase
    {
        private readonly ISwaggerMetadataFetcher _metadataFetcher;
        private readonly Dictionary<string, string> _yearSpecificYearCache = new Dictionary<string, string>();
        private readonly IOAuthRequestWrapper _oauthRequestWrapper;
        private string _encryptionKey;

        public EdFiServiceV311(DataImportDbContext dbContext, IEncryptionKeyResolver encryptionKeyResolver, IMapper mapper, ISwaggerMetadataFetcher metadataFetcher, IOAuthRequestWrapper oauthRequestWrapper)
            : base(mapper, dbContext)
        {
            _metadataFetcher = metadataFetcher;
            _oauthRequestWrapper = oauthRequestWrapper ?? throw new ArgumentNullException(nameof(oauthRequestWrapper));
            _encryptionKey = encryptionKeyResolver.GetEncryptionKey();
        }

        private string EncryptionKey
        {
            get { return _encryptionKey; }
        }

        public override bool CanHandle(string apiVersion)
        {
            return !apiVersion.IsOdsV2();
        }

        protected override IRestClient EstablishApiClient(ApiServer apiServer)
        {
            var tokenRetriever = new OdsApiTokenRetriever(_oauthRequestWrapper, apiServer, EncryptionKey);

            return new RestClient(apiServer?.Url)
            {
                Authenticator = new BearerTokenAuthenticator(tokenRetriever)
            };
        }

        protected override async Task<string> GetYearSpecificYear(ApiServer apiServer, ApiVersion apiVersion)
        {
            if (apiVersion.Version.IsOdsV2())
            {
                return null;
            }

            string cacheKey = $"{apiVersion}_{apiServer.Url}";
            if (_yearSpecificYearCache.ContainsKey(cacheKey))
            {
                return _yearSpecificYearCache[cacheKey];
            }

            string yearSpecificYear = await _metadataFetcher.GetYearSpecificYear(apiServer.Url);
            _yearSpecificYearCache.Add(cacheKey, yearSpecificYear);

            return yearSpecificYear;
        }
    }
}
