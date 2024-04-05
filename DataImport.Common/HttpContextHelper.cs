// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;

namespace Microsoft.AspNetCore.Http
{
    /// <summary>
    /// Temporary helper class for retrieving the current <see cref="HttpContext"/> . This temporary
    /// workaround should be removed in the future and <see cref="HttpContext"/> HttpContext should be retrieved
    /// from the current controller, middleware, or page instead. If working in another
    /// component, the current <see cref="HttpContext"/> can be retrieved from an <see cref="IHttpContextAccessor"/>
    /// retrieved via dependency injection.
    /// </summary>
    internal static class HttpContextHelper
    {
        private static readonly HttpContextAccessor _httpContextAccessor = new HttpContextAccessor();

        /// <summary>
        /// Gets the current <see cref="HttpContext"/>. Returns <c>null</c> if there is no current <see cref="HttpContext"/>.
        /// </summary>
#pragma warning disable S1133 // Deprecated code should be removed
        [Obsolete("Prefer accessing HttpContext via injection", error: false, DiagnosticId = "HttpContextCurrent", UrlFormat = "https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-context")]
#pragma warning restore S1133 // Deprecated code should be removed
        public static HttpContext Current => _httpContextAccessor.HttpContext;
    }
}
