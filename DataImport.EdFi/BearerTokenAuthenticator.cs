// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Linq;
using System.Threading.Tasks;
using DataImport.Common.Helpers;
using RestSharp;
using RestSharp.Authenticators;

namespace DataImport.EdFi
{
    public class BearerTokenAuthenticator : IAuthenticator
    {
        private string _bearerToken;

        public BearerTokenAuthenticator(ITokenRetriever tokenRetriever)
        {
            UpdateToken(tokenRetriever);
        }

        public void UpdateToken(ITokenRetriever tokenRetriever)
        {
            _bearerToken = tokenRetriever.ObtainNewBearerToken();
        }

        public ValueTask Authenticate(IRestClient client, RestRequest request)
        {
            // confirm bearer token is not already there -- implicit IAuthenticator requirement
            if (!request.Parameters.Any(p => p.Name.Equals("Authorization", StringComparison.OrdinalIgnoreCase)))
            {
                request.AddParameter("Authorization", "bearer " + _bearerToken, ParameterType.HttpHeader);
            }
            return new ValueTask();
        }
    }
}
