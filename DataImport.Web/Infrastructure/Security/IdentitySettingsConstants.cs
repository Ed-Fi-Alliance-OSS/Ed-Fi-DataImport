// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace DataImport.Web.Infrastructure.Security;

public class IdentitySettingsConstants
{
    public const string EntityFrameworkIdentityType = "EntityFramework";
    public const string OpenIdIdentityType = "OpenId";
    public const string OpenIdIdentityControllerName = "OpenIdConnect";
    public const string EntityFrameworkIdentityControllerName = "Account";
    public const string RoleClaimValue = "edfi_data_import/admin";
}
