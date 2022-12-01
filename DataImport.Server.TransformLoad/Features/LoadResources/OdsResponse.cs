// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Net;

namespace DataImport.Server.TransformLoad.Features.LoadResources
{
    public class OdsResponse
    {
        public HttpStatusCode StatusCode { get; }
        public string Content { get; }

        public OdsResponse(HttpStatusCode statusCode, string content)
        {
            StatusCode = statusCode;
            Content = content;
        }
    }
}
