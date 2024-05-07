# Data Import

## Overview

`Data Import` is a tool to simplify the loading of CSV data to the Operational Data Store (ODS) of the [Ed-Fi ODS / API](https://techdocs.ed-fi.org/pages/viewpage.action?pageId=95259668). The import handles domains where vendor integration to the Ed-Fi APIs is inchoate or nonexistent from legacy data sources such as state assessment systems. The system works by providing methods to extract information out of spreadsheet-based CSV data files, and transform and load to the Ed-Fi ODS / API.

Data Import is designed to match the Ed-Fi ODS / API operating model of choice by education-serving entities. The Data Import solution is intended to be used by system IT administrators and technical data analysts, in service of Local Education Agency (LEA) and State Education Agency (SEA) needs where directly integrated API solutions do not exist.

[![OpenSSF Scorecard](https://api.securityscorecards.dev/projects/github.com/Ed-Fi-Alliance-OSS/Ed-Fi-DataImport/badge)](https://securityscorecards.dev/viewer/?uri=github.com/Ed-Fi-Alliance-OSS/Ed-Fi-DataImport)

### Features

* Obtain CSVs via SFTP, FTPS, web site, or manual upload
* Map CSVs to Ed-Fi API endpoints and map CSV columns to Ed-Fi attributes
* Populate CSV row data into an Ed-Fi ODS / API based on mappings
* View import job status and other details
* Template Sharing
  * Data Import allows users to define templates, which map data sources to an Ed-Fi ODS / API. These configuration templates contain metadata information for the mapping of CSV files to Ed-Fi API endpoint(s), value replacement lookup tables, and Ed-Fi API bootstrap data necessary before import jobs.
  * Import or Export template files are shared on the [DataImport-Templates](https://github.com/Ed-Fi-Exchange-OSS/DataImport-Templates) repository in the Ed-Fi Exchange GitHub. More information about usage and sharing can be found there.

### Project Overview

Data Import is a multi-project C# .NET solution with a web administration panel in ASP.NET to view data and job status, and server components as .NET command-line applications to process data. Data Import is designed to run on-premises or within Docker containers. 

* Please see [Build Script Documentation](docs/build-script.md) to setup Data Import locally.
* Please see [Docker Documentation](#docker-deployment) to setup Data Import within Docker Containers.

## Installation Requirements

### Prerequisites

Data Import 2.3+ requires **.NET 8**

Additionally, familiarity with the following technologies are required for installing and configuring Data Import:

* PowerShell
* Microsoft SQL Server 2016 or higher
* Postgres 11 or higher database server
* SQL Server Management Studio (SSMS)

### Compatibility & Supported ODS / API Versions

Data Import is designed for use with the `Ed-Fi ODS / API v3.1+`. Data Import can be installed either alongside your Ed-Fi ODS / API server or used as a standalone application. Additionally, the Ed-Fi ODS / API instance must be reachable from the network on which the Data Import tool will be running.

See the [Ed-Fi Operational Data Store and API](https://techdocs.ed-fi.org/display/ETKB/Ed-Fi+Operational+Data+Store+and+API) and [Ed-Fi Technology Version Index](https://techdocs.ed-fi.org/display/ETKB/Ed-Fi+Technology+Version+Index) sections for more details on the ODS / API and version compatibility.

### The following are functional requirements to use Data Import:

* An API key and secret is needed with access permissions to create data for targeted entities. Please see [How To: View Security Configuration Details](https://techdocs.ed-fi.org/display/ODSAPIS3V53/How+To%3A+View+Security+Configuration+Details) and [How To: Configure Claim Sets](https://techdocs.ed-fi.org/display/ODSAPIS3V53/How+To%3A+Configure+Claim+Sets) for more information on managing security configuration and access permissions via claim sets.

* A SQL login for Data Import to use. This login can use either Windows Authentication or SQL Authentication, and will be provided during installation of Data Import. The login must have the `dbcreator` role assigned.

## Installation

Data Import supports 2 methods of installation: PowerShell scripts and Docker.

### Powershell Installation

PowerShell installation provides a convenient method for installing Data Import using PowerShell scripts and a simple configuration file.

* For installation instructions, see [PowerShell Installation for Data Import using NuGet Packages](docs/powershell-installation.md) based on the version you are installing.

### Docker Deployment

**Only Data Import 1.3 and above support deployment using Docker.**

* For general Docker Deployment information, see [Docker Deployment for Data Import](https://techdocs.ed-fi.org/display/EDFITOOLS/Docker+Deployment+for+Data+Import)

* For simple, out-of-the-box deployment of Data Import, see [Docker DI - Quick Start](https://techdocs.ed-fi.org/display/EDFITOOLS/Quick+Start+for+Data+Import+in+Docker)

> For additional information and detailed steps, please refer the [Docker Deployment documentation](https://techdocs.ed-fi.org/display/EDFITOOLS/Docker+Deployment+for+Data+Import)

## First-Time Configuration

For information on post-installation Data Import configuration process, see [First-Time Data Import Configuration](docs/first-time-configuration.md)

## Quick Start

Please refer the [Quick Start guide](https://techdocs.ed-fi.org/display/EDFITOOLS/Quick+Start) to verify installation and perform a simple end-to-end import using an Ed-Fi ODS / API v3.2 and the Grand Bend sample data set.

## Documentation

For detailed documentation, please see the [Data Import Tech Docs](https://techdocs.ed-fi.org/display/EDFITOOLS/Data+Import).

## Contributing

The Ed-Fi Alliance welcomes code contributions from the community. Please read
the [Ed-Fi Contribution
Guidelines](https://techdocs.ed-fi.org/display/ETKB/Code+Contribution+Guidelines)
for detailed information on how to contribute source code.

## License

Copyright (c) 2024 Ed-Fi Alliance, LLC and contributors.

Licensed under the Apache License, Version 2.0 (the "License").

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

See NOTICES for additional copyright and license notifications.
