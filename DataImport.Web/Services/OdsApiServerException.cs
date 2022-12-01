// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using System;

namespace DataImport.Web.Services
{
    public class OdsApiServerException : Exception
    {
        public int? ApiServerId { get; set; }

        public OdsApiServerException(ApiServer apiServer, Exception innerException)
            : this(innerException)
        {
            if (apiServer == null) throw new ArgumentNullException(nameof(apiServer));

            ApiServerId = apiServer.Id;
        }

        public OdsApiServerException(int apiServerId, Exception innerException)
            : this(innerException)
        {
            ApiServerId = apiServerId;
        }

        public OdsApiServerException(Exception innerException)
            : base("An exception was thrown while attempting to access the ODS API. " +
                   "See the inner exception for details.", innerException)
        {
        }
    }
}
