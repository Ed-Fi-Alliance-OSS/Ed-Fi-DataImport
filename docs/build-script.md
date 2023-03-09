# Build Script for Data Import

## Development Prerequisites

* [.NET Core 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
* Either:
  * [Visual Studio 2022](https://visualstudio.microsoft.com/downloads), or
  * [Visual Studio 2022 Build Tools](https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022) (install the ".NET Build Tools" component)
* Clone this repository locally

## Build Script

The PowerShell script [`build.ps1`](../build.ps1) contains functions for running standard build operations at the command line. This script assumes that .NET 6.0 SDK or newer is installed. Other dependencies tools are downloaded as needed (nuget, nunit).

Available commands:

* `.\build.ps1 setup` installs `python` and `nugetCLI`
* `.\build.ps1 clean` runs the MSBuild `clean` task
* `.\build.ps1 build` runs the MSBuild `build` task with several implicit steps,
  including NuGet restore and temporary injection of version numbers into the
  AssemblyInfo.cs files.
* `.\build.ps1 unittest` executes NUnit tests in projects named `*.UnitTest`,
  which do not connect to a database.
* `.\build.ps1 integrationtest` executes NUnit tests in projects named `*.Test`,
  which connect to a database.
* `.\build.ps1 powershelltests` executes  NUnit Powershell Category tests in projects named `*.Test` which connect to a database.
* `.\build.ps1 buildandtest` executes the Build, UnitTest, IntegrationTest
  and PowerShellTests commands.
* `.\build.ps1 package` builds NuGet package for the Data Import web application, installer and Data Import Transform Load application as single package.
* `.\build.ps1 push` uploads a NuGet package to the NuGet feed.
* `.\build.ps1 pushprerelease` uploads a pre-release NuGet package to the NuGet feed.
* `.\build.ps1 run` launches the Data Import from the build script. The LaunchProfile parameter is required for running Data Import. Valid values include 'mssql' and 'pg'.

Review the parameters at the top of `build.ps1` for additional commands and command line arguments.