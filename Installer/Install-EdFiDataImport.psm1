# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

#requires -version 5

$ErrorActionPreference = "Stop"

function Set-TlsVersion {

    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls11 -bor [Net.SecurityProtocolType]::Tls12 -bor [Net.SecurityProtocolType]::Tls13
}

Import-Module "$PSScriptRoot/key-management.psm1"

$appCommonDirectory = "$PSScriptRoot/AppCommon"
$RequiredDotNetHostingBundleVersion = "6.0.0"
Import-Module -Force "$appCommonDirectory/Environment/Prerequisites.psm1" -Scope Global
Set-TlsVersion
Install-DotNetCore "C:\temp\tools"

Import-Module -Force "$appCommonDirectory/Utility/hashtable.psm1" -Scope Global
Import-Module -Force "$appCommonDirectory/Utility/nuget-helper.psm1"
Import-Module -Force "$appCommonDirectory/Utility/TaskHelper.psm1"
Import-Module -Force "$appCommonDirectory/Utility/ToolsHelper.psm1"

# Import the following with global scope so that they are available inside of script blocks
Import-Module -Force "$appCommonDirectory/Application/Install.psm1" -Scope Global
Import-Module -Force "$appCommonDirectory/Application/Uninstall.psm1" -Scope Global
Import-Module -Force "$appCommonDirectory/Application/Configuration.psm1" -Scope Global

function Install-EdFiDataImport {
    <#
    .SYNOPSIS
        Installs the Data Import application into IIS.

    .DESCRIPTION
        Installs and configures the Data Import application in IIS running in Windows 10 or
        Windows Server 2016+. As needed, will create a new "Ed-Fi" website in IIS, configure it
        for HTTPS, and load the Data Import binaries as an an application. Transforms the web.config.
    .EXAMPLE
        PS c:\> $dbConnectionInfo = @{
            Server = "(local)"
            Engine = "SqlServer"
            UseIntegratedSecurity=$true
        }
        PS c:\> $parameters = @{
            ToolsPath = "C:/temp/tools"
            DbConnectionInfo = $dbConnectionInfo
        }
        PS c:\> Install-EdFiDataImport @parameters

        Installs Data Import to SQL Server with mainly defaults
    #>
    [CmdletBinding()]
    param (
        # Default: DataImport.Web.
        [string]
        $WebPackageName = "DataImport.Web",

        # Default: DataImport.Server.TransformLoad.
        [string]
        $TransformLoadPackageName = "DataImport.Server.TransformLoad.Win64",
        
        # Data Import version.
        [string]
        $PackageVersion,

        # NuGet package source. Defaults to "https://pkgs.dev.azure.com/ed-fi-alliance/Ed-Fi-Alliance-OSS/_packaging/EdFi/nuget/v3/index.json".
        [string]
        $PackageSource = "https://pkgs.dev.azure.com/ed-fi-alliance/Ed-Fi-Alliance-OSS/_packaging/EdFi/nuget/v3/index.json",

        # Path for storing installation tools, e.g. nuget.exe. Default: "C:\temp\tools".
        [string]
        $ToolsPath = "C:\temp\tools",

        # Path for storing downloaded packages
        [string]
        $DownloadPath = "C:\temp\downloads",

        # Path for the IIS WebSite. Default: c:\inetpub\Ed-Fi.
        [string]
        $WebSitePath = "c:\inetpub\Ed-Fi",

        # Web site name. Default: "Ed-Fi".
        [string]
        $WebsiteName = "Ed-Fi",

        # Web site port number. Default: 444.
        [int]
        $WebSitePort = 444,

        # Web application name. Default: "DataImport".
        [string]
        $WebApplicationName = "DataImport",
        
        # Transform Load application name. Default: "DataImportTransformLoad".
        [string]
        $TransformLoadApplicationName = "DataImportTransformLoad",

        # TLS certificiate thumbprint, optional. When not set, a self-signed certificate will be created.
        [string]
        $CertThumbprint,

        # Data Import Database Name
        [string]
        $DataImportDatabaseName = "EdFi_DataImport",

        # Database connectivity information.
        #
        # The hashtable must include: Server, Engine (SqlServer or PostgreSQL), and
        # either UseIntegratedSecurity or Username and Password. Optionally can include Port.
        [hashtable]
        [Parameter(Mandatory=$true)]
        $DbConnectionInfo,

        # Database Config
        [switch]
        $NoDuration,

        # User recovery token
        [string]
        $UserRecoveryToken
    )

    Write-InvocationInfo $MyInvocation

    Clear-Error

    $result = @()

    $Config = @{
        WebApplicationPath = (Join-Path $WebSitePath $WebApplicationName)
        TransformLoadApplicationPath = (Join-Path $WebSitePath $TransformLoadApplicationName)
        WebPackageName = $WebPackageName
        TransformLoadPackageName = $TransformLoadPackageName
        PackageVersion = $PackageVersion
        PackageSource = $PackageSource
        ToolsPath = $ToolsPath
        DownloadPath = $DownloadPath
        WebSitePath = $WebSitePath
        WebSiteName = $WebsiteName
        WebSitePort = $WebsitePort
        CertThumbprint = $CertThumbprint
        DataImportDatabaseName = $DataImportDatabaseName
        WebApplicationName = $WebApplicationName
        TransformLoadApplicationName = $TransformLoadApplicationName
        DbConnectionInfo = $DbConnectionInfo
        NoDuration = $NoDuration
        UserRecoveryToken = $UserRecoveryToken
    }

    $elapsed = Use-StopWatch {
        $result += Initialize-Configuration -Config $config
        $result += Get-DataImportPackages -Config $config
        $result += Invoke-TransformDataImportWebAppSettings -Config $Config
        $result += Invoke-TransformDataImportTransformLoadAppSettings -Config $Config
        $result += Invoke-TransformDataImportWebConnectionStrings -Config $config
        $result += Invoke-TransformDataImportTransformLoadConnectionStrings -Config $config
        $result += Install-Application -Config $Config
        $result += Set-SqlLogins -Config $Config

        $result
    }

    Test-Error

    if (-not $NoDuration) {
        $result += New-TaskResult -name "-" -duration "-"
        $result += New-TaskResult -name $MyInvocation.MyCommand.Name -duration $elapsed.format
        $result | Format-Table
    }
}

function Uninstall-EdFiDataImport {
    <#
    .SYNOPSIS
        Removes the Data Import web application from IIS.
    .DESCRIPTION
        Removes the Data Import web application from IIS, including its application
        pool (if not used for any other application). Removes the web site as well if
        there are no remaining applications, and the site's app pool.

        Does not remove IIS or the URL Rewrite module.

    .EXAMPLE
        PS c:\> Uninstall-EdFiDataImport

        Uninstall using all default values.
    .EXAMPLE
        PS c:\> $p = @{
            WebSiteName="Ed-Fi-3"
            WebApplicationPath="c:/inetpub/Ed-Fi-3/DataImport-3"
            WebApplicationName = "DataImport-3"
        }
        PS c:\> Uninstall-EdFiDataImport @p

        Uninstall when the web application and web site were setup with non-default values.
    #>
    [CmdletBinding()]
    param (
        # Path for storing installation tools, e.g. nuget.exe. Default: "./tools".
        [string]
        $ToolsPath = "$PSScriptRoot/tools",

        # Path for the web application. Default: "C:\inetpub\Ed-Fi\DataImport".
        [string]
        $WebApplicationPath = "C:\inetpub\Ed-Fi\DataImport",

        # Path for the transform load application. Default: "C:\inetpub\Ed-Fi\DataImportTransformLoad".
        [string]
        $TransformLoadApplicationPath = "C:\inetpub\Ed-Fi\DataImportTransformLoad",

        # Web application name. Default: "DataImport".
        [string]
        $WebApplicationName = "DataImport",

        # Web site name. Default: "Ed-Fi".
        [string]
        $WebSiteName = "Ed-Fi",

        # Turns off display of script run-time duration.
        [switch]
        $NoDuration
    )

    $config = @{
        ToolsPath = $ToolsPath
        WebApplicationPath = $WebApplicationPath
        TransformLoadApplicationPath = $TransformLoadApplicationPath 
        WebApplicationName = $WebApplicationName
        WebSiteName = $WebSiteName
    }

    $result = @()

    $elapsed = Use-StopWatch {

        Invoke-ResetIIS

        UninstallDataImport $config

        RemoveTransformLoad $config

        $result
    }

    Test-Error

    if (-not $NoDuration) {
        $result += New-TaskResult -name "-" -duration "-"
        $result += New-TaskResult -name $MyInvocation.MyCommand.Name -duration $elapsed.format
        $result | Format-Table
    }
}

function UninstallDataImport($config)
{
    $parameters = @{
        WebApplicationPath = $config.WebApplicationPath
        WebApplicationName = $config.WebApplicationName
        WebSiteName = $config.WebSiteName
    }

    Uninstall-EdFiApplicationFromIIS @parameters
}

function RemoveTransformLoad($Config)
{
    Remove-Item -Path $Config.TransformLoadApplicationPath -Force -Recurse
}

function Get-DataImportPackages {
    [CmdletBinding()]
    param (
        [hashtable]
        [Parameter(Mandatory=$true)]
        $Config
    )

    Invoke-Task -Name ($MyInvocation.MyCommand.Name) -Task {
        $parameters = @{
            PackageName = $Config.WebPackageName
            PackageVersion = $Config.PackageVersion
            ToolsPath = $Config.ToolsPath
            OutputDirectory = $Config.DownloadPath
            PackageSource = $Config.PackageSource
        }
        $webPackageDir = Get-NugetPackage @parameters
        Test-Error

        $parameters = @{
            PackageName = $Config.TransformLoadPackageName
            PackageVersion = $Config.PackageVersion
            ToolsPath = $Config.ToolsPath
            OutputDirectory = $Config.DownloadPath
            PackageSource = $Config.PackageSource
        }
        $transformLoadPackageDir = Get-NugetPackage @parameters
        Test-Error

        $Config.PackageDirectory = $webPackageDir
        $Config.DataImportWebSettingsPath = $webPackageDir 
        $Config.DataImportTransformLoadSettingsPath = $transformLoadPackageDir
    }
}


function Invoke-ResetIIS {
    Invoke-Task -Name ($MyInvocation.MyCommand.Name) -Task {
        $default = 'n'
        Write-Warning "NOTICE: In order to upgrade or uninstall, Information Internet Service (IIS) needs to be stopped during the process. This will impact availability if users are using applications hosted with IIS."
        $confirmation = Request-Information -DefaultValue 'y' -Prompt "Please enter 'y' to proceed with an IIS reset or enter 'n' to stop the upgrade or uninstall. [Default Action: '$default']"

        if (!$confirmation) { $confirmation = $default}
        if ($confirmation -ieq 'y') {
            & {iisreset}
        }
        else {
            Write-Warning "Exiting the uninstall/upgrade process."
            exit
        }
    }
}

function Initialize-Configuration {
    [CmdletBinding()]
    param (
        [hashtable]
        [Parameter(Mandatory=$true)]
        $Config
    )
    Invoke-Task -Name ($MyInvocation.MyCommand.Name) -Task {
        $Config.hasDbConnectionInfo = $Config.ContainsKey("DbConnectionInfo") -and (-not $null -eq $Config.DbConnectionInfo)
        if ($Config.hasDbConnectionInfo) {
            Assert-DatabaseConnectionInfo -DbConnectionInfo $Config.DbConnectionInfo
            $Config.engine = $Config.DbConnectionInfo.Engine
        }
    }
}

function New-JsonFile {
    param(
        [string] $FilePath,

        [hashtable] $Hashtable,

        [switch] $Overwrite
    )

    if (-not $Overwrite -and (Test-Path $FilePath)) { return }

    $Hashtable | ConvertTo-Json -Depth 10 | Out-File -FilePath $FilePath -NoNewline -Encoding UTF8
}

function Invoke-TransformDataImportWebAppSettings {
    [CmdletBinding()]
    param (
        [hashtable]
        [Parameter(Mandatory=$true)]
        $Config
    )

    Invoke-Task -Name ($MyInvocation.MyCommand.Name) -Task {
        $settingsFile = Join-Path $Config.DataImportWebSettingsPath "appsettings.json"
        $settings = Get-Content $settingsFile | ConvertFrom-Json | ConvertTo-Hashtable
        $settings.AppSettings.DatabaseEngine = $config.engine

        if(!$settings.AppSettings.EncryptionKey)
        {
            if($Config.ContainsKey("EncryptionKey") -AND $Config.EncryptionKey)
            {
                $encryptionKey = $Config.EncryptionKey
            }
            else {
                $encryptionKey = New-AESKey
            }
            $settings.AppSettings.EncryptionKey = $encryptionKey
        }
        $settings.AppSettings.UserRecoveryToken = $Config.UserRecoveryToken
        $EmptyHashTable=@{}
        $mergedSettings = Merge-Hashtables $settings, $EmptyHashTable
        New-JsonFile $settingsFile $mergedSettings -Overwrite
    }
}

function Invoke-TransformDataImportTransformLoadAppSettings {
    [CmdletBinding()]
    param (
        [hashtable]
        [Parameter(Mandatory=$true)]
        $Config
    )

    Invoke-Task -Name ($MyInvocation.MyCommand.Name) -Task {
        $settingsFile = Join-Path $Config.DataImportTransformLoadSettingsPath "appsettings.json"
        $settings = Get-Content $settingsFile | ConvertFrom-Json | ConvertTo-Hashtable
        $settings.AppSettings.DatabaseEngine = $config.engine

        if(!$settings.AppSettings.EncryptionKey)
        {
            if($Config.ContainsKey("EncryptionKey") -AND $Config.EncryptionKey)
            {
                $encryptionKey = $Config.EncryptionKey
            }
            else {
                $encryptionKey = New-AESKey
            }
            $settings.AppSettings.EncryptionKey = $encryptionKey
        }

        $EmptyHashTable=@{}
        $mergedSettings = Merge-Hashtables $settings, $EmptyHashTable
        New-JsonFile $settingsFile $mergedSettings -Overwrite
    }
}

function Invoke-TransformDataImportWebConnectionStrings {
    [CmdletBinding()]
    param (
        [hashtable]
        [Parameter(Mandatory=$true)]
        $Config
    )
    Invoke-Task -Name ($MyInvocation.MyCommand.Name) -Task {
        if ($Config.hasDbConnectionInfo -and (-not $Config.DbConnectionInfo.DatabaseName)) {
            $Config.DbConnectionInfo.DatabaseName = $Config.DataImportDatabaseName
        }

        $settingsFile = Join-Path $Config.DataImportWebSettingsPath  "appsettings.json"
        $settings = Get-Content $settingsFile | ConvertFrom-Json | ConvertTo-Hashtable

        Write-Host "Setting database connections in $($Config.DataImportWebSettingsPath)"
        $connString = New-ConnectionString -ConnectionInfo $Config.DbConnectionInfo -SspiUsername $Config.WebApplicationName

        $connectionstrings = @{
            ConnectionStrings = @{
                defaultConnection = $connString
            }
        }

        $mergedSettings = Merge-Hashtables $settings, $connectionstrings
        New-JsonFile $settingsFile  $mergedSettings -Overwrite
    }
}
function Invoke-TransformDataImportTransformLoadConnectionStrings {
    [CmdletBinding()]
    param (
        [hashtable]
        [Parameter(Mandatory=$true)]
        $Config
    )
    Invoke-Task -Name ($MyInvocation.MyCommand.Name) -Task {
        if ($Config.hasDbConnectionInfo -and (-not $Config.DbConnectionInfo.DatabaseName)) {
            $Config.DbConnectionInfo.DatabaseName = $Config.DataImportDatabaseName
        }

        $settingsFile = Join-Path $Config.DataImportTransformLoadSettingsPath  "appsettings.json"
        $settings = Get-Content $settingsFile | ConvertFrom-Json | ConvertTo-Hashtable

        Write-Host "Setting database connections in $($Config.DataImportTransformLoadSettingsPath)"
        $connString = New-ConnectionString -ConnectionInfo $Config.DbConnectionInfo -SspiUsername $Config.WebApplicationName

        $connectionstrings = @{
            ConnectionStrings = @{
                defaultConnection = $connString
            }
        }

        $mergedSettings = Merge-Hashtables $settings, $connectionstrings
        New-JsonFile $settingsFile  $mergedSettings -Overwrite
    }
}

function Request-Information {
  [CmdletBinding()]
  param (
      [Parameter(Mandatory=$true)]
      $Prompt,
      [Parameter(Mandatory=$true)]
      $DefaultValue
  )

  $isInteractive = [Environment]::UserInteractive
  if($isInteractive) {
      $confirmation = Read-Host -Prompt $Prompt
  } else {
      $confirmation = $DefaultValue
  }

  return $confirmation
}

function Install-Application {
    [CmdletBinding()]
    param (
        [hashtable]
        [Parameter(Mandatory=$true)]
        $Config
    )

    Invoke-Task -Name ($MyInvocation.MyCommand.Name) -Task {

        $iisParams = @{
            SourceLocation = $Config.PackageDirectory
            WebApplicationPath = $Config.WebApplicationPath
            WebApplicationName = $Config.WebApplicationName
            WebSitePath = $Config.WebSitePath
            WebSitePort = $Config.WebSitePort
            WebSiteName = $Config.WebSiteName
            CertThumbprint = $Config.CertThumbprint
            DotNetVersion = $RequiredDotNetHostingBundleVersion
        }

        Install-EdFiApplicationIntoIIS @iisParams
        
        $parameters = @{
            sourceLocation = "$($Config.DataImportTransformLoadSettingsPath)\*"
            installLocation = $Config.TransformLoadApplicationPath
        }
        Copy-TransformLoadToWebsitePath @parameters
    }
}

function Copy-TransformLoadToWebsitePath {
    param (
        [string] [Parameter(Mandatory=$true)] $sourceLocation,
        [string] [Parameter(Mandatory=$true)] $installLocation
    )

    New-Item -ItemType Directory -Path $installLocation -Force -ErrorAction Stop | Out-Null
    
    Write-info "Copying folder: ""$sourceLocation"" to destination: ""$installLocation"""
    $parameters = @{
        Path = $sourceLocation
        Recurse = $true
        Exclude = @(
            "*.nupkg"
            "Web.*.config"
        )
        Destination = $installLocation
        Force = $true
    }

    Copy-Item @parameters
}

function Set-SqlLogins {
    [CmdletBinding()]
    param (
        [hashtable]
        [Parameter(Mandatory=$true)]
        $Config
    )

    Invoke-Task -Name ($MyInvocation.MyCommand.Name) -Task {

        if($Config.hasDbConnectionInfo)
        {
            Add-SqlLogins $Config.DbConnectionInfo $Config.WebApplicationName -IsCustomLogin
        }
    }
}

Export-ModuleMember -Function Install-EdFiDataImport, Uninstall-EdFiDataImport
