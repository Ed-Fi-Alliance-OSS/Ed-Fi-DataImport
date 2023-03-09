# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

#requires -version 5

$ErrorActionPreference = "Stop"

function MeetsMinimumNuGetVersion {
    param (
        [Version]
        $Version
    )

    if (-not $Version) {
        return $false
    }

    $major = $Version.Major -as [int]
    $minor = $Version.Minor -as [int]

    # Version 5.4 or higher required
    switch ($major) {
        6 {
            return $true
        }
        5 {
            if ($minor -ge 4) {
                return $true
            }
            return $false
        }
        default {
            return $false
        }
    }
}

function Install-NugetCli {
    param(
        [string]
        $ToolsPath  = "$PSScriptRoot/.tools",

        [string]
        $SourceNugetExe = "https://dist.nuget.org/win-x86-commandline/v5.4.0/nuget.exe"
    )

    # See if NuGet >= 5.4.0 already in the path
    $info = Get-Command nuget.exe -ErrorAction SilentlyContinue
    if (MeetsMinimumNuGetVersion -Version $info.Version) {
        return $info
    }

    # Next see if it is in the .tools directory
    $info = Get-Command "$ToolsPath/nuget.exe" -ErrorAction SilentlyContinue
    if (MeetsMinimumNuGetVersion -Version $info.Version) {
        return $info.Path
    }

    # Not found, therefore remove any older version and download the current version
    New-Item -Path $ToolsPath -Type Directory -Force | Out-Null
    $exePath = "$ToolsPath/nuget.exe"
    Remove-Item -Path $exePath -Force -ErrorAction SilentlyContinue | Out-Null

    Write-Host "Downloading nuget.exe official distribution from " $sourceNugetExe
    Write-Host "Saving to $exePath"
    Invoke-WebRequest $sourceNugetExe -OutFile $exePath

    return $exePath
}

function Push-Package {
    param (
        [string]
        $PackageFile,

        [string]
        $NuGetFeed,

        [string]
        $NuGetApiKey       

    )   
    
    Write-Host "Executing: nuget.exe $arguments" -ForegroundColor Magenta
    dotnet nuget push $PackageFile --api-key $NuGetApiKey --source $NuGetFeed
}

function Get-RestApiPackage {
    param (
        [string]
        [Parameter(Mandatory=$true)]
        $RestApiPackageName,

        [string]
        [Parameter(Mandatory=$true)]
        $RestApiPackageVersion,

        [Switch]
        $RestApiPackagePrerelease,

        [string]
        [Parameter(Mandatory=$true)]
        $PackagesPath,

        [string]
        [Parameter(Mandatory=$true)]
        $NuGetFeed,

        [string]
        $ToolsPath = "$PSScriptRoot/.tools"
    )

    $nugetExe = Install-NugetCli -ToolsPath $ToolsPath

    $wildcardPath = "$PackagesPath/$RestApiPackageName.$RestApiPackageVersion*"

    # Remove anything that already exists, so that it is always easy to
    # use Resolve-Path with a wildcard to find the installed path without
    # having to parse pre-release number of the package.
    $existing = Resolve-Path $wildcardPath -ErrorAction SilentlyContinue
    if ($existing) {
        Remove-Item -Path $existing -Force -ErrorAction SilentlyContinue -Recurse | Out-Null
    }

    New-Item -Path $PackagesPath -ItemType Directory -Force | Out-Null

    $arguments = @(
        "install", "$RestApiPackageName",
        "-OutputDirectory", "$PackagesPath",
        "-Source", "$NuGetFeed"
    )

    if ($RestApiPackagePrerelease) {
        $arguments += "-Prerelease"
    }
    else {
        $arguments += "-Version"
        $arguments += "$RestApiPackageVersion"
    }

    Write-Host "Executing: nuget.exe $arguments" -ForegroundColor Magenta
    &$nugetExe @arguments | Out-Null

    if ($LASTEXITCODE -ne 0) {
        throw "NuGet package install failed for RestApi.Databases"
    }

    return (Resolve-Path $wildcardPath)
}

function Move-AppCommon {
    param (
        [string]
        [Parameter(Mandatory=$true)]
        $AppCommonSourceDirectory,

        [string]
        [Parameter(Mandatory=$true)]
        $AppCommonDestinationDirectory
    )

    # Move AppCommon's modules to a destination directory
    @(
        "Application"
        "Environment"
        "IIS"
        "Utility"
    ) | ForEach-Object {
        $parameters = @{
            Recurse = $true
            Force = $true
            Path = "$AppCommonSourceDirectory/$_"
            Destination = "$AppCommonDestinationDirectory/AppCommon/$_"
        }
        Copy-Item @parameters
    }
}

function Add-AppCommon {
    param (
        [string]
        [Parameter(Mandatory=$true)]
        $AppCommonPackageName,

        [string]
        [Parameter(Mandatory=$true)]
        $AppCommonPackageVersion,

        [string]
        [Parameter(Mandatory=$true)]
        $NuGetFeed,

        [string]
        [Parameter(Mandatory=$true)]
        $DestinationPath,

        [string]
        $PackagesPath = "$PSScriptRoot/.packages",

        [string]
        $ToolsPath = "$PSScriptRoot/.tools"
    )

    $wildcardPath = "$PackagesPath/$AppCommonPackageName.$AppCommonPackageVersion*"

    # Remove anything that already exists, so that it is always easy to
    # use Resolve-Path with a wildcard to find the installed path without
    # having to parse pre-release number of the package.
    $existing = Resolve-Path $wildcardPath -ErrorAction SilentlyContinue
    if ($existing) {
        Remove-Item -Path $existing -Force -ErrorAction SilentlyContinue -Recurse | Out-Null
    }

    New-Item -Path $PackagesPath -ItemType Directory -Force | Out-Null

    $parameters = @(
        "install", $AppCommonPackageName,
        "-source", $NuGetFeed,
        "-outputDirectory", $PackagesPath
        "-version", $AppCommonPackageVersion
    )

    Write-Host "Downloading AppCommon"
    Write-Host -ForegroundColor Magenta "Executing nuget: $parameters"
    nuget $parameters | Out-Null

    if ($LASTEXITCODE -ne 0) {
        throw "NuGet package install failed for AppCommon"
    }

    $appCommonDirectory = Resolve-Path $wildcardPath | Select-Object -Last 1

    Move-AppCommon $appCommonDirectory $DestinationPath
}

$functions = @(
    "Install-NugetCli",
    "Get-RestApiPackage",
    "Push-Package",
    "Add-AppCommon"
)

Export-ModuleMember -Function $functions
