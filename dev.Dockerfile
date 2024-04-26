# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

# tag sdk:8.0 alpine
FROM mcr.microsoft.com/dotnet/sdk@sha256:e646d8a0fa589bcd970e0ebde394780398e8ae08fffeb36781753c51fc9e87b0 AS build
WORKDIR /source

COPY DataImport.Web/*.csproj DataImport.Web/
COPY DataImport.Server.TransformLoad/*.csproj DataImport.Server.TransformLoad/
COPY DataImport.Models/*.csproj DataImport.Models/
COPY DataImport.Common/*.csproj DataImport.Common/
COPY DataImport.EdFi/*.csproj DataImport.EdFi/
COPY logging.json logging.json
COPY logging_PgSql.json logging_PgSql.json
COPY logging_Sql.json logging_Sql.json
RUN dotnet restore DataImport.Server.TransformLoad/DataImport.Server.TransformLoad.csproj
RUN dotnet restore DataImport.Web/DataImport.Web.csproj

COPY DataImport.Web/ DataImport.Web/
COPY DataImport.Server.TransformLoad/ DataImport.Server.TransformLoad/
COPY DataImport.Models/ DataImport.Models/
COPY DataImport.Common/ DataImport.Common/
COPY DataImport.EdFi/ DataImport.EdFi/

WORKDIR /source/DataImport.Web
RUN dotnet build -c Release --no-restore
FROM build AS publish
RUN dotnet publish -c Release --no-build -o /app/DataImport.Web

WORKDIR /source/DataImport.Server.TransformLoad
RUN dotnet build -c Release --no-restore
RUN dotnet publish -c Release --no-build -o /app/DataImport.Server.TransformLoad

#tag 8.0-alpine
FROM mcr.microsoft.com/dotnet/aspnet@sha256:646b1c5ff36375f35f6149b0ce19ca095f97b4b882b90652801e9fbe82bcfa8a
LABEL maintainer="Ed-Fi Alliance, LLC and Contributors <techsupport@ed-fi.org>"
# Alpine image does not contain Globalization Cultures library so we need to install ICU library to get for LINQ expression to work
# Disable the globaliztion invariant mode (set in base image)
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV ASPNETCORE_ENVIRONMENT Development

WORKDIR /app
ENV TZ=US/Central
RUN apk add --no-cache icu=~74 tzdata

WORKDIR /app/DataImport.Web
COPY --from=publish /app/DataImport.Web .

WORKDIR /app/DataImport.Server.TransformLoad
COPY --from=publish /app/DataImport.Server.TransformLoad .

EXPOSE 80
WORKDIR /app/DataImport.Web
ENTRYPOINT ["dotnet", "DataImport.Web.dll"]
