# Docker Deployment for Data Import

The Docker Deployment for Data Import provides deployment scripts to install Data Import within a Docker-based environment.

## Prerequisites

In addition to the prerequisites mentioned [here](../README.md#prerequisites), following requirements are additionaly needed to run Docker Deployment for Data Import:

* Access to a Docker environment. The instructions below use [Docker Desktop](https://www.docker.com/products/docker-desktop/) for workstation usage.
* An execution shell, such as PowerShell or bash using WSL, Mac OS or Linux.

## Docker within Non-Developer Use Cases (Production, Staging, etc.)

Release Images for Data Import are available at: https://hub.docker.com/r/edfialliance/data-import.

## Docker within Developer Use Cases

Example [Docker Compose file](../Compose/pgsql/compose-build.yml) and accompanying [.env](../Compose/pgsql/.env.example) files are available in the repository. It includes services for the Data Import application, backing `PostgreSQL` database and `PgBouncer` connection pool.

### Using Dockerfile

By default, the example compose-file is configured to use the [Dockerfile](../Dockerfile) in the repository root. You can modify the Dockerfile to pull different versions of the Data Import package by specifying the `VERSION` environment variable in the Dockerfile.

> Only Data Import 1.3 and above support deployment using Docker.

### Using dev.Dockerfile

By default, the example compose-file is configured to use the [Dockerfile](../Dockerfile) in the repository root. However, if you wish to build a local Data Import image containing local changes, you can modify the  compose-file to use [dev.Dockerfile](../dev.Dockerfile) as follows:

```yaml
...
services:
  dataimport:
    build:
      context: ../../
      dockerfile: dev.Dockerfile
...
```

### Using a Released Data Import image

You can use a released Data Import image by modifying the example compose file to use the released image as follows:

```yaml
...
services:
  dataimport:  
    image: edfialliance/data-import:v1.3.2
...
```

## Running alongside ODS/API solution

It is possible to run Data Import alongside an ODS/API solution in the following ways:

* Running this composition wholesale alongside an ODS/API solution deployed using Docker or other methods
* Adding the `dataimport` services and volumes to an existing ODS/API Docker composition and redeploying using the dataimport service as an example for plugging in to a composition with an existing database, updating the `POSTGRES_HOST` and other DB settings accordingly

Please see [Docker DI - Quick Start](https://techdocs.ed-fi.org/display/EDFITOOLS/Quick+Start+for+Data+Import+in+Docker) for further information.

## Using Databases (other than PostgreSQL)

### Modifying PostgreSQL

The default configurations provided are designed to be used with a dedicated  PostgreSQL server and connection pool as containers. In order to connect to a different PostgreSQL instance or otherwise modify that setup, simply change the `POSTGRES_` variables to match your environment.

> If you are not using the postgres  or pgbouncer  services, make sure to remove them from your orchestration.

### Using MSSQL Database Server

In order to connect to a Microsoft SQL Server instance you must have a valid SQL username and password. It is possible to leverage a Docker container or an instance installed on the host or elsewhere, but if using the latter options, consider the host/container networking relationship.

* Change `DATABASE_ENGINE` on .env file to `SqlServer` 
* Remove `POSTGRES_` environment settings
* Update `CONNECTIONSTRINGS__DEFAULTCONNECTION` to a valid connection string
* SQL username/password must be used to connect, as opposed to Integrated Security

## Upgrading Data Import

### From an existing Non-Docker Deployment

> **Database Migration:** Migrating the Data Import database from outside of Docker into a Docker container is not supported, as the ODS/API connection information will not be functional and schemas may differ between MSSQL Server and PostgreSQL. If this is a necessity, you may perform such a migration yourself manually and correct any errors that occur. &nbsp;
>

Alternatively, you may export relevant Data Maps, Bootstraps, and Preprocessors to files and re-import them on the new installation, or re-load from Template Sharing and adjust them as needed. In this case, you will need to re-establish ODS/API connections and configure Agents to match your previous installation. Consider completing the Docker installation and configuration before uninstalling the previous version to verify the setups match.

To upgrade from an existing Non-Docker Data Import installation, execute the following steps:

* If you are using the same database, **please make a backup of the Data Import database**, for safety. The installer is capable of automatically upgrading the content of the existing database, so the uninstall/install process can use the same database.
* **Make a backup of the Data Import configuration files for any values you may have set here.** 

> NOTE: Ensure you save the `EncryptionKey` value which appears the same in both files. Copy this as it will be re-used in the new Data Import installation.
> The files to check differ for versions less than 1.3.0:
>
> * 1.2.0: The web application `Web.config` and the Transform/Load application's `DataImport.Server.TransformLoad.exe.config`
> * 1.3.0+: The web application `appsettings.json` and the Transform/Load application's `appsettings.json`

* Stop the previous Data Import application and website from Internet Information Server
* Run the Docker Installation
  * Update configuration values to match those copied above
  * Verify the website is running correctly in Docker
* Manually delete the previous Data Import application, website, and app pools from Internet Information Server.

### From an existing Docker Deployment

To upgrade from an existing Docker deployment:

* **Make a backup of the Data Import database, for safety.** The installer is capable of automatically upgrading the content of the existing database, so the uninstall/install process can use the same database.
* **Update the image tag** for the Data Import service in your composition to the new version
* **Note and update** of any new environment variables which may need configured and that your current environment variables have not changed pay close attention that the `ENCRYPTION_KEY` setting does not change
* Redeploy the docker composition
