# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

import-module -force "$PSScriptRoot/Install-EdFiDataImport.psm1"

<#
Review and edit the following connection information for your database server

.EXAMPLE
Installs and connects the applications to the database using SQL Authentication

    $dbConnectionInfo = @{
        Server = "(local)"
        Engine = "SqlServer"
        UseIntegratedSecurity = $false
        Username = "exampleAdmin"
        Password = "examplePassword"
    }

Installs and connects the applications to the database using PostgreSql Authentication

    $dbConnectionInfo = @{
        Server = "localhost"
        Engine = "PostgreSql"
        UseIntegratedSecurity = $false
        Username = "postgres"
        Password = "examplePassword"
    }
#>

$dbConnectionInfo = @{
    Server                = "(local)"
    Engine                = "SqlServer"
    UseIntegratedSecurity = $true
}

<#
Review and edit the following application settings and connection information for Data Import

.EXAMPLE
Configure DataImport

    $p = @{
        ToolsPath = "C:/temp/tools"
        DbConnectionInfo = $dbConnectionInfo
        PackageVersion = '2.3.0.0'
    }

    UserRecoveryToken is optional. This value can be used to recover/ reset the application user credentials
    $p = @{
        ToolsPath = "C:/temp/tools"
        DbConnectionInfo = $dbConnectionInfo
        PackageVersion = '2.3.0.0'
        UserRecoveryToken = "bEnFYNociET2R1Wua3DHzwfU5u"
    }
#>

$packageSource = Split-Path $PSScriptRoot -Parent

$p = @{
    ToolsPath        = "C:/temp/tools"
    DbConnectionInfo = $dbConnectionInfo
    PackageVersion   = '2.3.0.0'
    PackageSource    = $packageSource
}

Install-EdFiDataImport @p
