# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

#tag aspnet:10.0.6-alpine3.23
FROM mcr.microsoft.com/dotnet/aspnet@sha256:1201dde897ab436b7c6b386f6dbd4f9a3ca0245f9c5a8aac8f8bcdccb4c7d484
LABEL maintainer="Ed-Fi Alliance, LLC and Contributors <techsupport@ed-fi.org>"
ARG TIME_ZONE=US/Central
ENV VERSION="2.4.0"
ENV TZ=${TIME_ZONE}

# Alpine image does not contain Globalization Cultures library so we need to install ICU library to get for LINQ expression to work
# Disable the globalization invariant mode (set in base image)
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

WORKDIR /app

RUN apk --no-cache add unzip=~6 dos2unix=~7 bash=~5 gettext=~0 postgresql16-client=~16 jq=~1 icu=~76 gcompat=~1 tzdata=~2026a && \
    wget -O /app/DataImport.zip https://pkgs.dev.azure.com/ed-fi-alliance/Ed-Fi-Alliance-OSS/_apis/packaging/feeds/EdFi/nuget/packages/DataImport.Web/versions/${VERSION}/content && \
    unzip /app/DataImport.zip -d /app/DataImport && \
    cp -r /app/DataImport/DataImport.Web/. /app/DataImport.Web && \
    cp -r /app/DataImport/DataImport.Server.TransformLoad/. /app/DataImport.Server.TransformLoad && \
    chmod 755 /app/DataImport.Server.TransformLoad/DataImport.Server.TransformLoad -- ** && \
    rm -r /app/DataImport && \
    rm -f /app/DataImport.zip

COPY Compose/pgsql/run.sh /app/DataImport.Web/run.sh
RUN dos2unix /app/DataImport.Web/*.json && \
    dos2unix /app/DataImport.Web/*.sh && \
    dos2unix /app/DataImport.Server.TransformLoad/*.json && \
    chmod 700 /app/DataImport.Web/*.sh -- **

EXPOSE 80

WORKDIR /app/DataImport.Web
ENTRYPOINT [ "/app/DataImport.Web/run.sh" ]
