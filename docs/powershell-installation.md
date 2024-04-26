# PowerShell Installation for Data Import using NuGet Packages

## Prerequisites

In addition to the prerequisites mentioned [here](../README.md#prerequisites), following requirements are additionally needed to install Data Import using the Powershell installer:

* Windows environment using Internet Information Server (IIS)

* The **.NET 8** is required on the destination server before installation of Data Import. After installing .NET, it is necessary to restart the computer for the changes to take effect.

* The IIS Server Role or Windows Feature must be enabled.

### Download and Install NuGet CLI

NuGet CLI is recommended to simplify the process of downloading and installing packages with the Powershell installer. The instructions to download NuGet CLI are available [here](https://learn.microsoft.com/en-us/nuget/reference/nuget-exe-cli-reference#installing-nugetexe).

## Installation using Data Import Installer NuGet package

### Download the Data Import Installer Package

* The latest released Data Import installer package can be found [here](https://dev.azure.com/ed-fi-alliance/Ed-Fi-Alliance-OSS/_artifacts/feed/EdFi@Release/NuGet/EdFi.DataImport.Installation/overview).

> **NOTE:** After downloading the package. Right-click and select "Properties." Update the file extension (from .nupkg to .zip). Remove the version number (optional). Check the box next to Unblock (this will prevent PowerShell from asking for permission to load every module in the installer) and click OK.

* The following Powershell snippet can also be used to download and extract the latest released installer package (**NuGet CLI required**)

```powershell
$pathToNuget = "Path\To\NuGet.exe"
$pathToOutputDirectory = "Path\To\Output\Directory"
$releaseVersion = "1.3.2"
$parameters = @(
    "install", "EdFi.DataImport.Installation",
    "-source", "https://pkgs.dev.azure.com/ed-fi-alliance/Ed-Fi-Alliance-OSS/_packaging/EdFi/nuget/v3/index.json",
    "-outputDirectory", $pathToOutputDirectory
    "-version", $releaseVersion
 )
& $pathToNuget $parameters
Rename-Item -Path "$pathToOutputDirectory\EdFi.DataImport.Installation.$releaseVersion" -NewName "EdFi.DataImport.Installation"
```

> **NOTE:** Substitute `Path\To\Nuget.exe` in `$pathToNuget` and `Path\To\Output\Directory` in `$pathToOutputDirectory` with the path to nuget.exe and the preferred output directory. Ensure that the package version is the current release version.

## Installation Configuration

> **NOTE:** You may need to configure TLS while running the installation scripts described in steps below.
`[Net.ServicePointManager]::SecurityProtocol += [Net.SecurityProtocolType]::Tls12`

> **NOTE:** The paths in these instructions assume that the package was extracted to a folder with the name of the package (e.g., C:\temp\EdFi.DataImport.Installation).

* Edit `install.ps1` to supply your configuration values. Default values are assumed if a value for a parameter is not configured.

* Configure `$dbConnectionInfo` (database parameters) by consulting the following table:

| Configuration   |  Description          |  Value |
|----------|:--------------|:-------|
| **Server** | Database Server name | For a local server, we can use `(local)` for SQL Server and `localhost` for PostgreSQL  |
| **Engine** | Database engine (SQL and PostgreSQL are supported) | Valid values: `SQLServer` and `PostgreSQL` |
| **UseIntegratedSecurity** | For Windows authentication, use `$true`. For SQL Server/ PostgreSQL server authentication, use `$false` and a valid `Username` and `Password` must be provided. | Valid values: `$true` or `$false` |
| **Username** (Optional) | Username to connect to the database | |
| **Password** (Optional) | Password to connect to the database | |
| **Port** (Optional) | Database server port, presuming the server is configured to use a specific port. | |

* Configure `$p` (installation process parameters) by consulting the following table:

| Configuration   |  Description          |  Value |
|----------|:--------------|:-------|
| **ToolsPath** |  Path for storing installation tools | Default: `C:/temp/tools`|
| **DbConnectionInfo** (Leave unedited) | Database Parameters | Already provided above |
| **DataImportDatabaseName** | Data Import database name | Default: `EdFi_DataImport` |
| **WebSitePath** (Optional) | Path for installing Web and TransformLoad files | Default: `C:\inetpub\Ed-Fi` |
| **WebApplicationName** (Optional) | Data Import Web Application Name | Default: `DataImport` |
| **TransformLoadApplicationName** (Optional) | Data Import Transform Load Application Name | Default: `DataImportTransformLoad` |
| **WebSitePort** (Optional) | Data Import Web Site Port | Default: `444` |
| **WebsiteName** (Optional) | Data Import Web Site Name | Default: `Ed-Fi` |
| **PackageVersion** (Optional) | Preconfigured with installer version downloaded | If not set, will retrieve the latest full release package |

### Minimal configuration examples

**SQL Server**

```powershell
$dbConnectionInfo = @{
    Server = "(local)"
    Engine = "SqlServer"
    UseIntegratedSecurity = $true
}
 
$p = @{
    DbConnectionInfo = $dbConnectionInfo
}
```

**PostgreSQL**

```powershell
$dbConnectionInfo = @{
    Server = "localhost"
    Engine = "PostgreSQL"
    Username = "exampleAdmin"
    Password = "examplePassword"
}
 
$parameters = @{
    DbConnectionInfo = $dbConnectionInfo
}
```

## Run the installation via Powershell

> **NOTE:** [Ensure execution permissions](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_execution_policies?view=powershell-7.2) for the PowerShell scripts.

* Launch Powershell **as an administrator**
* `cd` to the directory containing the installation files
* Run the installation script `./install.ps1`

### Database login setup (Integrated Security mode)

When prompted for database login details during the installation process, enter `Y` to continue with the default option or enter `N` to enter windows username.

**The default option creates `IIS APPPOOL\DataImport` database login on the server.**

**The windows username option will validate and create a database login using the entered usernmae if the lofin does not exist on the server already.**

### Ensuring SQL Server Login Exists

 The installation process sets up an appropriate SQL Login for use with the dedicated Data Import Application Pool in IIS. You can verify this in SQL Server Management Studio.
 However, if you wish to set up your login differently, some basic guidance on how to set this up is explained in [this blog post](https://learn.microsoft.com/en-us/archive/blogs/ericparvin/how-to-add-the-applicationpoolidentity-to-a-sql-server-login). 

 > You can choose either `Windows authentication` or `SQL Server authentication`.
 > If you choose SQL Server authentication,
 > * Ensure `"User must change password at next login"` is checked, you must connect to SSMS with those credentials to reset the password. Otherwise, the app pools won't be able to connect.
 > * On the Server Roles page, ensure that at least the `"dbcreator"` checkbox is checked, as `Entity Framework` will create the database when the application is launched. Once you have confirmed a proper SQL Server login exists, continue to the next step. 

> Please refer Step 5 [here](https://techdocs.ed-fi.org/display/EDFITOOLS/PowerShell+Installation+for+Data+Import+using+NuGet+Packages) for screenshots.

### Update Application Pool Identity (Optional)

**Skip this step if you would like to use the default App Pool Identity**

As the previous step sets up an appropriate SQL Login for use with the dedicated Data Import Application Pool in IIS.
If you don't wish to use the default App Pool identity, you can set up a custom one. 
* In IIS Manager, click on the browse icon under `Process Model > Identity` in the `Advanced Settings` for the App Pool.
* Choose the custom account option and click `Set...`. When setting the credentials, you can just use the username and password that you use to log in to Windows.
* If you need to include the app pool domain in the username, an example username would like: `"localhost\username"`, where `localhost` is the app pool domain.
* After entering the correct credentials, click OK on all screens until you're back to the main Application Pools page.

> Please refer Step 6 [here](https://techdocs.ed-fi.org/display/EDFITOOLS/PowerShell+Installation+for+Data+Import+using+NuGet+Packages) for screenshots.

### Confirm Installation in IIS Manager

Open IIS Manager, and confirm a Data Import app exists under the Ed-Fi site. You should also see the location of the Transform Load executable registered.

## Launch the Application

In IIS Manager, click on the DataImport app and select Browse application to launch in the browser. The initial installation will take a minute since the database is being created.

## Upgrading

To upgrade from a prior Data Import release, please execute the following steps:

* **Make a backup of the Data Import database, for safety.** The installer is capable of automatically upgrading the content of the existing database, so the uninstall/install process can use the same database.
* **Make a backup of the Data Import configuration files for any values you may have set here.**

> NOTE: Ensure you save the `EncryptionKey` value which appears the same in both files( Web application "appsettings.json" and the Transform/Load application's "appsettings.json" ). Copy this as it will be re-used in the new Data Import installation

* Run [Uninstall Instructions](#uninstalling).

> This will remove configuration and source files for Data Import, however will not delete the existing database which can continue to be used with the new Data Import installation.

* Rename the `C:\Ed-Fi\Data Import` directory to `C:\Ed-Fi\Data Import-prior` and the `C:\Ed-Fi\DataImportInstallation` directory to `C:\Ed-Fi\DataImportInstallation-prior`.
* Run through the [installation instructions](#installation-using-data-import-installer-nuget-package). While configuring, please ensure the existing database connection string and encryption key are used in this Data Import configuration.
* After installation is complete, verify any custom settings from your original backups of the config files are reapplied in your new config files, **especially your EncryptionKey.**
* Restart IIS and reload Data Import at `https://<servername>:444/DataImport`

## Uninstalling

* Launch PowerShell as an administrator 
* `cd` to the directory containing the installation files
* Run the `uninstall.ps1` script.
* In the directory with the Data Import source files (`C:\Ed-Fi\Data Import` in our case), delete all folders and files to be at a clean state.
