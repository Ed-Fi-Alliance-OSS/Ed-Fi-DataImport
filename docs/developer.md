# Developer documentation for Data Import

## How to generate a new database migration

1. Open the Package Manager Console window.
2. On that Console window select the **DataImport.Models** project.
3. Then you need to execute 2 commands, one for SQL Server and another one for Postgres

   **SQL Server**

        `Add-Migration <MigrationName> -Context SqlDataImportDbContext -OutputDir ./Migrations/SqlServer -verbose -Args "<ConnectionString> SqlServer"`

   **Postgres**

        `Add-Migration <MigrationName> -Context PostgreSqlDataImportDbContext -OutputDir ./Migrations/PostgreSql -verbose -Args "<ConnectionString> PostgreSql"`

4. Replace MigrationName with an appropriate one.
5. Replace ConnectionString a valid connection string for both engines.

   Ejm for SQL Server:

        `DataSource=(local);InitialCatalog=EdFi_DataImport;Trusted_Connection=True;`

   Ejm for Postgres:

        `Host=localhost;Port=5432;Database=EdFi_DataImport;username=postgres;Password=password;`
