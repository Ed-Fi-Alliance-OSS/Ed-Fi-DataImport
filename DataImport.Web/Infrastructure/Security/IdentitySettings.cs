// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;

namespace DataImport.Web.Infrastructure.Security;

public class IdentitySettings
{
    public string Type { get; set; }
    public OpenIdSettings OpenIdSettings { get; set; }

    public string ControllerName =>
        Type == IdentitySettingsConstants.OpenIdIdentityType
            ? IdentitySettingsConstants.OpenIdIdentityControllerName
            : IdentitySettingsConstants.EntityFrameworkIdentityControllerName;
}

public class OpenIdSettings
{
    public string Authority { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string UserProfileUri { get; set; }
    public string AuthenticationScheme { get; set; }
    public string ResponseType { get; set; }
    public bool RequireHttpsMetadata { get; set; }
    public bool GetClaimsFromUserInfoEndpoint { get; set; }
    public bool SaveTokens { get; set; }
    public List<string> Scopes { get; set; }

    /// <summary>
    /// Mappings from OIDC Claim Types to those used by Admin App. Should only be used "OnTicketReceived" - favor ClaimTypes used by Admin App, not OIDC.
    /// </summary>
    public ClaimTypeMappings ClaimTypeMappings { get; set; }
}

public class ClaimTypeMappings
{
    public string NameClaimType { get; set; }
    public string IdentifierClaimType { get; set; }
    public string EmailClaimType { get; set; }
    public string RoleClaimType { get; set; }
}
