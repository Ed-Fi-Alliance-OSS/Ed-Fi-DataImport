# Developer documentation for Data Import

## How to generate a new database migration

1. Open the Package Manager Console window.
2. On that Console window select the **DataImport.Models** project.
3. Then you need to execute 2 commands, one for SQL Server and another one for Postgres

   **SQL Server**

        Add-Migration <MigrationName> -Context SqlDataImportDbContext -OutputDir ./Migrations/SqlServer -verbose -Args "<ConnectionString> SqlServer"

   **Postgres**

        Add-Migration <MigrationName> -Context PostgreSqlDataImportDbContext -OutputDir ./Migrations/PostgreSql -verbose -Args "<ConnectionString> PostgreSql"

4. Replace MigrationName with an appropriate one.
5. Replace ConnectionString with valid one for both engines.

   Example for SQL Server:

        DataSource=(local);InitialCatalog=EdFi_DataImport;Trusted_Connection=True;
        Server=(local);Database=EdFi_DataImport;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;

   Example for Postgres:

        Host=localhost;Port=5432;Database=EdFi_DataImport;username=postgres;Password=password;

## How to update database

To update the database you need to follow similar steps. The only difference is that the Command changes a little bit

   **SQL Server**

        Update-Database -Context SqlDataImportDbContext  -verbose -Args "<ConnectionString> SqlServer"

   **Postgres**

        Update-Database -Context PostgreSqlDataImportDbContext  -verbose -Args "<ConnectionString> PostgreSql"

## Skipping certificate validation for SSL Connections

In order to be able to skip the certificate validation for local development we have added a flag with name `IgnoresCertificateErrors` to the Web application settings and to the Transform Load settings. Since these applications work independently, it needed to be added on both.
Change its value to true, when you are using a self signed certificate or any other not valid one. Otherwise the application will throw errors, for example, when trying to create a new connection to the ODS API, or when trying to transform and load the data.
Consider security risks, this value must not be set to true on production environments.
