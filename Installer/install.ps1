# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

#Requires -Version 5.0

param(
    # Database server hostname or instance name.
    [string]
    $Server = "(local)",

    # Database engine. Options are: SqlServer, PostgreSql.
    [string]
    [ValidateSet("SqlServer", "PostgreSql")]
    $Engine = "SqlServer",

    # When set, uses integrated (Windows) security instead of Username/Password.
    [switch]
    $UseIntegratedSecurity = $true,

    # Database username. Only required when UseIntegratedSecurity is not set.
    [string]
    $Username,

    # Database password. Only required when UseIntegratedSecurity is not set.
    [string]
    $Password,

    # Path where temporary tools will be installed.
    [string]
    $ToolsPath = "C:/temp/tools",

    # Version of the DataImport package to install.
    [string]
    $PackageVersion = "2.3.4.0",

    # Optional token used to recover or reset application user credentials.
    [string]
    $UserRecoveryToken
)

import-module -force "$PSScriptRoot/Install-EdFiDataImport.psm1"

$dbConnectionInfo = @{
    Server                = $Server
    Engine                = $Engine
    UseIntegratedSecurity = $UseIntegratedSecurity.IsPresent
}

if (-not $UseIntegratedSecurity) {
    $dbConnectionInfo["Username"] = $Username
    $dbConnectionInfo["Password"] = $Password
}

$packageSource = Split-Path $PSScriptRoot -Parent

$p = @{
    ToolsPath        = $ToolsPath
    DbConnectionInfo = $dbConnectionInfo
    PackageVersion   = $PackageVersion
    PackageSource    = $packageSource
}

if ($UserRecoveryToken) {
    $p["UserRecoveryToken"] = $UserRecoveryToken
}

Install-EdFiDataImport @p
