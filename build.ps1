# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

[CmdLetBinding()]
<#
    .SYNOPSIS
        Automation script for running build operations from the command line.

    .DESCRIPTION
        Provides automation of the following tasks:

        * SetUp: installs NuGet and Python to \tools
        * Clean: runs `dotnet clean`
        * Build: runs `dotnet build` with several implicit steps
          (clean, restore, inject version information).
        * UnitTest: executes NUnit tests in projects named `*.UnitTests`, which
          do not connect to a database.
        * IntegrationTest: executes NUnit tests in projects named `*.Test`,
          which connect to a database. Includes drop and deploy operations for
          installing fresh test databases.
        * BuildAndTest: executes the Build, UnitTest, and IntegrationTest
          commands.
        * Package: builds pre-release and release NuGet packages for the Data Import
          web application.
        * Push: uploads a NuGet package to the NuGet feed.
        * BuildAndDeployToDockerContainer: runs the build operation, update the appsettings.json with provided
          DockerEnvValues and copy over the latest files to existing DataImport docker container for testing.

    .EXAMPLE
        .\build.ps1 build -Configuration Release -Version "2.0.0"

        Overrides the default build configuration (Debug) to build in release
        mode with assembly version 2.0.0.45.

    .EXAMPLE
        .\build.ps1 unittest

        Output: test results displayed in the console and saved to XML files.

    .EXAMPLE
        .\build.ps1 integrationtest

        Output: test results displayed in the console and saved to XML files.

    .EXAMPLE
        .\build.ps1 package -Version "2.0.0"

        Output: NuGet package, with version 2.0.0.

    .EXAMPLE
        .\build.ps1 push -NuGetApiKey $env:nuget_key

    .EXAMPLE
       $p = @{
            ProductionApiUrl = "http://api"
            ApiExternalUrl = "https://localhost:5001"
            AppStartup = "OnPrem"
            XsdFolder = "/app/Schema"
            ApiStartupType = "SharedInstance"
            DatabaseEngine = "PostgreSql"
            BulkUploadHashCache = "/app/BulkUploadHashCache/"
            EncryptionKey = "<Generated encryption key>"
            DefaultConnection = "host=db-dataimport;port=5432;username=username;password=password;database=DataImport;Application Name=EdFi.DataImport;"
            }

        .\build.ps1 -Version "2.1.1" -Configuration Release -DockerEnvValues $p -Command BuildAndDeployToDockerContainer
#>
param(
    # Command to execute, defaults to "Build".
    [string]
    [ValidateSet("SetUp", "Clean", "Build", "UnitTest", "IntegrationTest", "PowerShellTests", "Package", "PackageTransformLoad", "Push", "BuildAndTest", "BuildAndPublish", "BuildAndDeployToDockerContainer", "Run")]
    $Command = "Build",

    # Assembly and package version number. The current package number is
    # configured in the build automation tool and passed to this script.
    [string]
    $Version = "0.1.0",   

    # .NET project build configuration, defaults to "Debug". Options are: Debug, Release.
    [string]
    [ValidateSet("Debug", "Release")]
    $Configuration = "Debug",

    # Ed-Fi's official NuGet package feed for package download and distribution.
    [string]
    $EdFiNuGetFeed = "https://pkgs.dev.azure.com/ed-fi-alliance/Ed-Fi-Alliance-OSS/_packaging/EdFi/nuget/v3/index.json",

    # API key for accessing the feed above. Only required with with the Push
    # command.
    [string]
    $NuGetApiKey,

    # Full path of a package file to push to the NuGet feed. Optional, only
    # applies with the Push command. If not set, then the script looks for a
    # NuGet package corresponding to the provided $Version.
    [string]
    $PackageFile,

    # Environment values for updating the appsettings on existing DataImport docker container.

    # Only required with the BuildAndDeployToDockerContainer command.
    [hashtable]
    $DockerEnvValues,

    # Only required with the Run command.
    [string]
    [ValidateSet("mssql", "pg")]
    $LaunchProfile,

    #Generate Test Report
    [switch]
    $Report,

    # Only required with the Push command.
    [string]
    [ValidateSet("Web", "TransformLoad")]
    $PackageFileType
)

$Env:MSBUILDDISABLENODEREUSE = "1"

$solution = "DataImport.sln"
$solutionRoot = "$PSScriptRoot"
$entryProject = "DataImport.Web"
$transformLoadProject = "DataImport.Server.TransformLoad"
$maintainers = "Ed-Fi Alliance, LLC and contributors"

Import-Module -Name "$PSScriptRoot/eng/build-helpers.psm1" -Force
Import-Module -Name "$PSScriptRoot/eng/package-manager.psm1" -Force
function Clean {
    Invoke-Execute { dotnet clean $solutionRoot -c $Configuration --nologo -v minimal }
}

function InitializeNuGet {
    Invoke-Execute { $script:nugetExe = Install-NugetCli }
}

function InitializePython {
	$toolsDir = "$PSScriptRoot/.tools"
	if (-not (Test-Path "$toolsDir")) {
        New-Item -ItemType Directory -Force -Path "$toolsDir" | Out-Null
    }

    $python = "$toolsDir\python\python.exe"
    if (-not (Test-Path $python)) {
        $sourcePythonZip = "https://www.python.org/ftp/python/3.10.2/python-3.10.2-embed-amd64.zip"
		$destPythonZip = "$toolsDir\python-3.10.2-embed-amd64.zip"
		
        Write-Host "Downloading python 3.10.2 official release"
        Invoke-WebRequest $sourcePythonZip -OutFile $destPythonZip
		Expand-Archive $destPythonZip -DestinationPath "$toolsDir\python"
		Remove-Item $destPythonZip
    }
}

function Clean {
    Invoke-Execute { dotnet clean $solutionRoot -c $Configuration --nologo -v minimal }
}

function Restore {
    Invoke-Execute { dotnet restore $solution }
}

function AssemblyInfo {
    Invoke-Execute {
        $assembly_version = $Version

        Invoke-RegenerateFile "$solutionRoot/Directory.Build.props" @"
<Project>
    <!-- This file is generated by the build script. -->
    <PropertyGroup>
        <Product>Ed-Fi DataImport</Product>
        <Authors>$maintainers</Authors>
        <Company>$maintainers</Company>
        <Copyright>Copyright © 2016 Ed-Fi Alliance</Copyright>
        <VersionPrefix>$assembly_version</VersionPrefix>
        <VersionSuffix></VersionSuffix>
    </PropertyGroup>
</Project>

"@
    }
}

function Compile {
    Invoke-Execute {
        dotnet --info
        dotnet build $solutionRoot -c $Configuration --nologo --no-restore
    }
}

function PublishWeb {
    Invoke-Execute {
        $outputPath = "$solutionRoot/$entryProject/publish"
        $project = "$solutionRoot/$entryProject/"
        dotnet publish $project -c $Configuration /p:EnvironmentName=Production -o $outputPath --no-build --nologo
    }
}

function PublishTransformLoad {
    Invoke-Execute {
        $outputPath = "$solutionRoot/$transformLoadProject/publish/fdd"
        $project = "$solutionRoot/$transformLoadProject/"
        dotnet publish $project -c $Configuration /p:EnvironmentName=Production -o $outputPath --no-build --nologo
    }
}

function PublishTransformLoadSelfContained {
    Invoke-Execute {
        $outputPath = "$solutionRoot/$transformLoadProject/publish/scd"
        $project = "$solutionRoot/$transformLoadProject/"      
        dotnet publish $project -c $Configuration /p:EnvironmentName=Production -o $outputPath -r win10-x64 --nologo --self-contained true
    }
}

function RunTests {
    param (
        # File search filter
        [string]
        $Filter
    )

    if ($Report) {
      Invoke-Execute { dotnet test -c $Configuration --filter $Filter --logger "trx;LogFileName=test-results.trx" }
    } else {
      Invoke-Execute { dotnet test -c $Configuration --filter $Filter }
    }
}

function UnitTests {
    Invoke-Execute { RunTests -Filter "FullyQualifiedName~.UnitTests" }
}

function ResetTestDatabases {
    param (
        [string]
        $OdsPackageName,

        [string]
        $OdsVersion,

        [switch]
        $Prerelease
    )

    Invoke-Execute {
        $arguments = @{
            RestApiPackageVersion = $OdsVersion
            RestApiPackageName = $OdsPackageName
            UseIntegratedSecurity = $true
            RestApiPackagePrerelease = $Prerelease
            NuGetFeed = $EdFiNuGetFeed
        }

        Invoke-PrepareDatabasesForTesting @arguments
    }
}

function IntegrationTests {
    Invoke-Execute { RunTests -Filter "FullyQualifiedName~.Tests&Category!=PowerShellTests" }
}

function PowerShellTests {
    Invoke-Execute { RunTests -Filter "FullyQualifiedName~.Tests&Category=PowerShellTests" }
}

function RunDotNetPack {
    param (
        [string]
        $PackageVersion,

        [string]
        $ProjectName,

        [string]
        $NuspecFileName
    )

    dotnet pack "$ProjectName.csproj" --no-build --no-restore --output "$PSScriptRoot" --configuration $Configuration -p:NuspecFile="$NuspecFileName.nuspec" -p:NuspecProperties="version=$PackageVersion"
}

function NewDevCertificate {
    Invoke-Command { dotnet dev-certs https -c }
    if ($lastexitcode) {
        Write-Host "Generating a new Dev Certificate" -ForegroundColor Magenta
        Invoke-Execute { dotnet dev-certs https --clean }
        Invoke-Execute { dotnet dev-certs https -t }
    } else {
        Write-Host "Dev Certificate already exists" -ForegroundColor Magenta
    }
}

function BuildPackage {
    $baseProjectFullName = "$solutionRoot/$entryProject/$entryProject"   
    RunDotNetPack -PackageVersion $Version -projectName $baseProjectFullName $baseProjectFullName
}

function BuildTransformLoadPackage {
    $baseProjectFullName = "$solutionRoot/$transformLoadProject/$transformLoadProject"  
    RunDotNetPack -PackageVersion $Version -projectName $baseProjectFullName $baseProjectFullName

    # Create windows 64 specific package
    RunDotNetPack -PackageVersion $Version -projectName $baseProjectFullName "$baseProjectFullName.Scd"
}

function PushPackage {
    param (
        [string]
        $PackageVersion = $Version       
    )

    if (-not $NuGetApiKey) {
        throw "Cannot push a NuGet package without providing an API key in the `NuGetApiKey` argument."
    }

    if (-not $PackageFile) {
        if("Web" -ieq $PackageFileType){       
         $PackageFile = "$PSScriptRoot/$entryProject.$PackageVersion.nupkg"  
         DotnetPush  $PackageFile
        }
        else {      
         $PackageFile = "$PSScriptRoot/$transformLoadProject.$PackageVersion.nupkg" 
         DotnetPush  $PackageFile
         $PackageFileWin64 = "$PSScriptRoot/$transformLoadProject.Win64.$PackageVersion.nupkg"
         DotnetPush  $PackageFileWin64
        }
    }
    else
    {
        DotnetPush  $PackageFile
    }
}

function DotnetPush {
    param (
        [string]
        $PackageFileName
    )

    Write-Host "Pushing $PackageFileName to $EdFiNuGetFeed"
    dotnet nuget push $PackageFileName --api-key $NuGetApiKey --source $EdFiNuGetFeed
}

function Invoke-Build {
    Write-Host "Building Version $Version" -ForegroundColor Cyan

    Invoke-Step { Clean }
    Invoke-Step { Restore }
    Invoke-Step { Compile }
}

function Invoke-Publish {
    Invoke-Step { PublishWeb }
    Invoke-Step { PublishTransformLoad }
    Invoke-Step { PublishTransformLoadSelfContained }
}

function Invoke-Run {
    Write-Host "Running Data Import" -ForegroundColor Cyan

    Invoke-Step { NewDevCertificate }

    $projectFilePath = "$solutionRoot/$entryProject"

    if ([string]::IsNullOrEmpty($LaunchProfile)) {
        Write-Host "LaunchProfile parameter is required for running Data Import. Please specify the LaunchProfile parameter. Valid values include 'mssql-district', 'mssql-shared', 'mssql-year', 'pg-district', 'pg-shared' and 'pg-year'" -ForegroundColor Red
    } else {
        Invoke-Execute { dotnet run --project $projectFilePath --launch-profile $LaunchProfile }
    }
}

function Invoke-SetUp {
    Invoke-Step { InitializePython }
	Invoke-Step { InitializeNuGet }
}

function Invoke-Clean {
    Invoke-Step { Clean }
}

function Invoke-UnitTests {
    Invoke-Step { UnitTests }
}

function Invoke-IntegrationTests {
    Invoke-Step { IntegrationTests }
}

function Invoke-PowerShellTests {
    Invoke-Step { PowerShellTests }
}

function Invoke-BuildPackage {
    Invoke-Step { InitializeNuGet }
    Invoke-Step { BuildPackage }
}

function Invoke-BuildTransformLoadPackage {
    Invoke-Step { InitializeNuGet }
    Invoke-Step { BuildTransformLoadPackage }
}

function Invoke-PushPackage {    
    Invoke-Step { PushPackage }
}

function UpdateAppSettingsForDocker {
    $filePath = "$solutionRoot/$entryProject/publish/appsettings.json"
    $json = (Get-Content -Path $filePath) | ConvertFrom-Json
    Write-Output $DockerEnvValues | Out-Host

    $json.ConnectionStrings.defaultConnection = $DockerEnvValues["defaultConnection"]
    $json | ConvertTo-Json | Set-Content $filePath
}

function CopyLatestFilesToContainer {
    $source = "$solutionRoot/$entryProject/publish/."
    docker cp $source dataimport:/app
}

function RestartDataImportContainer {
    &docker restart dataimport
}

function Invoke-DockerDeploy {
   Invoke-Step { UpdateAppSettingsForDocker }
   Invoke-Step { CopyLatestFilesToContainer }
   Invoke-Step { RestartDataImportContainer }
}

function Invoke-SetAssemblyInfo {
    Write-Output "Setting Assembly Information" -ForegroundColor Cyan

    Invoke-Step { AssemblyInfo }  
}

Invoke-Main {
    switch ($Command) {
        SetUp { Invoke-SetUp }
		Clean { Invoke-Clean }
        Build { Invoke-Build }
        BuildAndPublish {
            Invoke-SetAssemblyInfo
            Invoke-Build
            Invoke-Publish
        }
        Run { Invoke-Run }
        UnitTest { Invoke-UnitTests }
        IntegrationTest { Invoke-IntegrationTests }
        PowerShellTests { Invoke-PowerShellTests }
        BuildAndTest {
            Invoke-Build
            Invoke-UnitTests
            Invoke-IntegrationTests
            Invoke-PowerShellTests
        }
        Package { Invoke-BuildPackage }
        PackageTransformLoad { Invoke-BuildTransformLoadPackage }
        Push { Invoke-PushPackage }
        BuildAndDeployToDockerContainer {
            Invoke-Build
            Invoke-DockerDeploy
        }
        default { throw "Command '$Command' is not recognized" }
    }
}
