# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

#requires -version 5
param (
    [string]
    [Parameter(Mandatory=$true)]
    $SemanticVersion,

    [string]
    [Parameter(Mandatory=$true)]
    $BuildCounter,

    [string]
    $PreReleaseLabel = "pre",

    [switch]
    $PublishReleaseAndRepackage,

    [switch]
    $PublishPreRelease,

    [string]
    $NuGetFeed,

    [string]
    $NuGetApiKey
)
$NuGetApiKeyReceived = $NuGetApiKey;
$NuGetFeedReceived = $NuGetFeed;
$ErrorActionPreference = "Stop"
$OutputDirectory = Resolve-Path $PSScriptRoot
$PackageDefinitionFile = Resolve-Path "$PSScriptRoot/Installer.DataImport.nuspec"
$Downloads = "$PSScriptRoot/downloads"

function Add-AppCommon{

    if(-not(Test-Path -Path $Downloads )){
        mkdir $Downloads
    }

    $PackageName = "EdFi.Installer.AppCommon"
    $PackageVersion = "3.0.0"

    $parameters = @(
        "install", $PackageName,
        "-source", $NuGetFeedReceivedq,
        "-outputDirectory", $Downloads
        "-version", $PackageVersion
    )

    Write-Information "Downloading AppCommon"
    Write-Information "Executing nuget: $parameters"
    nuget $parameters

    $appCommonDirectory = Resolve-Path $Downloads/$PackageName.$PackageVersion* | Select-Object -Last 1

    # Move AppCommon's modules to a local AppCommon directory
    @(
        "Application"
        "Environment"
        "IIS"
        "Utility"
    ) | ForEach-Object {
        $parameters = @{
            Recurse = $true
            Force = $true
            Path = "$appCommonDirectory/$_"
            Destination = "$PSScriptRoot/AppCommon/$_"
        }
        Copy-Item @parameters
    }
}

function New-Package {
    [CmdletBinding(SupportsShouldProcess=$true)]
    param (
        [string]
        $Suffix
     )
    if ($PSCmdlet.ShouldProcess("Package", "New")) {
        $parameters = @(
            "pack", $PackageDefinitionFile,
            "-Version", $SemanticVersion,
            "-OutputDirectory", $OutputDirectory,
            "-Verbosity", "detailed"
        )
        if ($Suffix) {
            $parameters += "-Suffix"
            $parameters += $Suffix
        }

        Write-Information @parameters
        nuget @parameters
    }
}

function Get-PackageId
{
    [ xml ] $fileContents = Get-Content -Path  $PackageDefinitionFile
    return $fileContents.package.metadata.id
}

function Publish-Package{
    param (
        [string]
        $Version = $SemanticVersion
    )

    $packageId = Get-PackageId
    $packageName = "$packageId.$Version.nupkg"

    $parameters = @(
        "push", (Get-ChildItem "$OutputDirectory/$packageName").FullName,
        "-Source", $NuGetFeedReceived
        "-ApiKey", $NuGetApiKeyReceived,
        "-Verbosity", "detailed"
    )

    Write-Information "Pushing $packageName to azure artifacts"
    nuget @parameters
}

#Add AppCommon
Add-AppCommon

# Build release
Write-Information "Building Release package"
New-Package

# Build pre-release
Write-Information "Building Pre-release package"
$Suffix = "$PreReleaseLabel$($BuildCounter.PadLeft(4,'0'))"
New-Package $Suffix

if($PublishReleaseAndRepackage)
{
    Write-Information "Publishing release package"
    Publish-Package
}
if ($PublishPreRelease) {
    Write-Information "Publishing pre-release package"
    $PackageVersion = $SemanticVersion + "-$Suffix"
    Publish-Package $PackageVersion
}
