// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Net;

namespace DataImport.Common.Enums
{
    public enum EdFiHttpStatus
    {
        Ok = HttpStatusCode.OK,
        Created = HttpStatusCode.Created,
        Conflict = HttpStatusCode.Conflict,
        BadRequest = HttpStatusCode.BadRequest,
        Unauthorized = HttpStatusCode.Unauthorized,
        Forbidden = HttpStatusCode.Forbidden,
        InternalServerError = HttpStatusCode.InternalServerError,
        NotImplemented = HttpStatusCode.NotImplemented,
        NotFound = HttpStatusCode.NotFound,
        ServiceUnavailable = HttpStatusCode.ServiceUnavailable,
        NoContent = HttpStatusCode.NoContent,
        NotModified = HttpStatusCode.NotModified,
        TemporaryRedirect = HttpStatusCode.TemporaryRedirect,
        Other = 1
    }
}
